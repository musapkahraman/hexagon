using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexagonMusapKahraman.Core
{
    public class CellSelector : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField] private TMP_Text textfield;
        private Camera _camera;
        private Grid _grid;

        private void Awake()
        {
            _camera = Camera.main;
            _grid = GetComponentInParent<Grid>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            textfield.text = $"Pointer {GetClickedCell(eventData.position).ToString()}";
        }

        public void OnDrag(PointerEventData eventData)
        {
            textfield.text = $"Dragging {GetClickedCell(eventData.position).ToString()}";
        }

        private Vector3Int GetClickedCell(Vector2 position)
        {
            var worldPoint = _camera.ScreenToWorldPoint(position);
            return _grid.WorldToCell(worldPoint);
        }
    }
}