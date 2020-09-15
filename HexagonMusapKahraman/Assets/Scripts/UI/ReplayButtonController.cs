using HexagonMusapKahraman.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexagonMusapKahraman.UI
{
    public class ReplayButtonController : MonoBehaviour
    {
        [SerializeField] private DynamicData score;
        [SerializeField] private DynamicData move;
        [SerializeField] private DynamicData highScore;

        public void Replay()
        {
            score.ResetValue();
            move.ResetValue();
            SceneManager.LoadScene(0);
        }

        private void OnApplicationQuit()
        {
            score.ResetValue();
            move.ResetValue();
            highScore.ResetValue();
        }
    }
}