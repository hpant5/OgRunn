using UnityEngine;

namespace Runn.Player
{
    public class FollowCamera : MonoBehaviour
    {
        public Transform Target;
        public Vector3 Offset = new Vector3(0f, 15f, -13f);
        public Vector3 LookOffset = new Vector3(0f, 0.6f, 4f);
        public float Smooth = 12f;

        private void LateUpdate()
        {
            if (Target == null) return;

            Vector3 desired = Target.position + Offset;
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-Smooth * Time.deltaTime));
            transform.LookAt(Target.position + LookOffset, Vector3.up);
        }
    }
}
