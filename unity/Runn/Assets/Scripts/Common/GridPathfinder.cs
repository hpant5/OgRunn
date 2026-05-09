using System.Collections.Generic;
using UnityEngine;

namespace Runn.Common
{
    public static class GridPathfinder
    {
        public static List<Vector2Int> FindPath(bool[,] walkable, Vector2Int start, Vector2Int goal)
        {
            int width = walkable.GetLength(0);
            int height = walkable.GetLength(1);

            if (!InBounds(start, width, height) || !InBounds(goal, width, height)) return null;
            if (!walkable[start.x, start.y] || !walkable[goal.x, goal.y]) return null;
            if (start == goal) return new List<Vector2Int> { start };

            var open = new SortedSet<(int f, int tie, Vector2Int p)>(Comparer<(int f, int tie, Vector2Int p)>.Create((a, b) =>
            {
                int c = a.f.CompareTo(b.f);
                if (c != 0) return c;
                c = a.tie.CompareTo(b.tie);
                if (c != 0) return c;
                c = a.p.x.CompareTo(b.p.x);
                if (c != 0) return c;
                return a.p.y.CompareTo(b.p.y);
            }));

            var came = new Dictionary<Vector2Int, Vector2Int>();
            var g = new Dictionary<Vector2Int, int> { [start] = 0 };
            int tieCounter = 0;
            open.Add((Heuristic(start, goal), tieCounter++, start));

            Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

            while (open.Count > 0)
            {
                var current = open.Min;
                open.Remove(current);
                if (current.p == goal) return Reconstruct(came, current.p);

                foreach (var d in dirs)
                {
                    var n = current.p + d;
                    if (!InBounds(n, width, height) || !walkable[n.x, n.y]) continue;
                    int tentative = g[current.p] + 1;
                    if (!g.TryGetValue(n, out int existing) || tentative < existing)
                    {
                        g[n] = tentative;
                        came[n] = current.p;
                        int f = tentative + Heuristic(n, goal);
                        open.Add((f, tieCounter++, n));
                    }
                }
            }

            return null;
        }

        public static Vector2Int RandomWalkable(bool[,] walkable, System.Random rng)
        {
            int width = walkable.GetLength(0);
            int height = walkable.GetLength(1);
            for (int tries = 0; tries < 200; tries++)
            {
                int x = rng.Next(width);
                int y = rng.Next(height);
                if (walkable[x, y]) return new Vector2Int(x, y);
            }
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (walkable[x, y]) return new Vector2Int(x, y);
            return Vector2Int.zero;
        }

        private static int Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private static bool InBounds(Vector2Int p, int w, int h)
        {
            return p.x >= 0 && p.x < w && p.y >= 0 && p.y < h;
        }

        private static List<Vector2Int> Reconstruct(Dictionary<Vector2Int, Vector2Int> came, Vector2Int end)
        {
            var path = new List<Vector2Int> { end };
            while (came.TryGetValue(end, out var prev))
            {
                path.Add(prev);
                end = prev;
            }
            path.Reverse();
            return path;
        }
    }
}
