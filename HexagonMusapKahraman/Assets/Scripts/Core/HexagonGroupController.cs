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
        private readonly List<GameObject> _hexagonSprites = new List<GameObject>();
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
                var hexagonSprite = Instantiate(hexagonSpritePrefab, placedHexagon.Center, Quaternion.identity);
                hexagonSprite.GetComponent<SpriteRenderer>().color = placedHexagon.Hexagon.color;
                hexagonSprite.GetComponent<HexagonSprite>().Hexagon = placedHexagon;
                hexagonSprite.transform.parent = _rotatingParent.transform;
                _hexagonSprites.Add(hexagonSprite);

                var hexagonSpriteMask = Instantiate(hexagonSpriteMaskPrefab, placedHexagon.Center, Quaternion.identity);
                _hexagonSpriteMasks.Add(hexagonSpriteMask);
            }
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
                    if (!CheckForMatch(out var matchList))
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

        private void ClearInstantiatedObjects()
        {
            if (_rotatingParent != null) Destroy(_rotatingParent);
            foreach (var o in _hexagonSprites) Destroy(o);
            _hexagonSprites.Clear();
            foreach (var o in _hexagonSpriteMasks) Destroy(o);
            _hexagonSpriteMasks.Clear();
        }

        private bool CheckForMatch(out HashSet<PlacedHexagon> matchList)
        {
            var tempPlacedHexagons = _gridBuilder.GetPlacement();
            for (var i = 0; i < _hexagonSprites.Count; i++)
            for (var index = 0; index < tempPlacedHexagons.Count; index++)
                if (tempPlacedHexagons[index].Equals(_hexagonSprites[i].GetComponent<HexagonSprite>().Hexagon))
                    tempPlacedHexagons[index].Center = _hexagonSprites[i].transform.position;

            matchList = new HashSet<PlacedHexagon>();
            for (var i = 0; i < _hexagonSprites.Count; i++)
            {
                var position = _hexagonSprites[i].transform.position;
                var neighbors = NeighborHood.GetNeighbors(position, tempPlacedHexagons, _grid);
                var color = _hexagonSprites[i].GetComponent<SpriteRenderer>().color;
                for (var index = 0; index < neighbors.Count; index++)
                {
                    if (!neighbors[index].Hexagon.color.Equals(color)) continue;

                    int previousIndex = index == 0 ? neighbors.Count - 1 : index - 1;
                    if (neighbors[previousIndex].Hexagon.color.Equals(color))
                    {
                        float distance = Vector3.Distance(neighbors[previousIndex].Center, neighbors[index].Center);
                        if (distance <= _grid.cellSize.x)
                        {
                            matchList.Add(_hexagonSprites[i].GetComponent<HexagonSprite>().Hexagon);
                            matchList.Add(neighbors[index]);
                            matchList.Add(neighbors[previousIndex]);
                        }
                    }

                    int nextIndex = index == neighbors.Count - 1 ? 0 : index + 1;
                    if (neighbors[nextIndex].Hexagon.color.Equals(color))
                    {
                        float distance = Vector3.Distance(neighbors[nextIndex].Center, neighbors[index].Center);
                        if (distance <= _grid.cellSize.x)
                        {
                            matchList.Add(_hexagonSprites[i].GetComponent<HexagonSprite>().Hexagon);
                            matchList.Add(neighbors[index]);
                            matchList.Add(neighbors[nextIndex]);
                        }
                    }
                }
            }

            if (matchList.Count <= 2) return false;
            ReplaceMatchedTiles(matchList, tempPlacedHexagons);
            return true;
        }

        private void ReplaceMatchedTiles(HashSet<PlacedHexagon> matchList, List<PlacedHexagon> tempPlacedHexagons)
        {
            var hexesAboveMatches = (from tempPlacedHexagon in tempPlacedHexagons
                from matchedHexagon in matchList
                where Math.Abs(tempPlacedHexagon.Center.x - matchedHexagon.Center.x) < _grid.cellSize.y * 0.5f &&
                      tempPlacedHexagon.Center.y >= matchedHexagon.Center.y
                select tempPlacedHexagon).ToList();

            foreach (var hexagon in matchList)
            {
                tempPlacedHexagons.Remove(hexagon);
                hexesAboveMatches.RemoveAll(placedHexagon => placedHexagon.Center == hexagon.Center);
            }

            foreach (var hexAboveMatches in hexesAboveMatches)
            {
                Instantiate(debugSprite, hexAboveMatches.Center, Quaternion.identity);
            }

            _gridBuilder.SetPlacement(tempPlacedHexagons);
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
    }
}