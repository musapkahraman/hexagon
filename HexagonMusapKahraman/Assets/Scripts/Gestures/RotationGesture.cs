using System;
using HexagonMusapKahraman.Pointer;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexagonMusapKahraman.Gestures
{
    public static class RotationGesture
    {
        public static event Action<RotationDirection> Rotated;
        private static bool _rotated;
        private static int _counter;
        private static Vector2 _dragBeginPoint;

        public static void OnBeginDrag(PointerEventData eventData)
        {
            _rotated = false;
            _counter = 0;
            _dragBeginPoint = eventData.position;
        }

        public static void OnDrag(PointerEventData eventData, Vector2 center)
        {
            if (_rotated || ++_counter != 2) return;
            _rotated = true;
            Rotated?.Invoke(GetRotationDirection(center, eventData.delta));
        }

        private static RotationDirection GetRotationDirection(Vector2 center, Vector2 delta)
        {
            float signedAngle = Vector2.SignedAngle(center - _dragBeginPoint, delta);
            return signedAngle > 0 ? RotationDirection.Clockwise : RotationDirection.AntiClockwise;
        }
    }
}