using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HexagonMusapKahraman.Gestures;
using HexagonMusapKahraman.GridMap;
using HexagonMusapKahraman.ScriptableObjects;
using HexagonMusapKahraman.UI;
using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    public class HexagonGroupController : MonoBehaviour
    {
        [SerializeField] private int scoreMultiplier = 5;
        [SerializeField] private int bombScoreInterval = 50;
        [SerializeField] private BombTimerController bombTimerController;
        [SerializeField] private GameOverMessage gameOverMessage;
        [SerializeField] private DynamicData score;
        [SerializeField] private DynamicData move;
        [SerializeField] private DynamicData highScore;
        [SerializeField] private GameObject rotatingParentPrefab;
        [SerializeField] private GameObject hexagonSpritePrefab;
        [SerializeField] private GameObject hexagonSpriteMaskPrefab;
        [SerializeField] private ParticleSystem particles;
        private readonly List<GameObject> _hexagonSpriteMasks = new List<GameObject>();
        private readonly List<GameObject> _rotatingHexagonSprites = new List<GameObject>();
        private Grid _grid;
        private GridBuilder _gridBuilder;
        private bool _isAlreadyRotating;
        private GameObject _rotatingParent;
        private int _rotationCheckCounter;
        private bool _shouldPlaceBomb;

        private void Awake()
        {
            _grid = GetComponent<Grid>();
            _gridBuilder = GetComponent<GridBuilder>();
        }

        public void ShowAtCenter(Vector3 center, IEnumerable<PlacedHexagon> neighbors)
        {
            if (_isAlreadyRotating) return;

            ClearInstantiatedObjects();

            _rotatingParent = Instantiate(rotatingParentPrefab, center, Quaternion.identity);
            foreach (var placedHexagon in neighbors)
            {
                var hexagonSprite = BuildHexagonSprite(placedHexagon);
                hexagonSprite.transform.parent = _rotatingParent.transform;
                _rotatingHexagonSprites.Add(hexagonSprite);
                if (placedHexagon is BombHexagon hex)
                {
                    bombTimerController.Show(hexagonSprite.transform);
                    bombTimerController.SetTimerText(hex.Timer);
                }

                var hexagonSpriteMask = Instantiate(hexagonSpriteMaskPrefab,
                    _grid.GetCellCenterWorld(placedHexagon.Cell), Quaternion.identity);
                _hexagonSpriteMasks.Add(hexagonSpriteMask);
            }
        }

        private void ClearInstantiatedObjects()
        {
            if (_rotatingParent) Destroy(_rotatingParent);
            foreach (var o in _rotatingHexagonSprites) Destroy(o);
            _rotatingHexagonSprites.Clear();
            foreach (var o in _hexagonSpriteMasks) Destroy(o);
            _hexagonSpriteMasks.Clear();
        }

        private GameObject BuildHexagonSprite(PlacedHexagon placedHexagon)
        {
            var hexagonSprite = Instantiate(hexagonSpritePrefab, _grid.GetCellCenterWorld(placedHexagon.Cell),
                Quaternion.identity);
            var spriteRenderer = hexagonSprite.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = placedHexagon.Hexagon.tile.sprite;
            spriteRenderer.color = placedHexagon.Hexagon.color;
            var hexagonRelation = hexagonSprite.GetComponent<HexagonRelation>();
            hexagonRelation.Hexagon = placedHexagon;
            hexagonRelation.SetGrid(_grid);
            return hexagonSprite;
        }

        public void RotateSelectedHexagonGroup(RotationDirection direction)
        {
            if (_isAlreadyRotating) return;
            Rotate(direction);
        }

        private void Rotate(RotationDirection direction)
        {
            if (_rotatingParent == null || _rotatingParent.transform == null) return;
            switch (direction)
            {
                case RotationDirection.Clockwise:
                    _rotatingParent.transform.DORotate(120 * Vector3.back, 0.3f, RotateMode.LocalAxisAdd)
                        .OnComplete(OnRotationCompleted);
                    _isAlreadyRotating = true;
                    break;
                case RotationDirection.AntiClockwise:
                    _rotatingParent.transform.DORotate(120 * Vector3.forward, 0.3f, RotateMode.LocalAxisAdd)
                        .OnComplete(OnRotationCompleted);
                    _isAlreadyRotating = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            void OnRotationCompleted()
            {
                if (_rotationCheckCounter >= 2)
                {
                    ResetGateKeepers();
                }
                else
                {
                    _rotationCheckCounter++;
                    if (!CheckRotatingSpritesForMatch(out var matchList))
                    {
                        Rotate(direction);
                    }
                    else
                    {
                        PopMatchedTilesOut(matchList);
                        move.IncreaseValue(1);
                        for (var i = 0; i < matchList.Count; i++)
                        {
                            score.IncreaseValue(scoreMultiplier);
                            if (score.GetValue() % bombScoreInterval == 0) _shouldPlaceBomb = true;
                        }

                        highScore.SetMaximum(score.GetValue());
                        ResetGateKeepers();
                        ClearInstantiatedObjects();
                    }
                }

                void ResetGateKeepers()
                {
                    _rotationCheckCounter = 0;
                    _isAlreadyRotating = false;
                }
            }
        }

        private void PopMatchedTilesOut(ICollection<PlacedHexagon> matchList)
        {
            var sumX = 0f;
            var sumY = 0f;
            var color = Color.white;
            foreach (var hexagon in matchList)
            {
                var center = _grid.GetCellCenterWorld(hexagon.Cell);
                sumX += center.x;
                sumY += center.y;
                color = hexagon.Hexagon.color;
            }

            particles.transform.position = new Vector3(sumX / matchList.Count, sumY / matchList.Count);
            var main = particles.main;
            main.startColor = color;
            particles.Play();
        }

        private bool CheckRotatingSpritesForMatch(out HashSet<PlacedHexagon> matchList)
        {
            var placedHexagons = _gridBuilder.GetPlacement();

            // Compare colors of each sprite with its surrounding tiles' colors.
            matchList = new HashSet<PlacedHexagon>();
            foreach (var sprite in _rotatingHexagonSprites)
                sprite.GetComponent<HexagonRelation>().Hexagon.CheckForThreeMatch(_grid, placedHexagons, matchList);

            if (matchList.Count <= 2) return false;

            foreach (var placedHexagon in placedHexagons)
            {
                if (!(placedHexagon is BombHexagon hex)) continue;
                bombTimerController.Show(_grid.GetCellCenterWorld(hex.Cell));
                bombTimerController.SetTimerText(--hex.Timer);
                if (hex.Timer <= 0)
                {
                    _gridBuilder.Clear();
                    _rotatingParent.SetActive(false);
                    bombTimerController.Hide();
                    gameOverMessage.Show();
                    return false;
                }

                _shouldPlaceBomb = false;
            }

            FillHoles(matchList, placedHexagons);
            return true;
        }

        private void FillHoles(HashSet<PlacedHexagon> matchList, ICollection<PlacedHexagon> placedHexagons)
        {
            var aboveMatchedHexagons = new List<PlacedHexagon>();

            // Filter hexagons to the columns (above and including the matched hexagons)
            foreach (var hex in placedHexagons)
                aboveMatchedHexagons.AddRange(from matchedHex in matchList
                    let hexCenter = _grid.GetCellCenterWorld(hex.Cell)
                    let matchedHexCenter = _grid.GetCellCenterWorld(matchedHex.Cell)
                    let horizontalDistance = Math.Abs(hexCenter.x - matchedHexCenter.x)
                    let halfGridHeight = _grid.cellSize.y * 0.5f
                    where horizontalDistance < halfGridHeight && hexCenter.y >= matchedHexCenter.y
                    select hex);

            // Eliminate duplicates
            var set = new HashSet<PlacedHexagon>(aboveMatchedHexagons);
            aboveMatchedHexagons = new List<PlacedHexagon>(set);

            // Order the filtered list by height
            aboveMatchedHexagons = aboveMatchedHexagons.OrderBy(hex => _grid.GetCellCenterWorld(hex.Cell).y).ToList();

            // Separate them by columns
            var columns = new Dictionary<int, List<PlacedHexagon>>();
            foreach (var hex in aboveMatchedHexagons)
            {
                int key = hex.Cell.y;
                if (columns.ContainsKey(key)) columns[key].Add(hex);
                else columns.Add(key, new List<PlacedHexagon> {hex});
            }

            // Number of hexagon matches per column
            var matchCountPerColumns = new Dictionary<int, int>();
            foreach (var hex in from hex in aboveMatchedHexagons
                from match in matchList
                where hex.Cell == match.Cell
                select hex)
            {
                int key = hex.Cell.y;
                if (matchCountPerColumns.ContainsKey(key)) matchCountPerColumns[key]++;
                else matchCountPerColumns.Add(key, 1);
            }

            // Place masks over them
            var masks = new Dictionary<int, List<GameObject>>();
            foreach (var hex in aboveMatchedHexagons)
            {
                int key = hex.Cell.y;
                var mask = Instantiate(hexagonSpriteMaskPrefab, _grid.GetCellCenterWorld(hex.Cell),
                    Quaternion.identity);
                if (masks.ContainsKey(key)) masks[key].Add(mask);
                else masks.Add(key, new List<GameObject> {mask});
            }

            // Remove matched hexagons from the filtered list
            foreach (var hexagon in matchList)
            {
                if (hexagon is BombHexagon) bombTimerController.Hide();

                placedHexagons.Remove(hexagon);
                aboveMatchedHexagons.RemoveAll(placedHexagon => placedHexagon.Cell == hexagon.Cell);
                foreach (var column in columns)
                    column.Value.RemoveAll(placedHexagon => placedHexagon.Cell == hexagon.Cell);
            }

            foreach (var column in columns)
            {
                int matchCountPerColumn = matchCountPerColumns[column.Key];
                float distanceToDescend = matchCountPerColumn * _grid.cellSize.x;
                foreach (var hexAboveMatches in column.Value)
                {
                    var targetCenter = _grid.GetCellCenterWorld(hexAboveMatches.Cell) +
                                       distanceToDescend * Vector3.down;

                    // Create sprites to be animated
                    var hexagonSprite = BuildHexagonSprite(hexAboveMatches);
                    if (hexAboveMatches is BombHexagon hex)
                    {
                        bombTimerController.Show(hexagonSprite.transform);
                        bombTimerController.SetTimerText(hex.Timer);
                    }

                    hexagonSprite.transform.DOMove(targetCenter, matchCountPerColumn * 0.25f).SetEase(Ease.InSine)
                        .OnComplete(() =>
                        {
                            Destroy(hexagonSprite);
                            foreach (var o in masks[column.Key]) Destroy(o);
                        });

                    placedHexagons.Remove(hexAboveMatches);
                    hexAboveMatches.Cell = _grid.WorldToCell(targetCenter);
                    placedHexagons.Add(hexAboveMatches);
                }
            }

            _gridBuilder.SetPlacement(placedHexagons);

            var complementaryHexagons = _gridBuilder.GetComplementaryHexagons(_shouldPlaceBomb);
            foreach (var complementaryHexagon in complementaryHexagons)
                if (complementaryHexagon is BombHexagon hex)
                {
                    bombTimerController.Show(_grid.GetCellCenterWorld(hex.Cell));
                    bombTimerController.SetTimerText(hex.Timer);
                    _shouldPlaceBomb = false;
                }
        }
    }
}