using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HexagonMusapKahraman.Gestures;
using HexagonMusapKahraman.GridMap;
using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    public class HexagonGroupController : MonoBehaviour
    {
        [SerializeField] private GameObject debugSprite;
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

        private void Awake()
        {
            _gridBuilder = GetComponent<GridBuilder>();
            _grid = GetComponent<Grid>();
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

                var hexagonSpriteMask = Instantiate(hexagonSpriteMaskPrefab, placedHexagon.Center, Quaternion.identity);
                _hexagonSpriteMasks.Add(hexagonSpriteMask);
            }
        }

        private void ClearInstantiatedObjects()
        {
            if (_rotatingParent != null) Destroy(_rotatingParent);
            foreach (var o in _rotatingHexagonSprites) Destroy(o);
            _rotatingHexagonSprites.Clear();
            foreach (var o in _hexagonSpriteMasks) Destroy(o);
            _hexagonSpriteMasks.Clear();
        }

        private GameObject BuildHexagonSprite(PlacedHexagon placedHexagon)
        {
            var hexagonSprite = Instantiate(hexagonSpritePrefab, placedHexagon.Center, Quaternion.identity);
            hexagonSprite.GetComponent<SpriteRenderer>().color = placedHexagon.Hexagon.color;
            hexagonSprite.GetComponent<HexagonRelation>().Hexagon = placedHexagon;
            return hexagonSprite;
        }

        public void RotateSelectedHexagonGroup(RotationDirection direction)
        {
            if (_isAlreadyRotating) return;
            Rotate(direction);
        }

        private void Rotate(RotationDirection direction)
        {
            if (_rotatingParent == null) return;
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
                sumX += hexagon.Center.x;
                sumY += hexagon.Center.y;
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

            // Reflect sprite's movement to the related hexagon.
            foreach (var sprite in _rotatingHexagonSprites)
            foreach (var placedHexagon in placedHexagons.Where(gridTile =>
                gridTile.Equals(sprite.GetComponent<HexagonRelation>().Hexagon)))
                placedHexagon.Center = sprite.transform.position;

            // Compare colors of each sprite with its surrounding tiles' colors.
            matchList = new HashSet<PlacedHexagon>();
            foreach (var sprite in _rotatingHexagonSprites)
                sprite.GetComponent<HexagonRelation>().Hexagon.CheckForThreeMatch(_grid, placedHexagons, matchList);

            if (matchList.Count <= 2) return false;

            FillHoles(matchList, placedHexagons);
            return true;
        }

        private void FillHoles(HashSet<PlacedHexagon> matchList, ICollection<PlacedHexagon> tempPlacedHexagons)
        {
            // Filter hexagons to the columns (above and including the matched hexagons)
            var hexesAboveMatches = (from tempPlacedHexagon in tempPlacedHexagons
                from matchedHexagon in matchList
                where Math.Abs(tempPlacedHexagon.Center.x - matchedHexagon.Center.x) < _grid.cellSize.y * 0.5f &&
                      tempPlacedHexagon.Center.y >= matchedHexagon.Center.y
                select tempPlacedHexagon).ToList();

            // Eliminate duplicates
            var set = new HashSet<PlacedHexagon>(hexesAboveMatches);
            hexesAboveMatches = new List<PlacedHexagon>(set);

            // Order the filtered list by height
            hexesAboveMatches = hexesAboveMatches.OrderBy(hexagon => hexagon.Center.y).ToList();

            // Separate them by columns
            var columns = new Dictionary<int, List<PlacedHexagon>>();
            foreach (var hex in hexesAboveMatches)
            {
                int key = _grid.WorldToCell(hex.Center).y;
                if (columns.ContainsKey(key)) columns[key].Add(hex);
                else columns.Add(key, new List<PlacedHexagon> {hex});
            }

            // Number of hexagon matches per column
            var matchCountPerColumns = new Dictionary<int, int>();
            foreach (var hex in from hex in hexesAboveMatches
                from match in matchList
                where hex.Center == match.Center
                select hex)
            {
                int key = _grid.WorldToCell(hex.Center).y;
                if (matchCountPerColumns.ContainsKey(key)) matchCountPerColumns[key]++;
                else matchCountPerColumns.Add(key, 1);
            }

            // Place masks over them
            var masks = new Dictionary<int, List<GameObject>>();
            foreach (var hex in hexesAboveMatches)
            {
                int key = _grid.WorldToCell(hex.Center).y;
                var mask = Instantiate(hexagonSpriteMaskPrefab, hex.Center, Quaternion.identity);
                if (masks.ContainsKey(key)) masks[key].Add(mask);
                else masks.Add(key, new List<GameObject> {mask});
            }

            // Remove matched hexagons from the filtered list
            foreach (var hexagon in matchList)
            {
                tempPlacedHexagons.Remove(hexagon);
                hexesAboveMatches.RemoveAll(placedHexagon => placedHexagon.Center == hexagon.Center);
                foreach (var column in columns)
                    column.Value.RemoveAll(placedHexagon => placedHexagon.Center == hexagon.Center);
            }


            foreach (var column in columns)
            {
                int matchCountPerColumn = matchCountPerColumns[column.Key];
                float distanceToDescend = matchCountPerColumn * _grid.cellSize.x;
                foreach (var hexAboveMatches in column.Value)
                {
                    var targetCenter = hexAboveMatches.Center + distanceToDescend * Vector3.down;

                    // Create sprites to be animated
                    var hexagonSprite = BuildHexagonSprite(hexAboveMatches);
                    hexagonSprite.transform.DOMove(targetCenter, matchCountPerColumn * 0.25f).SetEase(Ease.InSine)
                        .OnComplete(() =>
                        {
                            Destroy(hexagonSprite);
                            foreach (var o in masks[column.Key]) Destroy(o);
                        });

                    tempPlacedHexagons.Remove(hexAboveMatches);
                    hexAboveMatches.Center = targetCenter;
                    tempPlacedHexagons.Add(hexAboveMatches);
                }
            }

            _gridBuilder.SetPlacement(tempPlacedHexagons);

            foreach (var hexagon in _gridBuilder.GetComplementaryHexagons()) Debug.Log(hexagon.Center);
        }
    }
}