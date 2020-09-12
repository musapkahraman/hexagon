using UnityEngine;
using UnityEngine.Tilemaps;

namespace HexagonMusapKahraman.GridMap
{
    public class GridResizer : MonoBehaviour
    {
        private const float MinCameraSize = 6.5f;
        [SerializeField] private Tile tile;
        [SerializeField] private Vector2 size = new Vector2(8, 9);
        [SerializeField] private Vector2 cameraOffset = new Vector2(1, 1);
        private Camera _camera;
        private BoxCollider2D _collider;
        private Tilemap _tilemap;

        private void OnValidate()
        {
            size.x = (int) Mathf.Clamp(size.x, 2, 15);
            size.y = (int) Mathf.Clamp(size.y, 2, 15);
            _tilemap = GetComponentInChildren<Tilemap>();
            _collider = _tilemap.GetComponent<BoxCollider2D>();
            _camera = Camera.main;
        }

        public void ResizeGridMap()
        {
            RepaintTileMap();
            var mapBounds = ResizeCollider();
            ReplaceCamera(mapBounds);
        }

        private void RepaintTileMap()
        {
            _tilemap.ClearAllTiles();
            for (var x = 0; x < size.y; x++)
            for (var y = 0; y < size.x; y++)
                _tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            _tilemap.CompressBounds();
        }

        private Bounds ResizeCollider()
        {
            var mapBounds = _tilemap.localBounds;
            _collider.size = mapBounds.size;
            _collider.offset = mapBounds.center;
            return mapBounds;
        }

        private void ReplaceCamera(Bounds mapBounds)
        {
            _camera.transform.position = mapBounds.center + new Vector3(cameraOffset.x, cameraOffset.y, -10);
            // Zoom in/out to show all the tiles in the map.
            float camHalfHeight = _camera.orthographicSize;
            float camHalfWidth = _camera.aspect * camHalfHeight;
            float mapHalfHeight = mapBounds.size.y * 0.5f;
            float mapHalfWidth = mapBounds.size.x * 0.5f;
            _camera.orthographicSize = mapHalfWidth / camHalfWidth * _camera.orthographicSize;
            if (_camera.orthographicSize < mapHalfHeight * 1.2f)
            {
                _camera.orthographicSize = mapHalfHeight * 1.2f;
            }
        }
    }
}