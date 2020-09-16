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
        [SerializeField] private DynamicData score;
        [SerializeField] private DynamicData move;
        [SerializeField] private DynamicData highScore;
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private BombTimerController bombTimerController;
        [SerializeField] private GameOverMessage gameOverMessage;
        [SerializeField] private HexagonSpritePool spritePool;
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
            if (_isAlreadyRotating) return;
            _isAlreadyRotating = true;
            Rotate(direction);
        }

        private void Rotate(RotationDirection direction)
        {
            if (!_rotatingParent) return;
            switch (direction)
            {
                case RotationDirection.Clockwise:
                    _rotatingParent.DORotate(120 * Vector3.back, 0.3f, RotateMode.LocalAxisAdd)
                        .OnComplete(OnRotationComplete);
                    break;
                case RotationDirection.AntiClockwise:
                    _rotatingParent.DORotate(120 * Vector3.forward, 0.3f, RotateMode.LocalAxisAdd)
                        .OnComplete(OnRotationComplete);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            void OnRotationComplete()
            {
                if (_rotationCheckCounter >= 2)
                {
                    ResetGateKeepers();
                }
                else
                {
                    _rotationCheckCounter++;
                    if (!RotatingGroupMatched(out var matchedHexagons))
                    {
                        Rotate(direction);
                    }
                    else
                    {
                        AnimatePopOut(matchedHexagons);
                        move.IncreaseValue(1);
                        for (var i = 0; i < matchedHexagons.Count; i++)
                        {
                            score.IncreaseValue(scoreMultiplier);
                            if (score.GetValue() % bombScoreInterval == 0) _shouldPlaceBomb = true;
                        }

                        highScore.SetMaximum(score.GetValue());
                        ResetGateKeepers();
                        spritePool.ReturnRotatingParent();
                    }
                }

                void ResetGateKeepers()
                {
                    _rotationCheckCounter = 0;
                    _isAlreadyRotating = false;
                }
            }
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

        private bool RotatingGroupMatched(out HashSet<PlacedHexagon> matchedHexagons)
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
                    _gridBuilder.Clear();
                    spritePool.ReturnRotatingParent();
                    bombTimerController.Hide();
                    gameOverMessage.Show();
                    return false;
                }

                _shouldPlaceBomb = false;
            }

            DestroyMatchedAndFillHoles(matchedHexagons, placedHexagons);
            return true;
        }

        private void DestroyMatchedAndFillHoles(HashSet<PlacedHexagon> matchedHexagons,
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

            // Number of matched hexagons per column
            var matchCountPerColumns = new Dictionary<int, int>();
            foreach (var hex in from hex in aboveMatchedHexagons
                from match in matchedHexagons
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
                var mask = spritePool.GetMask(_grid.GetCellCenterWorld(hex.Cell));
                if (masks.ContainsKey(key)) masks[key].Add(mask);
                else masks.Add(key, new List<GameObject> {mask});
            }

            // Remove matched hexagons from the filtered list
            foreach (var hexagon in matchedHexagons)
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
                float descend = matchCountPerColumn * _grid.cellSize.x;
                foreach (var hexAboveMatches in column.Value)
                {
                    var targetCenter = _grid.GetCellCenterWorld(hexAboveMatches.Cell) + descend * Vector3.down;

                    // Create sprites to be animated
                    var hexagonSprite = spritePool.GetHexagonSprite(hexAboveMatches);
                    if (hexAboveMatches is BombHexagon hex)
                    {
                        bombTimerController.Show(hexagonSprite.transform);
                        bombTimerController.SetTimerText(hex.Timer);
                    }

                    hexagonSprite.transform.DOMove(targetCenter, matchCountPerColumn * 0.25f).SetEase(Ease.InSine)
                        .OnComplete(() =>
                        {
                            bombTimerController.UnHookTransform();
                            spritePool.ReturnHexagonSprite(hexagonSprite);
                            foreach (var mask in masks[column.Key]) spritePool.ReturnMask(mask);
                        });

                    placedHexagons.Remove(hexAboveMatches);
                    hexAboveMatches.Cell = _grid.WorldToCell(targetCenter);
                    placedHexagons.Add(hexAboveMatches);
                }
            }

            _gridBuilder.SetPlacement(placedHexagons);

            var complementaryHexagons = _gridBuilder.GetComplementaryHexagons(_shouldPlaceBomb);
            foreach (var complementaryHexagon in complementaryHexagons)
            {
                if (!(complementaryHexagon is BombHexagon hex)) continue;
                bombTimerController.Show(_grid.GetCellCenterWorld(hex.Cell));
                bombTimerController.SetTimerText(hex.Timer);
                _shouldPlaceBomb = false;
            }
        }
    }
}