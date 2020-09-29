using HexagonGame.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexagonGame.UI
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
            Time.timeScale = 1;
            score.ResetValue();
            move.ResetValue();
            SceneManager.LoadScene(0);
        }
    }
}