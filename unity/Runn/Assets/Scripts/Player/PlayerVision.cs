using UnityEngine;

namespace Runn.Player
{
    public class PlayerVision : MonoBehaviour
    {
        public float ViewDistance = Runn.Systems.GameConstants.PlayerVisibilityRadius;
        public float FovDegrees = 360f;
        public LayerMask WallMask;

        public bool CanSee(Transform target)
        {
            if (target == null) return false;
            Vector3 origin = transform.position + Vector3.up * 1.6f;
            Vector3 to = target.position + Vector3.up * 1.6f - origin;
            float dist = to.magnitude;
            if (dist > ViewDistance) return false;
            Vector3 dir = to / Mathf.Max(dist, 0.0001f);
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle > FovDegrees * 0.5f) return false;
            if (Physics.Raycast(origin, dir, out var hit, dist, WallMask)) return false;
            return true;
        }
    }
}
