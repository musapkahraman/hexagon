using HexagonMusapKahraman.ScriptableObjects;
using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    public class BombHexagon : PlacedHexagon
    {
        public int Timer;
        
        public BombHexagon(Hexagon hexagon, Vector3 center, int timer) : base(hexagon, center)
        {
            Hexagon = hexagon;
            Center = center;
            Timer = timer;
        }
    }
}