using HexagonMusapKahraman.Core;
using HexagonMusapKahraman.Gestures;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexagonMusapKahraman.GridMap
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CellSelector : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private int selectionCount = 3;
        private Camera _camera;
        private GridBuilder _gridBuilder;
        private HexagonGroupController _groupController;
        private bool _isUserRotateInput;
        private Vector3 _selectionCenter;

        private void Awake()
        {
            _camera = Camera.main;
            _gridBuilder = GetComponentInParent<GridBuilder>();
            _groupController = GetComponentInParent<HexagonGroupController>();
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
            var neighbors = NeighborHood.GetNeighbors(clickedPoint, _gridBuilder.GetPlacement(), selectionCount);
            var sumX = 0f;
            var sumY = 0f;
            for (var i = 0; i < selectionCount; i++)
            {
                sumX += neighbors[i].Center.x;
                sumY += neighbors[i].Center.y;
            }

            _selectionCenter = new Vector3(sumX / selectionCount, sumY / selectionCount, 0);
            _groupController.ShowAtCenter(_selectionCenter, neighbors);
        }

        private void OnRotated(RotationDirection direction)
        {
            _groupController.RotateSelectedHexagonGroup(direction);
        }
    }
}