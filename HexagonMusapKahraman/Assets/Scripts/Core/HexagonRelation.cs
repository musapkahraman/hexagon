using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class HexagonRelation : MonoBehaviour
    {
        private Grid _grid;
        private SpriteRenderer _spriteRenderer;
        private PlacedHexagon _placedHexagon;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _placedHexagon = new PlacedHexagon(null, Vector3Int.zero);
        }

        private void Update()
        {
            _placedHexagon.Cell = _grid.WorldToCell(transform.position);
        }

        public void SetGrid(Grid grid)
        {
            _grid = grid;
        }

        public void SetRelation(PlacedHexagon placedHexagon)
        {
            _placedHexagon = placedHexagon;
        }

        public void RemoveRelation()
        {
            _placedHexagon = new PlacedHexagon(null, Vector3Int.zero);
        }

        public PlacedHexagon GetPlacedHexagon()
        {
            return _placedHexagon;
        }

        public void SetSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }

        public void SetColor(Color color)
        {
            _spriteRenderer.color = color;
        }

        public void EnableRenderer()
        {
            _spriteRenderer.enabled = true;
        }

        public void DisableRenderer()
        {
            _spriteRenderer.enabled = false;
        }
    }
}