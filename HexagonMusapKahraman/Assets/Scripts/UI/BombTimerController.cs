using TMPro;
using UnityEngine;

namespace HexagonMusapKahraman.UI
{
    public class BombTimerController : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        public void SetTimerText(int time)
        {
            timerText.text = time.ToString();
        }

        public void Show(Vector3 position)
        {
            _canvas.enabled = true;
            transform.position = position;
        }

        public void Hide()
        {
            _canvas.enabled = false;
        }
    }
}