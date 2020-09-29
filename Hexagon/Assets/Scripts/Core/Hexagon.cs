using UnityEngine;
using UnityEngine.Tilemaps;

namespace HexagonGame.Core
{
    [CreateAssetMenu]
    public class Hexagon : ScriptableObject
    {
        public Tile tile;
        public Color color;
    }
}