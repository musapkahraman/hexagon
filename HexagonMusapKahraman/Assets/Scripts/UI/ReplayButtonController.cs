using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexagonMusapKahraman.UI
{
    public class ReplayButtonController : MonoBehaviour
    {
        public void Replay()
        {
            SceneManager.LoadScene(0);
        }
    }
}
