using HexagonMusapKahraman.Gestures;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexagonMusapKahraman.Core
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CellSelector : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler
    {
        private Camera _camera;
        private Grid _grid;

        private void Awake()
        {
            _camera = Camera.main;
            _grid = GetComponentInParent<Grid>();
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
            RotationGesture.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RotationGesture.OnDrag(eventData, new Vector2(332f, 558));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Debug.Log($"Clicked cell: {GetClickedCell(eventData.position)}");
        }

        private Vector3Int GetClickedCell(Vector2 position)
        {
            var worldPoint = _camera.ScreenToWorldPoint(position);
            // Debug.Log($"Position: {position} ");
            // Debug.Log($"World: {worldPoint} ");
            return _grid.WorldToCell(worldPoint);
        }

        private void OnRotated(RotationDirection direction)
        {
            Debug.Log(direction);
        }
    }
}