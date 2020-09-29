using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexagonGame.Gestures
{
    public static class RotationGesture
    {
        private static bool _rotated;
        private static int _counter;
        private static Vector2 _dragBeginPoint;
        public static event Action<RotationDirection> Rotated;

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