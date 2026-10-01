using NUnit.Framework;

namespace RPGame.Encounter.Tests
{
    public sealed class ScoreSystemTests
    {
        [Test]
        public void CurrentScore_WhenCreated_IsZero()
        {
            ScoreSystem scoreSystem = new();

            Assert.AreEqual(0, scoreSystem.CurrentScore);
        }

        [Test]
        public void RegisterKill_WhenCostIsPositive_AddsCost()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(3);

            Assert.AreEqual(3, scoreSystem.CurrentScore);
        }

        [Test]
        public void RegisterKill_WhenCalledMultipleTimes_AccumulatesScore()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(2);
            scoreSystem.RegisterKill(5);
            scoreSystem.RegisterKill(1);

            Assert.AreEqual(8, scoreSystem.CurrentScore);
        }

        [Test]
        public void RegisterKill_WhenCostIsZeroOrNegative_DoesNotChangeScore()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(4);
            scoreSystem.RegisterKill(0);
            scoreSystem.RegisterKill(-2);

            Assert.AreEqual(4, scoreSystem.CurrentScore);
        }
    }
}
