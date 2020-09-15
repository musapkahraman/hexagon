using System;
using UnityEngine;

namespace HexagonMusapKahraman.ScriptableObjects
{
    [CreateAssetMenu]
    public class DynamicData : ScriptableObject
    {
        [SerializeField] private int value;

        private void OnValidate()
        {
            SetValue(Mathf.Clamp(value, 0, 999999));
        }

        public event Action<int> ValueChanged;

        public int GetValue()
        {
            return value;
        }

        public void ResetValue()
        {
            value = 0;
        }

        public void SetValue(int newValue)
        {
            value = newValue;
            ValueChanged?.Invoke(value);
        }

        public int IncreaseValue(int amount)
        {
            value += amount;
            ValueChanged?.Invoke(value);
            return value;
        }

        public void SetMaximum(int newValue)
        {
            if (newValue <= value) return;
            value = newValue;
            ValueChanged?.Invoke(value);
        }
    }
}