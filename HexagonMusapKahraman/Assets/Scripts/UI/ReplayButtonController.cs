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

        private void OnApplicationQuit()
        {
            score.ResetValue();
            move.ResetValue();
            highScore.ResetValue();
        }

        public void Replay()
        {
            score.ResetValue();
            move.ResetValue();
            SceneManager.LoadScene(0);
        }
    }
}