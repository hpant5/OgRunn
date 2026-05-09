using UnityEngine;

namespace Runn.Level
{
    public class ExitDoor : MonoBehaviour
    {
        public float UseRadius = 1.6f;
        public bool RequiresKey;

        public bool CanExit(Vector3 playerPos, bool hasKey)
        {
            if (RequiresKey && !hasKey) return false;
            var d = playerPos - transform.position;
            d.y = 0f;
            return d.sqrMagnitude <= UseRadius * UseRadius;
        }
    }
}
