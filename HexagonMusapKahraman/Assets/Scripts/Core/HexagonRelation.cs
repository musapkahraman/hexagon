using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    public class HexagonRelation : MonoBehaviour
    {
        private Grid _grid;
        public PlacedHexagon Hexagon;

        private void Update()
        {
            Hexagon.Cell = _grid.WorldToCell(transform.position);
        }

        public void SetGrid(Grid grid)
        {
            _grid = grid;
        }
    }
}