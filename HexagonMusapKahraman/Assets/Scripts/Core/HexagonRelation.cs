using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    public class HexagonRelation : MonoBehaviour
    {
        public PlacedHexagon Hexagon;

        private void Update()
        {
            Hexagon.Center = transform.position;
        }
    }
}