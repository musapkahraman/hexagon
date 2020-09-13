using HexagonMusapKahraman.GridMap;
using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    public class HexagonSprite : MonoBehaviour
    {
        public PlacedHexagon Hexagon;

        private void Update()
        {
            Hexagon.Center = transform.position;
        }
    }
}