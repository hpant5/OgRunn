using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Runn.UI
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public RectTransform Background;
        public RectTransform Knob;
        public float Radius = 80f;

        public Vector2 Value { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateValue(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateValue(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Value = Vector2.zero;
            if (Knob != null) Knob.anchoredPosition = Vector2.zero;
        }

        private void UpdateValue(PointerEventData eventData)
        {
            if (Background == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Background, eventData.position, eventData.pressEventCamera, out var local);
            Vector2 v = local;
            float r = Mathf.Min(Background.sizeDelta.x, Background.sizeDelta.y) * 0.5f;
            if (r <= 0f) r = Radius;
            if (v.magnitude > r) v = v.normalized * r;
            if (Knob != null) Knob.anchoredPosition = v;
            Value = v / r;
        }
    }
}
