using System.Collections.Generic;
using HexagonMusapKahraman.ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HexagonMusapKahraman.GridMap
{
    [RequireComponent(typeof(GridResizer))]
    public class GridBuilder : MonoBehaviour
    {
        [SerializeField] private List<Hexagon> hexagons;
        private readonly List<PlacedHexagon> _placedHexagons = new List<PlacedHexagon>();
        private Grid _grid;
        private Vector2 _gridSize;
        private Tilemap _tilemap;

        private void Awake()
        {
            _grid = GetComponent<Grid>();
            _tilemap = GetComponentInChildren<Tilemap>();
            _gridSize = GetComponent<GridResizer>().GetGridSize();
            SetInitialMap();
        }

        private void OnValidate()
        {
            var hashSet = new HashSet<Hexagon>();
            foreach (var hexagon in hexagons) hashSet.Add(hexagon);
        }

        private void SetInitialMap()
        {
            for (var columnIndex = 0; columnIndex < _gridSize.x; columnIndex++)
            for (var rowIndex = 0; rowIndex < _gridSize.y; rowIndex++)
            {
                var position = new Vector3Int(rowIndex, columnIndex, 0);
                var cellCenter = _grid.GetCellCenterWorld(position);
                var hexagon = hexagons[Random.Range(0, hexagons.Count)];
                if (NeighborHood.GetBottomLeftNeighbor(cellCenter, _placedHexagons, _grid, out var neighbor))
                    while (neighbor.Hexagon.color.Equals(hexagon.color))
                        hexagon = hexagons[Random.Range(0, hexagons.Count)];

                var tile = CreateTile(hexagon);
                PlaceHexagon(rowIndex, columnIndex, tile, hexagon);
            }

            Tile CreateTile(Hexagon hexagon)
            {
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = hexagon.tile.sprite;
                tile.color = hexagon.color;
                return tile;
            }

            void PlaceHexagon(int rowIndex, int columnIndex, TileBase tile, Hexagon hexagon)
            {
                var position = new Vector3Int(rowIndex, columnIndex, 0);
                _tilemap.SetTile(position, tile);
                _placedHexagons.Add(new PlacedHexagon {Hexagon = hexagon, Center = _grid.GetCellCenterWorld(position)});
            }
        }

        public List<PlacedHexagon> GetPlacement()
        {
            return new List<PlacedHexagon>(_placedHexagons);
        }
    }
}