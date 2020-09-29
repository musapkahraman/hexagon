using UnityEngine;

namespace HexagonGame.Core
{
    public class BombHexagon : PlacedHexagon
    {
        public int Timer;

        public BombHexagon(Hexagon hexagon, Vector3Int cell, int timer) : base(hexagon, cell)
        {
            Hexagon = hexagon;
            Cell = cell;
            Timer = timer;
        }
    }
}