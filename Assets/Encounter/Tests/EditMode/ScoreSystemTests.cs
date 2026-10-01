using NUnit.Framework;

namespace RPGame.Encounter.Tests
{
    public sealed class ScoreSystemTests
    {
        private const float ComboWindowSeconds = 5f;
        private const float MultiplierTolerance = 0.0001f;
        private const float ScoreTolerance = 0.0001f;

        [Test]
        public void CurrentScore_WhenCreated_IsZero()
        {
            ScoreSystem scoreSystem = new();

            Assert.AreEqual(0f, scoreSystem.CurrentScore, ScoreTolerance);
        }

        [Test]
        public void CurrentMultiplier_WhenCreated_IsOne()
        {
            ScoreSystem scoreSystem = new();

            Assert.AreEqual(1f, scoreSystem.CurrentMultiplier, MultiplierTolerance);
        }

        [Test]
        public void RegisterKill_WhenFirstKill_ScoresAtInitialMultiplier()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(3);

            Assert.AreEqual(3f, scoreSystem.CurrentScore, ScoreTolerance);
        }

        [Test]
        public void RegisterKill_WhenSecondKillIsWithinWindow_ScoresAtIncreasedMultiplier()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(10);
            scoreSystem.RegisterKill(10);

            Assert.AreEqual(21f, scoreSystem.CurrentScore, ScoreTolerance);
        }

        [Test]
        public void RegisterKill_WhenThirdKillIsWithinWindow_ScoresAtNextIncreasedMultiplier()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(10);
            scoreSystem.RegisterKill(10);
            scoreSystem.RegisterKill(10);

            Assert.AreEqual(33f, scoreSystem.CurrentScore, ScoreTolerance);
        }

        [Test]
        public void RegisterKill_WhenCalledMultipleTimes_AccumulatesScore()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(2);
            scoreSystem.RegisterKill(5);
            scoreSystem.RegisterKill(1);

            Assert.AreEqual(8.7f, scoreSystem.CurrentScore, ScoreTolerance);
        }

        [Test]
        public void RegisterKill_WhenMultipliedScoreIsFractional_KeepsFraction()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(10);
            scoreSystem.RegisterKill(15);

            Assert.AreEqual(26.5f, scoreSystem.CurrentScore, ScoreTolerance);
        }

        [Test]
        public void RegisterKill_WhenValidKillHappens_RefreshesComboWindow()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(10);
            scoreSystem.Tick(ComboWindowSeconds - 0.25f);
            scoreSystem.RegisterKill(10);
            scoreSystem.Tick(ComboWindowSeconds - 0.25f);
            scoreSystem.RegisterKill(10);

            Assert.AreEqual(33f, scoreSystem.CurrentScore, ScoreTolerance);
        }

        [Test]
        public void Tick_WhenComboWindowExpires_ResetsMultiplier()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(10);
            scoreSystem.Tick(ComboWindowSeconds);

            Assert.AreEqual(1f, scoreSystem.CurrentMultiplier, MultiplierTolerance);
        }

        [Test]
        public void RegisterKill_WhenCalledAfterTimeout_StartsNewComboAtInitialMultiplier()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(10);
            scoreSystem.Tick(ComboWindowSeconds);
            scoreSystem.RegisterKill(10);

            Assert.AreEqual(20f, scoreSystem.CurrentScore, ScoreTolerance);
        }

        [Test]
        public void RegisterKill_WhenCostIsZeroOrNegative_DoesNotChangeScoreComboOrTimer()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(4);
            scoreSystem.RegisterKill(0);
            scoreSystem.RegisterKill(-2);

            Assert.AreEqual(4f, scoreSystem.CurrentScore, ScoreTolerance);
            Assert.AreEqual(1.1f, scoreSystem.CurrentMultiplier, MultiplierTolerance);
        }

        [Test]
        public void RegisterKill_WhenInvalidCostHappensNearTimeout_DoesNotRefreshComboWindow()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(10);
            scoreSystem.Tick(ComboWindowSeconds - 0.1f);
            scoreSystem.RegisterKill(0);
            scoreSystem.RegisterKill(-5);
            scoreSystem.Tick(0.2f);
            scoreSystem.RegisterKill(10);

            Assert.AreEqual(20f, scoreSystem.CurrentScore, ScoreTolerance);
        }

        [Test]
        public void Tick_WhenDeltaTimeIsZeroOrNegative_DoesNotChangeComboState()
        {
            ScoreSystem scoreSystem = new();

            scoreSystem.RegisterKill(10);
            scoreSystem.Tick(0f);
            scoreSystem.Tick(-1f);
            scoreSystem.Tick(ComboWindowSeconds - 0.1f);
            scoreSystem.RegisterKill(10);

            Assert.AreEqual(21f, scoreSystem.CurrentScore, ScoreTolerance);
        }
    }
}
