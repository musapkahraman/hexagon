using UnityEngine;

namespace HexagonMusapKahraman.Core
{
    public class HexagonGroupController : MonoBehaviour
    {
        [SerializeField] private GameObject rotatingSpritePrefab;

        public void ShowAtCenter(Vector3 center)
        {
            Instantiate(rotatingSpritePrefab, center, Quaternion.identity);
        }
    }
}