using System.Collections.Generic;
using HexagonMusapKahraman.Core;
using UnityEngine;

namespace HexagonMusapKahraman.GridMap
{
    public static class NeighborHood
    {
        public static List<PlacedHexagon> GetNeighbors(Vector3 point, List<PlacedHexagon> hexagons, int count)
        {
            var distances = new SortedList<float, PlacedHexagon>();
            for (var i = 0; i < hexagons.Count; i++)
            {
                var hexagon = hexagons[i];
                float sqrMagnitude = Vector3.SqrMagnitude(point - hexagon.Center);

                while (distances.ContainsKey(sqrMagnitude))
                    sqrMagnitude += 0.001f;

                distances.Add(sqrMagnitude, hexagon);
            }

            var neighbors = new List<PlacedHexagon>();
            for (var i = 0; neighbors.Count < count; i++)
            {
                if (i > 1)
                {
                    float distanceBetweenLegs =
                        Vector3.SqrMagnitude(neighbors[1].Center - distances[distances.Keys[i]].Center);
                    if (distanceBetweenLegs > 2) continue;
                }

                neighbors.Add(distances[distances.Keys[i]]);
            }

            return neighbors;
        }

        public static List<PlacedHexagon> GetNeighbors(Vector3 cellCenterWorld, List<PlacedHexagon> hexagons,
            Grid grid)
        {
            var cellSize = grid.cellSize;
            float height = cellSize.x;
            float width = cellSize.y * 0.8f;
            var neighborCenterPoints = new List<Vector3>(6)
            {
                grid.GetCellCenterWorld(grid.WorldToCell(cellCenterWorld + new Vector3(0, height, 0))),
                grid.GetCellCenterWorld(grid.WorldToCell(cellCenterWorld + new Vector3(width, height * 0.5f, 0))),
                grid.GetCellCenterWorld(grid.WorldToCell(cellCenterWorld + new Vector3(width, -height * 0.5f, 0))),
                grid.GetCellCenterWorld(grid.WorldToCell(cellCenterWorld + new Vector3(0, -height, 0))),
                grid.GetCellCenterWorld(grid.WorldToCell(cellCenterWorld + new Vector3(-width, -height * 0.5f, 0))),
                grid.GetCellCenterWorld(grid.WorldToCell(cellCenterWorld + new Vector3(-width, height * 0.5f, 0)))
            };
            var neighbors = new List<PlacedHexagon>();
            for (var i = 0; i < neighborCenterPoints.Count; i++)
            for (var j = 0; j < hexagons.Count; j++)
            {
                float distance = Vector3.Distance(hexagons[j].Center, neighborCenterPoints[i]);
                if (distance < 0.5f) neighbors.Add(hexagons[j]);
            }

            return neighbors;
        }

        public static bool GetBottomLeftNeighbor(Vector3 cellCenterWorld, List<PlacedHexagon> hexagons, Grid grid,
            out PlacedHexagon neighbor)
        {
            var cellSize = grid.cellSize;
            float height = cellSize.x;
            float width = cellSize.y * 0.8f;
            var neighborWorldPoint = cellCenterWorld + new Vector3(-width, -height * 0.5f, 0);
            var neighborCell = grid.WorldToCell(neighborWorldPoint);
            var neighborCenterPoint = grid.GetCellCenterWorld(neighborCell);

            for (var j = 0; j < hexagons.Count; j++)
            {
                float distance = Vector3.Distance(hexagons[j].Center, neighborCenterPoint);
                if (!(distance < 0.5f)) continue;
                neighbor = hexagons[j];
                return true;
            }

            neighbor = new PlacedHexagon(null, Vector3.negativeInfinity);
            return false;
        }
    }
}