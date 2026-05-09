using NUnit.Framework;
using Runn.Systems;

namespace Runn.Tests
{
    public class V1ConstantsTests
    {
        [Test]
        public void V1GameplayConstantsMatchSpec()
        {
            Assert.AreEqual(2, GameConstants.MapRevealUsesPerLevel);
            Assert.AreEqual(5f, GameConstants.MapRevealDurationSeconds);
            Assert.AreEqual(2, GameConstants.BenchesPerLevel);
            Assert.AreEqual(GameConstants.PlayerSpeed * 0.8f, GameConstants.OgreSpeed, 0.001f);
        }
    }
}
