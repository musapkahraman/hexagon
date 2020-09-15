using System.Collections.Generic;
using HexagonMusapKahraman.GridMap;
using HexagonMusapKahraman.ScriptableObjects;
using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    public class PlacedHexagon
    {
        public Vector3Int Cell;
        public Hexagon Hexagon;

        public PlacedHexagon(Hexagon hexagon, Vector3Int cell)
        {
            Hexagon = hexagon;
            Cell = cell;
        }

        public void CheckForMatch(Grid grid, List<PlacedHexagon> placedHexagons, ISet<PlacedHexagon> list)
        {
            var cellSize = grid.cellSize;
            float gridDistance = Mathf.Min(cellSize.x, cellSize.y) * 1.5f;
            var neighbors = NeighborHood.GetNeighbors(Cell, placedHexagons, grid);
            for (var i = 0; i < neighbors.Count; i++)
            {
                if (!neighbors[i].Hexagon.color.Equals(Hexagon.color)) continue;
                int nextIndex = i == neighbors.Count - 1 ? 0 : i + 1;
                if (!neighbors[nextIndex].Hexagon.color.Equals(Hexagon.color)) return;
                var nextNeighborCenter = grid.GetCellCenterWorld(neighbors[nextIndex].Cell);
                var neighborCenter = grid.GetCellCenterWorld(neighbors[i].Cell);
                float distance = Vector3.Distance(nextNeighborCenter, neighborCenter);
                if (distance > gridDistance) return;
                list.Add(this);
                list.Add(neighbors[i]);
                list.Add(neighbors[nextIndex]);
            }
        }
    }
}