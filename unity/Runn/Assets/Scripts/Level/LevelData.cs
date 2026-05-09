using System.Collections.Generic;
using UnityEngine;
using Runn.Systems;

namespace Runn.Level
{
    public class LevelData
    {
        public const float TileSize = GameConstants.TileSize;
        public const float WallHeight = GameConstants.WallHeight;

        public int Width;
        public int Height;
        public bool[,] Walkable;
        public Vector2Int PlayerSpawn;
        public Vector2Int OgreSpawn;
        public Vector2Int Exit;
        public List<Vector2Int> Benches = new List<Vector2Int>();
        public List<Vector2Int> Walls = new List<Vector2Int>();

        public Vector3 GridToWorld(Vector2Int g, float y = 0f)
        {
            return new Vector3((g.x + 0.5f) * TileSize, y, (g.y + 0.5f) * TileSize);
        }

        public Vector2Int WorldToGrid(Vector3 world)
        {
            int x = Mathf.Clamp(Mathf.FloorToInt(world.x / TileSize), 0, Width - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(world.z / TileSize), 0, Height - 1);
            return new Vector2Int(x, y);
        }
    }
}
