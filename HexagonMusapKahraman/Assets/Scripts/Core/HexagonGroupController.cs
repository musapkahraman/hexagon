using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HexagonMusapKahraman.Gestures;
using HexagonMusapKahraman.GridMap;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HexagonMusapKahraman.Core
{
    public class HexagonGroupController : MonoBehaviour
    {
        [SerializeField] private GameObject rotatingParentPrefab;
        [SerializeField] private GameObject hexagonSpritePrefab;
        [SerializeField] private GameObject hexagonSpriteMaskPrefab;
        [SerializeField] private ParticleSystem particles;
        private readonly List<GameObject> _hexagonSpriteMasks = new List<GameObject>();
        private readonly List<GameObject> _hexagonSprites = new List<GameObject>();
        private Grid _grid;
        private GridBuilder _gridBuilder;
        private GameObject _rotatingParent;
        private int _rotationCheckCounter;
        private bool _isAlreadyRotating;
        private List<PlacedHexagon> _placedHexagons;

        private void Awake()
        {
            _gridBuilder = GetComponent<GridBuilder>();
            _grid = GetComponent<Grid>();
        }

        public void ShowAtCenter(Vector3 center, IEnumerable<PlacedHexagon> neighbors)
        {
            if (_isAlreadyRotating) return;

            ClearInstantiatedObjects();

            _placedHexagons = _gridBuilder.GetPlacement();
            _rotatingParent = Instantiate(rotatingParentPrefab, center, Quaternion.identity);
            foreach (var placedHexagon in neighbors)
            {
                _placedHexagons.Remove(placedHexagon);

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
                if (_rotationCheckCounter < 2)
                {
                    _rotationCheckCounter++;
                    if (CheckForMatch(out var matchList))
                    {
                        var sumX = 0f;
                        var sumY = 0f;
                        var color = Color.white;
                        foreach (var hexagon in matchList)
                        {
                            sumX += hexagon.Center.x;
                            sumY += hexagon.Center.y;
                            
                            Instantiate(hexagonSpriteMaskPrefab, hexagon.Center, Quaternion.identity);
                            color = hexagon.Hexagon.color;
                        }

                        particles.transform.position = new Vector3(sumX / matchList.Count, sumY/ matchList.Count);
                        var main = particles.main;
                        main.startColor = color;
                        particles.Play();
                        ResetGateKeepers();
                        ClearInstantiatedObjects();
                        return;
                    }

                    Rotate(direction);
                }
                else
                {
                    ResetGateKeepers();
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
            // for (var j = 0; j < _hexagonSprites.Count; j++)
            // {
            //     _placedHexagons.Add(new PlacedHexagon
            //     {
            //         Center = _hexagonSprites[j].transform.position,
            //         Hexagon = _hexagonSprites[j].GetComponent<HexagonSprite>().Hexagon.Hexagon
            //     });
            // }

            matchList = new HashSet<PlacedHexagon>();
            for (var j = 0; j < _hexagonSprites.Count; j++)
            {
                var position = _hexagonSprites[j].transform.position;
                var neighbors = NeighborHood.GetNeighbors(position, _placedHexagons, _grid);
                var color = _hexagonSprites[j].GetComponent<SpriteRenderer>().color;
                for (var index = 0; index < neighbors.Count; index++)
                {
                    if (!neighbors[index].Hexagon.color.Equals(color)) continue;
                    int previousIndex = index == 0 ? neighbors.Count - 1 : index - 1;
                    int nextIndex = index == neighbors.Count - 1 ? 0 : index + 1;
                    if (neighbors[previousIndex].Hexagon.color.Equals(color))
                    {
                        if (Vector3.Distance(neighbors[previousIndex].Center, neighbors[index].Center) < 1.5f)
                        {
                            matchList.Add(_hexagonSprites[j].GetComponent<HexagonSprite>().Hexagon);
                            matchList.Add(neighbors[index]);
                            matchList.Add(neighbors[previousIndex]);
                        }
                    }

                    if (neighbors[nextIndex].Hexagon.color.Equals(color))
                    {
                        if (Vector3.Distance(neighbors[nextIndex].Center, neighbors[index].Center) < 1.5f)
                        {
                            matchList.Add(_hexagonSprites[j].GetComponent<HexagonSprite>().Hexagon);
                            matchList.Add(neighbors[index]);
                            matchList.Add(neighbors[nextIndex]);
                        }
                    }
                }
            }


            foreach (var placedHexagon in matchList)
            {
                Debug.Log(placedHexagon.Center);
            }

            return matchList.Count > 2;
        }
    }
}