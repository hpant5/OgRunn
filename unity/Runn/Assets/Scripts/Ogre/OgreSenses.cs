using UnityEngine;

namespace Runn.Ogre
{
    public class OgreSenses : MonoBehaviour
    {
        public float ViewDistance = Runn.Systems.GameConstants.OgreDetectionRadius;
        public float FovDegrees = 360f;
        public float EyeHeight = 1.8f;
        public LayerMask WallMask;

        public bool CanSee(Transform target, bool targetHidden)
        {
            if (target == null || targetHidden) return false;
            Vector3 origin = transform.position + Vector3.up * EyeHeight;
            Vector3 to = target.position + Vector3.up * 1.4f - origin;
            float dist = to.magnitude;
            if (dist > ViewDistance) return false;
            Vector3 dir = to / Mathf.Max(dist, 0.0001f);
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle > FovDegrees * 0.5f) return false;
            if (Physics.Raycast(origin, dir, out _, dist, WallMask)) return false;
            return true;
        }
    }
}
