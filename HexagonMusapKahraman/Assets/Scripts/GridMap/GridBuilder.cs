﻿using System.Collections.Generic;
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
            for (var x = 0; x < _gridSize.y; x++)
            for (var y = 0; y < _gridSize.x; y++)
            {
                var hexagon = hexagons[Random.Range(0, hexagons.Count)];
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = hexagon.tile.sprite;
                tile.color = hexagon.color;
                var position = new Vector3Int(x, y, 0);
                _tilemap.SetTile(position, tile);
                _placedHexagons.Add(new PlacedHexagon
                {
                    Hexagon = hexagon, Center = _grid.GetCellCenterWorld(position)
                });
            }
        }

        public List<PlacedHexagon> GetPlacement()
        {
            return new List<PlacedHexagon>(_placedHexagons);
        }
    }
}