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
            var neighbors = NeighborHood.GetNeighborsIndexed(Cell, placedHexagons, grid);
            for (var i = 0; i < 6; i++)
            {
                if (!neighbors.ContainsKey(i)) continue;
                if (!neighbors[i].Hexagon.color.Equals(Hexagon.color)) continue;
                int nextIndex = (i + 1) % 6;
                if (!neighbors.ContainsKey(nextIndex)) continue;
                if (!neighbors[nextIndex].Hexagon.color.Equals(Hexagon.color)) return;
                list.Add(this);
                list.Add(neighbors[i]);
                list.Add(neighbors[nextIndex]);
            }
        }
    }
}