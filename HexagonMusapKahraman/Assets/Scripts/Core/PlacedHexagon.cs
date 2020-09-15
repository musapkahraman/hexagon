using System.Collections.Generic;
using HexagonMusapKahraman.GridMap;
using HexagonMusapKahraman.ScriptableObjects;
using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    public class PlacedHexagon
    {
        public Hexagon Hexagon;
        public Vector3 Center;

        public PlacedHexagon(Hexagon hexagon, Vector3 center)
        {
            Hexagon = hexagon;
            Center = center;
        }

        public void CheckForThreeMatch(Grid grid, List<PlacedHexagon> placedHexagons, ISet<PlacedHexagon> list)
        {
            var cellSize = grid.cellSize;
            float gridDistance = Mathf.Min(cellSize.x, cellSize.y) * 1.5f;
            var neighbors = NeighborHood.GetNeighbors(Center, placedHexagons, grid);
            for (var i = 0; i < neighbors.Count; i++)
            {
                if (!neighbors[i].Hexagon.color.Equals(Hexagon.color)) continue;
                int nextIndex = i == neighbors.Count - 1 ? 0 : i + 1;
                if (!neighbors[nextIndex].Hexagon.color.Equals(Hexagon.color)) return;
                float distance = Vector3.Distance(neighbors[nextIndex].Center, neighbors[i].Center);
                if (!(distance <= gridDistance)) return;
                list.Add(this);
                list.Add(neighbors[i]);
                list.Add(neighbors[nextIndex]);
            }
        }
    }
}