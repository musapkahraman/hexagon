using UnityEngine;

namespace HexagonGame.UI
{
    public class GameOverMessage : MonoBehaviour
    {
        private Canvas _canvas;
        private Transform _target;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        public void Show()
        {
            _canvas.enabled = true;
        }
    }
}