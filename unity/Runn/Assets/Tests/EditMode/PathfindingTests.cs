using NUnit.Framework;
using UnityEngine;
using Runn.Common;

namespace Runn.Tests
{
    public class PathfindingTests
    {
        [Test]
        public void FindPathAvoidsWalls()
        {
            var walkable = new bool[5, 5];
            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                    walkable[x, y] = true;

            walkable[2, 1] = false;
            walkable[2, 2] = false;
            walkable[2, 3] = false;

            var path = GridPathfinder.FindPath(walkable, new Vector2Int(1, 2), new Vector2Int(3, 2));

            Assert.NotNull(path);
            Assert.False(path.Exists(p => p.x == 2 && p.y >= 1 && p.y <= 3));
        }
    }
}
