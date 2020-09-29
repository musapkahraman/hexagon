using System;
using System.Collections.Generic;
using HexagonGame.Core;
using UnityEngine;

namespace HexagonGame.GridMap
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

        public static bool FindNeighbor(Vector3Int cell, List<PlacedHexagon> hexagons, Grid grid,
            NeighborType neighborType, out PlacedHexagon neighbor)
        {
            var neighbors = GetNeighborsIndexed(cell, hexagons, grid);
            neighbor = new PlacedHexagon(null, Vector3Int.down);
            switch (neighborType)
            {
                case NeighborType.Top:
                    if (!neighbors.ContainsKey(0)) return false;
                    neighbor = neighbors[0];
                    break;
                case NeighborType.Down:
                    if (!neighbors.ContainsKey(3)) return false;
                    neighbor = neighbors[3];
                    break;
                case NeighborType.BottomLeft:
                    if (!neighbors.ContainsKey(4)) return false;
                    neighbor = neighbors[4];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(neighborType), neighborType, null);
            }

            return true;
        }

        public static Dictionary<int, PlacedHexagon> GetNeighborsIndexed(Vector3Int cell, List<PlacedHexagon> hexagons,
            Grid grid)
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

            var neighbors = new Dictionary<int, PlacedHexagon>();
            for (var i = 0; i < neighborsCount; i++)
            for (var j = 0; j < hexagons.Count; j++)
                if (neighborCells[i] == hexagons[j].Cell)
                {
                    if (neighbors.ContainsKey(i))
                        neighbors[i] = hexagons[j];
                    else
                        neighbors.Add(i, hexagons[j]);
                }

            return neighbors;
        }

        public static bool IsThereAnyAvailableMovesLeft(List<PlacedHexagon> hexagons, Grid grid)
        {
            foreach (var placedHexagon in hexagons)
            {
                var color = placedHexagon.Hexagon.color;
                var neighbors = GetNeighborsIndexed(placedHexagon.Cell, hexagons, grid);
                for (var i = 0; i < neighbors.Count; i++)
                {
                    if (!neighbors.ContainsKey(i)) continue;
                    if (neighbors[i].Hexagon.color != color) continue;

                    int key = (i + 1) % 6;
                    if (!neighbors.ContainsKey(key)) continue;
                    var rightFlank = GetNeighborsIndexed(neighbors[key].Cell, hexagons, grid);
                    for (var j = 0; j < 4; j++)
                    {
                        if (!rightFlank.ContainsKey(j)) continue;
                        if (rightFlank[j].Hexagon.color == color)
                            return true;
                    }

                    key = (i + 5) % 6;
                    if (!neighbors.ContainsKey(key)) continue;
                    var leftFlank = GetNeighborsIndexed(neighbors[(i + 5) % 6].Cell, hexagons, grid);
                    for (var j = 3; j < 7; j++)
                    {
                        if (!leftFlank.ContainsKey(j)) continue;
                        if (leftFlank[j % 6].Hexagon.color == color)
                            return true;
                    }
                }
            }

            return false;
        }
    }
}