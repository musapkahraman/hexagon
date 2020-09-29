using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HexagonGame.Gestures;
using HexagonGame.GridMap;
using HexagonGame.UI;
using UnityEngine;

namespace HexagonGame.Core
{
    public class HexagonGroupController : MonoBehaviour
    {
        private const int RotationPeriodCount = 3;
        private const float TileSpeed = 0.2f;
        [SerializeField] private int scoreMultiplier = 5;
        [SerializeField] private int bombScoreInterval = 50;
        [SerializeField] private DynamicData score;
        [SerializeField] private DynamicData move;
        [SerializeField] private DynamicData highScore;
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private BombTimerController bombTimerController;
        [SerializeField] private GameOverMessage gameOverMessage;
        [SerializeField] private HexagonSpritePool spritePool;
        private readonly HashSet<Vector3Int> _descendingTiles = new HashSet<Vector3Int>();
        private Grid _grid;
        private GridBuilder _gridBuilder;
        private bool _isAlreadyRotating;
        private Transform _rotatingParent;
        private int _rotationCheckCounter;
        private bool _shouldPlaceBomb;

        private void Awake()
        {
            _grid = GetComponent<Grid>();
            _gridBuilder = GetComponent<GridBuilder>();
        }

        public void ShowRotatingGroupAtCenter(Vector3 center, List<PlacedHexagon> neighbors)
        {
            if (_isAlreadyRotating) return;
            bombTimerController.UnHookTransform();
            _rotatingParent = spritePool.GetRotatingParent(center, neighbors);
        }

        public void RotateSelectedHexagonGroup(RotationDirection direction)
        {
            if (_isAlreadyRotating || _descendingTiles.Count > 0) return;
            _isAlreadyRotating = true;
            Rotate(direction);
        }

