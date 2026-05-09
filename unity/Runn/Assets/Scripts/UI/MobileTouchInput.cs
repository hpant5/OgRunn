using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Runn.UI
{
    public class MobileTouchInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public Joystick MoveStick;
        public float LookSensitivity = 0.25f;

        public Vector2 Move => MoveStick != null ? MoveStick.Value : Vector2.zero;
        public float LookDeltaX { get; private set; }

        private int _lookFingerId = -2;
        private Vector2 _lastLookPos;

        private void LateUpdate()
        {
            LookDeltaX = 0f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_lookFingerId != -2) return;
            _lookFingerId = eventData.pointerId;
            _lastLookPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _lookFingerId) return;
            Vector2 cur = eventData.position;
            float dx = cur.x - _lastLookPos.x;
            _lastLookPos = cur;
            LookDeltaX += dx * LookSensitivity;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == _lookFingerId)
            {
                _lookFingerId = -2;
            }
        }
    }
}
