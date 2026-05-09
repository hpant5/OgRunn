using UnityEngine;
using Runn.Systems;
using Runn.Common;

namespace Runn.Level
{
    public static class LevelLoader
    {
        public static LevelData LoadFromResources(int oneBasedIndex)
        {
            var asset = Resources.Load<TextAsset>($"Level{oneBasedIndex}");
            if (asset == null)
            {
                Debug.LogError($"[Runn] Missing level resource: Level{oneBasedIndex}.txt");
                return null;
            }
            return Parse(asset.text);
        }

        public static LevelData Parse(string text)
        {
            var lines = text.Replace("\r", "").Split('\n');
            int height = 0;
            int width = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) continue;
                height++;
                if (lines[i].Length > width) width = lines[i].Length;
            }

            var data = new LevelData
            {
                Width = width,
                Height = height,
                Walkable = new bool[width, height]
            };

            int playerCount = 0;
            int ogreCount = 0;
            int gateCount = 0;
            int row = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;
                int gridY = height - 1 - row;
                for (int x = 0; x < width; x++)
                {
                    char c = x < line.Length ? line[x] : '#';
                    var cell = new Vector2Int(x, gridY);
                    switch (c)
                    {
                        case '#':
                            data.Walls.Add(cell);
                            data.Walkable[x, gridY] = false;
                            break;
                        case 'P':
                            playerCount++;
                            data.PlayerSpawn = cell;
                            data.Walkable[x, gridY] = true;
                            break;
                        case 'O':
                            ogreCount++;
                            data.OgreSpawn = cell;
                            data.Walkable[x, gridY] = true;
                            break;
                        case 'B':
                            data.Benches.Add(cell);
                            data.Walkable[x, gridY] = true;
                            break;
                        case 'G':
                        case 'E':
                            gateCount++;
                            data.Exit = cell;
                            data.Walkable[x, gridY] = true;
                            break;
                        default:
                            data.Walkable[x, gridY] = true;
                            break;
                    }
                }
                row++;
            }

            Validate(data, playerCount, ogreCount, gateCount);
            return data;
        }

        private static void Validate(LevelData data, int playerCount, int ogreCount, int gateCount)
        {
            if (playerCount != 1) Debug.LogError($"[Runn] Level must contain exactly one P. Found {playerCount}.");
            if (ogreCount != 1) Debug.LogError($"[Runn] Level must contain exactly one O. Found {ogreCount}.");
            if (gateCount != 1) Debug.LogError($"[Runn] Level must contain exactly one G. Found {gateCount}.");
            if (data.Benches.Count != GameConstants.BenchesPerLevel)
                Debug.LogError($"[Runn] Level must contain exactly {GameConstants.BenchesPerLevel} B tiles. Found {data.Benches.Count}.");

            if (playerCount == 1 && gateCount == 1 && GridPathfinder.FindPath(data.Walkable, data.PlayerSpawn, data.Exit) == null)
                Debug.LogError("[Runn] Player spawn cannot reach gate.");
            if (ogreCount == 1 && playerCount == 1 && GridPathfinder.FindPath(data.Walkable, data.OgreSpawn, data.PlayerSpawn) == null)
                Debug.LogError("[Runn] Ogre spawn is not connected to player spawn.");
        }
    }
}
