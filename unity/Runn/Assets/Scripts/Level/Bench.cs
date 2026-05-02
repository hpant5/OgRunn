using UnityEngine;

namespace Runn.Level
{
    public class Bench : MonoBehaviour
    {
        public Vector2Int GridPos;
        public float HideRadius = LevelData.TileSize * 1.05f;

        public bool PlayerInRange(Vector3 playerPos)
        {
            var d = playerPos - transform.position;
            d.y = 0f;
            return d.sqrMagnitude <= HideRadius * HideRadius;
        }
    }
}
