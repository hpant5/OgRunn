using UnityEngine;

namespace Runn.Level
{
    public class MazeBuilder : MonoBehaviour
    {
        public LayerMask WallLayer;

        public Transform Build(LevelData data, Material wallMat, Material floorMat)
        {
            var root = new GameObject("Maze").transform;
            root.SetParent(transform, false);

            float worldW = data.Width * LevelData.TileSize;
            float worldH = data.Height * LevelData.TileSize;

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(root, false);
            floor.transform.position = new Vector3(worldW * 0.5f, 0f, worldH * 0.5f);
            floor.transform.localScale = new Vector3(worldW * 0.1f, 1f, worldH * 0.1f);
            if (floorMat != null) floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            int wallLayerIdx = LayerForMask(WallLayer);
            foreach (var w in data.Walls)
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Wall_{w.x}_{w.y}";
                wall.transform.SetParent(root, false);
                wall.transform.position = new Vector3((w.x + 0.5f) * LevelData.TileSize, LevelData.WallHeight * 0.5f, (w.y + 0.5f) * LevelData.TileSize);
                wall.transform.localScale = new Vector3(LevelData.TileSize, LevelData.WallHeight, LevelData.TileSize);
                if (wallLayerIdx >= 0) wall.layer = wallLayerIdx;
                if (wallMat != null) wall.GetComponent<Renderer>().sharedMaterial = wallMat;
            }

            return root;
        }

        private static int LayerForMask(LayerMask mask)
        {
            int v = mask.value;
            if (v == 0) return -1;
            for (int i = 0; i < 32; i++) if ((v & (1 << i)) != 0) return i;
            return -1;
        }
    }
}
