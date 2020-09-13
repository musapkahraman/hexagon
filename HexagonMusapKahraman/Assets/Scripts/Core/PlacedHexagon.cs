using HexagonMusapKahraman.ScriptableObjects;
using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    public class PlacedHexagon
    {
        public readonly Hexagon Hexagon;
        public Vector3 Center;

        public PlacedHexagon(Hexagon hexagon, Vector3 center)
        {
            Hexagon = hexagon;
            Center = center;
        }
    }
}