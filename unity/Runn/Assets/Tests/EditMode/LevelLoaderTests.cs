using NUnit.Framework;
using Runn.Level;
using Runn.Systems;

namespace Runn.Tests
{
    public class LevelLoaderTests
    {
        [Test]
        public void ParseReadsV1Markers()
        {
            const string level =
                "#####\n" +
                "#O.G#\n" +
                "#B.B#\n" +
                "#..P#\n" +
                "#####";

            var data = LevelLoader.Parse(level);

            Assert.AreEqual(5, data.Width);
            Assert.AreEqual(5, data.Height);
            Assert.AreEqual(3, data.PlayerSpawn.x);
            Assert.AreEqual(1, data.PlayerSpawn.y);
            Assert.AreEqual(1, data.OgreSpawn.x);
            Assert.AreEqual(3, data.OgreSpawn.y);
            Assert.AreEqual(3, data.Exit.x);
            Assert.AreEqual(GameConstants.BenchesPerLevel, data.Benches.Count);
        }
    }
}
