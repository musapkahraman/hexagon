using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private List<Hexagon> bombHexagons;
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
            var hashSet = new HashSet<Hexagon>(hexagons);
            hexagons = new List<Hexagon>(hashSet);
            hexagons = hexagons.OrderBy(hexagon => hexagon.name).ToList();
            hashSet = new HashSet<Hexagon>(bombHexagons);
            bombHexagons = new List<Hexagon>(hashSet);
            bombHexagons = bombHexagons.OrderBy(hexagon => hexagon.name).ToList();
        }

        public void Clear()
        {
            _placedHexagons.Clear();
            _tilemap.ClearAllTiles();
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
                _tilemap.SetTile(placedHexagon.Cell, tile);
            }
        }

        public IEnumerable<PlacedHexagon> GetComplementaryHexagons(bool shouldPlaceBomb)
        {
            var alreadyPlacedBomb = false;
            var complementaryHexagons = new List<PlacedHexagon>();
            for (var columnIndex = 0; columnIndex < _gridSize.x; columnIndex++)
            for (var rowIndex = 0; rowIndex < _gridSize.y; rowIndex++)
            {
                var position = new Vector3Int(rowIndex, columnIndex, 0);
                var value = _placedHexagons.Find(placedHexagon => placedHexagon.Cell == position);
                if (value != null) continue;

                if (shouldPlaceBomb && !alreadyPlacedBomb)
                {
                    alreadyPlacedBomb = true;
                    var hexagon = bombHexagons[Random.Range(0, bombHexagons.Count)];
                    if (NeighborHood.GetNeighbor(position, _placedHexagons, _grid, NeighborType.BottomLeft,
                        out var neighbor))
                        while (neighbor.Hexagon.color.Equals(hexagon.color))
                            hexagon = bombHexagons[Random.Range(0, bombHexagons.Count)];

                    var tile = CreateTile(hexagon);
                    _tilemap.SetTile(position, tile);
                    var item = new BombHexagon(hexagon, position, Random.Range(6, 10));
                    complementaryHexagons.Add(item);
                    _placedHexagons.Add(item);
                }
                else
                {
                    var hexagon = hexagons[Random.Range(0, hexagons.Count)];
                    if (NeighborHood.GetNeighbor(position, _placedHexagons, _grid, NeighborType.BottomLeft,
                        out var neighbor))
                        while (neighbor.Hexagon.color.Equals(hexagon.color))
                            hexagon = hexagons[Random.Range(0, hexagons.Count)];

                    var tile = CreateTile(hexagon);
                    _tilemap.SetTile(position, tile);
                    var item = new PlacedHexagon(hexagon, position);
                    complementaryHexagons.Add(item);
                    _placedHexagons.Add(item);
                }
            }

            return complementaryHexagons;
        }

        private void SetInitialMap()
        {
            for (var columnIndex = 0; columnIndex < _gridSize.x; columnIndex++)
            for (var rowIndex = 0; rowIndex < _gridSize.y; rowIndex++)
            {
                var position = new Vector3Int(rowIndex, columnIndex, 0);
                var hexagon = hexagons[Random.Range(0, hexagons.Count)];
                if (NeighborHood.GetNeighbor(position, _placedHexagons, _grid, NeighborType.BottomLeft,
                    out var neighbor))
                    while (neighbor.Hexagon.color.Equals(hexagon.color))
                        hexagon = hexagons[Random.Range(0, hexagons.Count)];

                var tile = CreateTile(hexagon);
                _tilemap.SetTile(position, tile);
                _placedHexagons.Add(new PlacedHexagon(hexagon, position));
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