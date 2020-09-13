using UnityEngine;
using UnityEngine.Tilemaps;

namespace HexagonMusapKahraman.ScriptableObjects
{
    [CreateAssetMenu]
    public class Hexagon : ScriptableObject
    {
        public Tile tile;
        public Color color;
    }
}