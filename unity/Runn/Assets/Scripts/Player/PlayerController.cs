using UnityEngine;
using Runn.Systems;
using Runn.UI;

namespace Runn.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float WalkSpeed = GameConstants.PlayerSpeed;
        public float Gravity = -20f;

        public Transform CameraRig;
        public MobileTouchInput Input;

        private CharacterController _cc;
        private float _verticalVel;
        private float _yaw;
        public bool Frozen;
        public Vector3 LastMoveDir { get; private set; }

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _yaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            if (Frozen)
            {
                _verticalVel = 0f;
                LastMoveDir = Vector3.zero;
                return;
            }

            Vector2 move = Input != null ? Input.Move : Vector2.zero;

            if (Application.isEditor)
            {
                float kx = (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f)
                    + (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow) ? 1f : 0f);
                float ky = (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow) ? -1f : 0f)
                    + (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow) ? 1f : 0f);
                if (Mathf.Abs(kx) > 0.001f || Mathf.Abs(ky) > 0.001f) move = new Vector2(kx, ky);
            }

            float speed = WalkSpeed;

            Vector3 desired = new Vector3(move.x, 0f, move.y);
            if (desired.sqrMagnitude > 1f) desired.Normalize();
            LastMoveDir = desired;
            if (desired.sqrMagnitude > 0.0001f)
            {
                _yaw = Mathf.Atan2(desired.x, desired.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            }

            if (_cc.isGrounded && _verticalVel < 0f) _verticalVel = -2f;
            _verticalVel += Gravity * Time.deltaTime;

            Vector3 vel = desired * speed;
            vel.y = _verticalVel;
            _cc.Move(vel * Time.deltaTime);
        }
    }
}
