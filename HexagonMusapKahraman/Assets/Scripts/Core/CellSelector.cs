using System.Collections.Generic;
using HexagonMusapKahraman.Gestures;
using HexagonMusapKahraman.GridMap;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexagonMusapKahraman.Core
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CellSelector : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private int selectionCount = 3;
        private Vector3 _selectionCenter;
        private Camera _camera;
        private GridBuilder _gridBuilder;
        private bool _isUserRotateInput;

        private void Awake()
        {
            _camera = Camera.main;
            _gridBuilder = GetComponentInParent<GridBuilder>();
        }

        private void OnEnable()
        {
            RotationGesture.Rotated += OnRotated;
        }

        private void OnDisable()
        {
            RotationGesture.Rotated += OnRotated;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isUserRotateInput = true;
            RotationGesture.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RotationGesture.OnDrag(eventData, _camera.WorldToScreenPoint(_selectionCenter));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isUserRotateInput = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isUserRotateInput) return;
            var cameraClickedWorldPoint = _camera.ScreenToWorldPoint(eventData.position);
            var clickedPoint = new Vector3(cameraClickedWorldPoint.x, cameraClickedWorldPoint.y, 0);
            Debug.Log($"clickedPoint: {clickedPoint}");
            var distances = new SortedList<float, PlacedHexagon>();
            foreach (var placedHexagon in _gridBuilder.GetPlacement())
            {
                float sqrMagnitude = Vector3.SqrMagnitude(clickedPoint - placedHexagon.Center);
                distances.Add(sqrMagnitude, placedHexagon);
            }

            var neighbors = new List<PlacedHexagon>();
            for (var i = 0; i < selectionCount; i++)
            {
                float distance = distances.Keys[i];
                neighbors.Add(distances[distance]);
            }

            _selectionCenter = neighbors[0].Center;
            foreach (var placedHexagon in neighbors)
            {
                _selectionCenter = Vector3.Lerp(_selectionCenter, placedHexagon.Center, 0.5f);
            }
            Debug.Log($"selectionCenter: {_selectionCenter}");
        }

        private void OnRotated(RotationDirection direction)
        {
            Debug.Log(direction);
        }
    }
}