        private void Rotate(RotationDirection direction)
        {
            const int rotationPeriodAngle = 360 / RotationPeriodCount;
            const float duration = 2 * TileSpeed;
            if (!_rotatingParent) return;
            switch (direction)
            {
                case RotationDirection.Clockwise:
                    _rotatingParent.DORotate(rotationPeriodAngle * Vector3.back, duration, RotateMode.LocalAxisAdd)
                        .OnComplete(OnRotationComplete);
                    break;
                case RotationDirection.AntiClockwise:
                    _rotatingParent.DORotate(rotationPeriodAngle * Vector3.forward, duration, RotateMode.LocalAxisAdd)
                        .OnComplete(OnRotationComplete);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            void OnRotationComplete()
            {
                if (_rotationCheckCounter >= RotationPeriodCount - 1)
                {
                    ResetGateKeepers();
                }
                else
                {
                    _rotationCheckCounter++;
                    if (!IsRotatingGroupMatched(out var matchedHexagons))
                    {
                        Rotate(direction);
                    }
                    else
                    {
                        ResetGateKeepers();
                        move.IncreaseValue(1);
                        spritePool.ReturnRotatingParent();
                        ExplodeMatchedHexagons(matchedHexagons);
                    }
                }

                void ResetGateKeepers()
                {
                    _rotationCheckCounter = 0;
                    _isAlreadyRotating = false;
                }
            }
        }

        private void ExplodeMatchedHexagons(ICollection<PlacedHexagon> matchedHexagons)
        {
            AnimatePopOut(matchedHexagons);
            for (var i = 0; i < matchedHexagons.Count; i++)
            {
                score.IncreaseValue(scoreMultiplier);
                if (score.GetValue() % bombScoreInterval == 0) _shouldPlaceBomb = true;
            }

            foreach (var _ in _gridBuilder.GetPlacement().OfType<BombHexagon>()) _shouldPlaceBomb = false;
            highScore.SetMaximum(score.GetValue());
            if (!NeighborHood.IsThereAnyAvailableMovesLeft(_gridBuilder.GetPlacement(), _grid)) ExecuteGameOver();
        }

        private void ExecuteGameOver()
        {
            _isAlreadyRotating = true;
            spritePool.ReturnRotatingParent();
            gameOverMessage.Show();
            Time.timeScale = 0;
        }

        private void AnimatePopOut(ICollection<PlacedHexagon> matchedHexagons)
        {
            var sumX = 0f;
            var sumY = 0f;
            var color = Color.white;
            foreach (var hexagon in matchedHexagons)
            {
                var center = _grid.GetCellCenterWorld(hexagon.Cell);
                sumX += center.x;
                sumY += center.y;
                color = hexagon.Hexagon.color;
            }

            particles.transform.position = new Vector3(sumX / matchedHexagons.Count, sumY / matchedHexagons.Count);
            var main = particles.main;
            main.startColor = color;
            particles.Play();
        }

        private bool IsRotatingGroupMatched(out HashSet<PlacedHexagon> matchedHexagons)
        {
            var placedHexagons = _gridBuilder.GetPlacement();

            // Compare colors of each sprite with its surrounding tiles' colors.
            matchedHexagons = new HashSet<PlacedHexagon>();
            foreach (var sprite in spritePool.GetRotatingHexagonSprites())
                sprite.GetPlacedHexagon().CheckForMatch(_grid, placedHexagons, matchedHexagons);

            if (matchedHexagons.Count <= 2) return false;

            // One move is played. Decrease timer if there is a bomb.
            foreach (var placedHexagon in placedHexagons)
            {
                if (!(placedHexagon is BombHexagon bombHexagon)) continue;
                bombTimerController.Show(_grid.GetCellCenterWorld(bombHexagon.Cell));
                bombTimerController.SetTimerText(--bombHexagon.Timer);
                if (bombHexagon.Timer <= 0)
                {
                    // The bomb explodes, game is over.
                    ExecuteGameOver();
                    return false;
                }

                _shouldPlaceBomb = false;
            }

            DestroyMatchedAndFillHoles(matchedHexagons, placedHexagons);
            return true;
        }

        private void DestroyMatchedAndFillHoles(IReadOnlyCollection<PlacedHexagon> matchedHexagons,
            ICollection<PlacedHexagon> placedHexagons)
        {
            var aboveMatchedHexagons = new List<PlacedHexagon>();

            // Filter hexagons to the columns (above and including the matched hexagons)
            foreach (var hex in placedHexagons)
                aboveMatchedHexagons.AddRange(from matchedHex in matchedHexagons
                    let hexCenter = _grid.GetCellCenterWorld(hex.Cell)
                    let matchedHexCenter = _grid.GetCellCenterWorld(matchedHex.Cell)
                    let horizontalDistance = Math.Abs(hexCenter.x - matchedHexCenter.x)
                    let halfGridHeight = _grid.cellSize.y * 0.5f
                    where horizontalDistance < halfGridHeight && hexCenter.y >= matchedHexCenter.y
                    select hex);

            // Eliminate duplicates
            var set = new HashSet<PlacedHexagon>(aboveMatchedHexagons);
            aboveMatchedHexagons = new List<PlacedHexagon>(set);

            // Order by height
            aboveMatchedHexagons = aboveMatchedHexagons.OrderBy(hex => _grid.GetCellCenterWorld(hex.Cell).y).ToList();

            // Separate by columns
            var columns = new Dictionary<int, List<PlacedHexagon>>();
            foreach (var hex in aboveMatchedHexagons)
            {
                int key = hex.Cell.y;
                if (columns.ContainsKey(key)) columns[key].Add(hex);
                else columns.Add(key, new List<PlacedHexagon> {hex});
            }

            // Number of matched hexagons per column
            var matchedHexagonCountPerColumns = new Dictionary<int, int>();
            foreach (var hex in from hex in aboveMatchedHexagons
                from match in matchedHexagons
                where hex.Cell == match.Cell
                select hex)
            {
                int key = hex.Cell.y;
                if (matchedHexagonCountPerColumns.ContainsKey(key)) matchedHexagonCountPerColumns[key]++;
                else matchedHexagonCountPerColumns.Add(key, 1);
            }

            // Place masks over the hexagons above and including the matched hexagons
            var masks = new Dictionary<int, List<GameObject>>();
            foreach (var hex in aboveMatchedHexagons)
            {
                int key = hex.Cell.y;
                var mask = spritePool.GetMask(_grid.GetCellCenterWorld(hex.Cell));
                if (masks.ContainsKey(key)) masks[key].Add(mask);
                else masks.Add(key, new List<GameObject> {mask});
            }

            // Remove matched hexagons
            foreach (var hexagon in matchedHexagons)
            {
                if (hexagon is BombHexagon) bombTimerController.Hide();
                placedHexagons.Remove(hexagon);
                // Remove matched hexagons from the filter result
                aboveMatchedHexagons.RemoveAll(placedHexagon => placedHexagon.Cell == hexagon.Cell);
                foreach (var column in columns)
                    column.Value.RemoveAll(placedHexagon => placedHexagon.Cell == hexagon.Cell);
            }

            // Animate descending of the hexagons to fill the holes
            foreach (var column in columns)
            {
                if (column.Value.Count == 0) OnColumnDescended(masks[column.Key]);

                float descend = matchedHexagonCountPerColumns[column.Key] * _grid.cellSize.x;
                StartCoroutine(DescendCoroutine(column.Value, descend, masks[column.Key]));

                foreach (var placedHexagon in column.Value)
                {
                    var destination = _grid.GetCellCenterWorld(placedHexagon.Cell) + descend * Vector3.down;
                    placedHexagons.Remove(placedHexagon);
                    placedHexagon.Cell = _grid.WorldToCell(destination);
                    placedHexagons.Add(placedHexagon);
                }
            }

            _gridBuilder.SetPlacement(placedHexagons);
            var complementaryHexagons = _gridBuilder.GetComplementaryHexagons(_shouldPlaceBomb);
            foreach (var complementaryHexagon in complementaryHexagons)
            {
                if (!(complementaryHexagon is BombHexagon hex)) continue;
                bombTimerController.SetTimerText(hex.Timer);
                _shouldPlaceBomb = false;
            }
        }

        private IEnumerator DescendCoroutine(IReadOnlyList<PlacedHexagon> descendingHexagonsInTheColumn, float descend,
            List<GameObject> masksInTheColumn)
        {
            var hexagonSprites = new List<HexagonRelation>();
            for (var i = 0; i < descendingHexagonsInTheColumn.Count; i++)
                hexagonSprites.Add(spritePool.GetHexagonSprite(descendingHexagonsInTheColumn[i]));

            var counter = 0;
            for (var i = 0; i < descendingHexagonsInTheColumn.Count; i++)
            {
                var hexagon = descendingHexagonsInTheColumn[i];
                var hexagonSprite = hexagonSprites[i];
                if (hexagon is BombHexagon bombHexagon)
                {
                    bombTimerController.Show(hexagonSprite.transform);
                    bombTimerController.SetTimerText(bombHexagon.Timer);
                }

                var destination = _grid.GetCellCenterWorld(hexagon.Cell) + descend * Vector3.down;
                int maskIndex = i;
                _descendingTiles.Add(hexagonSprite.GetPlacedHexagon().Cell);
                hexagonSprite.transform.DOMove(destination, descend * TileSpeed).SetEase(Ease.InSine)
                    .OnComplete(() =>
                    {
                        _descendingTiles.Remove(hexagonSprite.GetPlacedHexagon().Cell);
                        if (hexagon is BombHexagon) bombTimerController.UnHookTransform();
                        spritePool.ReturnHexagonSprite(hexagonSprite);
                        spritePool.ReturnMask(masksInTheColumn[maskIndex]);

                        if (++counter != descendingHexagonsInTheColumn.Count) return;
                        masksInTheColumn.Reverse();
                        for (var j = 0; j < counter; j++)
                            masksInTheColumn.RemoveAt(masksInTheColumn.Count - 1);
                        OnColumnDescended(masksInTheColumn);
                    });
                yield return new WaitForSeconds(TileSpeed * 0.5f);
            }
        }

        private void OnColumnDescended(IReadOnlyList<GameObject> remainingMasks)
        {
            var counter = 0;
            float descend = remainingMasks.Count * _grid.cellSize.x;
            for (var i = 0; i < remainingMasks.Count; i++)
                foreach (var hexagon in _gridBuilder.GetPlacement())
                {
                    if (hexagon.Cell != _grid.WorldToCell(remainingMasks[i].transform.position)) continue;
                    var hexagonSprite = spritePool.GetHexagonSprite(hexagon);
                    var spriteTransform = hexagonSprite.transform;
                    var position = spriteTransform.position;
                    var destination = position;
                    position += Vector3.up * descend;
                    spriteTransform.position = position;
                    int maskIndex = i;
                    _descendingTiles.Add(hexagonSprite.GetPlacedHexagon().Cell);
                    spriteTransform.DOMove(destination, descend * TileSpeed).SetEase(Ease.InSine)
                        .OnComplete(() =>
                        {
                            _descendingTiles.Remove(hexagonSprite.GetPlacedHexagon().Cell);
                            if (hexagon is BombHexagon) bombTimerController.UnHookTransform();
                            spritePool.ReturnHexagonSprite(hexagonSprite);
                            spritePool.ReturnMask(remainingMasks[maskIndex]);
                            if (++counter != remainingMasks.Count) return;
                            StartCoroutine(CheckAllTilesForMatchedHexagons());
                        });
                }
        }

        private IEnumerator CheckAllTilesForMatchedHexagons()
        {
            while (_descendingTiles.Count > 0) yield return null;

            var placedHexagons = _gridBuilder.GetPlacement();
            var matchedHexagons = new HashSet<PlacedHexagon>();
            foreach (var placedHexagon in placedHexagons)
            {
                if (placedHexagon is BombHexagon bombHexagon)
                {
                    bombTimerController.Show(_grid.GetCellCenterWorld(placedHexagon.Cell));
                    bombTimerController.SetTimerText(bombHexagon.Timer);
                }

                placedHexagon.CheckForMatch(_grid, placedHexagons, matchedHexagons);
            }

            if (matchedHexagons.Count <= 2) yield break;
            var counter = 0;
            var firstThree = matchedHexagons.Where(matchedHexagon => ++counter <= 3).ToList();
            ExplodeMatchedHexagons(firstThree);
            DestroyMatchedAndFillHoles(firstThree, placedHexagons);
        }
    }
}