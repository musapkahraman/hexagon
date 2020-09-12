using UnityEngine;

namespace HexagonMusapKahraman.ScriptableObjects
{
    [CreateAssetMenu]
    public class Hexagon : ScriptableObject
    {
        [SerializeField] private Color32 color;
    }
}