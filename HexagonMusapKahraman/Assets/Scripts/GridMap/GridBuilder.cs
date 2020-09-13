using System.Collections.Generic;
using HexagonMusapKahraman.Core;
using HexagonMusapKahraman.ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HexagonMusapKahraman.GridMap
{
    [RequireComponent(typeof(GridResizer))]
    public class GridBuilder : MonoBehaviour
    {
        [SerializeField] private List<Hexagon> hexagons;
        private Grid _grid;
        private Vector2 _gridSize;
        private List<PlacedHexagon> _placedHexagons = new List<PlacedHexagon>();
        private Tilemap _tilemap;

        private void Awake()
        {
            _grid = GetComponent<Grid>();
            _tilemap = GetComponentInChildren<Tilemap>();
            var gridResizer = GetComponent<GridResizer>();
            gridResizer.ResizeGridMap();
            _gridSize = gridResizer.GetGridSize();
            SetInitialMap();
        }

        private void OnValidate()
        {
            var hashSet = new HashSet<Hexagon>();
            foreach (var hexagon in hexagons) hashSet.Add(hexagon);
        }

        public List<PlacedHexagon> GetPlacement()
        {
            return new List<PlacedHexagon>(_placedHexagons);
        }

        public void SetPlacement(IEnumerable<PlacedHexagon> placedHexagons)
        {
            _placedHexagons = new List<PlacedHexagon>(placedHexagons);
            _tilemap.ClearAllTiles();
            foreach (var placedHexagon in _placedHexagons)
            {
                var tile = CreateTile(placedHexagon.Hexagon);
                _tilemap.SetTile(_grid.WorldToCell(placedHexagon.Center), tile);
            }
        }

        private void SetInitialMap()
        {
            for (var columnIndex = 0; columnIndex < _gridSize.x; columnIndex++)
            for (var rowIndex = 0; rowIndex < _gridSize.y; rowIndex++)
            {
                var position = new Vector3Int(rowIndex, columnIndex, 0);
                var cellCenter = _grid.GetCellCenterWorld(position);
                var hexagon = hexagons[Random.Range(0, hexagons.Count)];
                if (NeighborHood.GetNeighbor(cellCenter, _placedHexagons, _grid, NeighborType.BottomLeft,
                    out var neighbor))
                    while (neighbor.Hexagon.color.Equals(hexagon.color))
                        hexagon = hexagons[Random.Range(0, hexagons.Count)];

                var tile = CreateTile(hexagon);
                PlaceHexagon(position, tile, hexagon);
            }

            void PlaceHexagon(Vector3Int position, TileBase tile, Hexagon hexagon)
            {
                _tilemap.SetTile(position, tile);
                _placedHexagons.Add(new PlacedHexagon(hexagon, _grid.GetCellCenterWorld(position)));
            }
        }

        private static Tile CreateTile(Hexagon hexagon)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = hexagon.tile.sprite;
            tile.color = hexagon.color;
            return tile;
        }
    }
}