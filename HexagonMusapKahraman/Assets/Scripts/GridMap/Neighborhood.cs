using System;
using System.Collections.Generic;
using HexagonMusapKahraman.Core;
using UnityEngine;

namespace HexagonMusapKahraman.GridMap
{
    public enum NeighborType
    {
        Top,
        Down,
        BottomLeft
    }

    public static class NeighborHood
    {
        private const float HexagonTilesHorizontalDistance = 0.75f;

        public static List<PlacedHexagon> GetNeighbors(Grid grid, Vector3 point, List<PlacedHexagon> hexagons,
            int count)
        {
            var distances = new SortedList<float, PlacedHexagon>();
            for (var i = 0; i < hexagons.Count; i++)
            {
                var hexagon = hexagons[i];
                float sqrMagnitude = Vector3.SqrMagnitude(point - grid.GetCellCenterWorld(hexagon.Cell));

                while (distances.ContainsKey(sqrMagnitude))
                    sqrMagnitude += 0.001f;

                distances.Add(sqrMagnitude, hexagon);
            }

            var neighbors = new List<PlacedHexagon>();
            for (var i = 0; neighbors.Count < count; i++)
            {
                if (i > 1)
                {
                    var neighborCenter = grid.GetCellCenterWorld(neighbors[1].Cell);
                    var center = grid.GetCellCenterWorld(distances[distances.Keys[i]].Cell);
                    float distanceBetweenLegs = Vector3.SqrMagnitude(neighborCenter - center);
                    if (distanceBetweenLegs > 2) continue;
                }

                neighbors.Add(distances[distances.Keys[i]]);
            }

            return neighbors;
        }

        public static List<PlacedHexagon> GetNeighbors(Vector3Int cell, List<PlacedHexagon> hexagons, Grid grid)
        {
            var cellSize = grid.cellSize;
            float height = cellSize.x;
            float width = cellSize.y * HexagonTilesHorizontalDistance;
            var cellCenterWorld = grid.GetCellCenterWorld(cell);
            const int neighborsCount = 6;
            var neighborCells = new List<Vector3Int>(neighborsCount)
            {
                grid.WorldToCell(cellCenterWorld + new Vector3(0, height, 0)),
                grid.WorldToCell(cellCenterWorld + new Vector3(width, height * 0.5f, 0)),
                grid.WorldToCell(cellCenterWorld + new Vector3(width, -height * 0.5f, 0)),
                grid.WorldToCell(cellCenterWorld + new Vector3(0, -height, 0)),
                grid.WorldToCell(cellCenterWorld + new Vector3(-width, -height * 0.5f, 0)),
                grid.WorldToCell(cellCenterWorld + new Vector3(-width, height * 0.5f, 0))
            };

            var neighbors = new List<PlacedHexagon>();
            for (var i = 0; i < neighborsCount; i++)
            for (var j = 0; j < hexagons.Count; j++)
                if (neighborCells[i] == hexagons[j].Cell)
                    neighbors.Add(hexagons[j]);

            return neighbors;
        }

        public static bool GetNeighbor(Vector3Int cell, List<PlacedHexagon> hexagons, Grid grid,
            NeighborType neighborType, out PlacedHexagon neighbor)
        {
            var cellSize = grid.cellSize;
            float height = cellSize.x;
            float width = cellSize.y * HexagonTilesHorizontalDistance;
            var cellCenterWorld = grid.GetCellCenterWorld(cell);
            Vector3Int neighborCell;
            switch (neighborType)
            {
                case NeighborType.Top:
                    neighborCell = grid.WorldToCell(cellCenterWorld + new Vector3(0, height, 0));
                    break;
                case NeighborType.Down:
                    neighborCell = grid.WorldToCell(cellCenterWorld + new Vector3(0, -height, 0));
                    break;
                case NeighborType.BottomLeft:
                    neighborCell = grid.WorldToCell(cellCenterWorld + new Vector3(-width, -height * 0.5f, 0));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(neighborType), neighborType, null);
            }

            for (var j = 0; j < hexagons.Count; j++)
            {
                if (neighborCell != hexagons[j].Cell) continue;
                neighbor = hexagons[j];
                return true;
            }

            neighbor = new PlacedHexagon(null, neighborCell);
            return false;
        }
    }
}