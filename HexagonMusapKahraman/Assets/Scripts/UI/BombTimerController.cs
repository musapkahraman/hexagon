using System;
using TMPro;
using UnityEngine;

namespace HexagonMusapKahraman.UI
{
    public class BombTimerController : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;
        private Canvas _canvas;
        private Transform _target;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Update()
        {
            if (_target)
                transform.position = _target.position;
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
        
        public void Show(Transform t)
        {
            _canvas.enabled = true;
            _target = t;
        }

        public void Hide()
        {
            _canvas.enabled = false;
            _target = null;
        }
    }
}