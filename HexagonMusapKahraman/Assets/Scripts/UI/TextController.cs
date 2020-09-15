using HexagonMusapKahraman.ScriptableObjects;
using TMPro;
using UnityEngine;

namespace HexagonMusapKahraman.UI
{
    public class TextController : MonoBehaviour
    {
        [SerializeField] private DynamicData data;
        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            data.ValueChanged += OnDynamicDataChanged;
        }

        private void Start()
        {
            OnDynamicDataChanged(data.GetValue());
        }

        private void OnDestroy()
        {
            data.ValueChanged -= OnDynamicDataChanged;
        }

        private void OnDynamicDataChanged(int value)
        {
            _text.text = value.ToString();
        }
    }
}