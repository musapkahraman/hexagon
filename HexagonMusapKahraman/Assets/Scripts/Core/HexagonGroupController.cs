using System;
using System.Collections.Generic;
using HexagonMusapKahraman.Gestures;
using HexagonMusapKahraman.GridMap;
using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    public class HexagonGroupController : MonoBehaviour
    {
        [SerializeField] private GameObject rotatingParentPrefab;
        [SerializeField] private GameObject hexagonSpritePrefab;
        [SerializeField] private GameObject hexagonSpriteMaskPrefab;
        private readonly List<GameObject> _hexagonSpriteMasks = new List<GameObject>();
        private readonly List<GameObject> _hexagonSprites = new List<GameObject>();
        private GameObject _rotatingParent;

        public void ShowAtCenter(Vector3 center, IEnumerable<PlacedHexagon> neighbors)
        {
            ClearInstantiatedObjects();

            _rotatingParent = Instantiate(rotatingParentPrefab, center, Quaternion.identity);
            foreach (var placedHexagon in neighbors)
            {
                var hexagonSprite = Instantiate(hexagonSpritePrefab, placedHexagon.Center, Quaternion.identity);
                hexagonSprite.GetComponent<SpriteRenderer>().color = placedHexagon.Hexagon.color;
                hexagonSprite.transform.parent = _rotatingParent.transform;
                _hexagonSprites.Add(hexagonSprite);

                var hexagonSpriteMask = Instantiate(hexagonSpriteMaskPrefab, placedHexagon.Center, Quaternion.identity);
                _hexagonSpriteMasks.Add(hexagonSpriteMask);
            }
        }

        public void Rotate(RotationDirection direction)
        {
            switch (direction)
            {
                case RotationDirection.Clockwise:
                    Debug.Log("Clockwise");
                    break;
                case RotationDirection.AntiClockwise:
                    Debug.Log("AntiClockwise");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
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
    }
}