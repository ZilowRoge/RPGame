const { LeaderboardsApi } = require("@unity-services/leaderboards-1.1");

const leaderboardId = "endless_wave_leaderboard";
const maximumScore = 100000000;
const maximumWavesCompleted = 10000;
const maximumGameVersionLength = 64;

module.exports = async ({ params, context, logger }) => {
  const { score, wavesCompleted, encounterSeed, gameVersion } = params;

  if (!Number.isFinite(score)
      || score < 0
      || score > maximumScore
      || !Number.isInteger(wavesCompleted)
      || wavesCompleted < 0
      || wavesCompleted > maximumWavesCompleted
      || !Number.isInteger(encounterSeed)
      || typeof gameVersion !== "string"
      || gameVersion.length === 0
      || gameVersion.length > maximumGameVersionLength) {
    logger.error("Invalid endless run submission.");
    throw new Error("Invalid endless run submission.");
  }

  try {
    const leaderboardsApi = new LeaderboardsApi(context);
    await leaderboardsApi.addLeaderboardPlayerScore(
      context.projectId,
      leaderboardId,
      context.playerId,
      {
        score,
        metadata: {
          wavesCompleted,
          encounterSeed,
          gameVersion
        }
      });
    return true;
  } catch (error) {
    logger.error("Failed to submit endless run score.", { "error.message": error.message });
    throw error;
  }
};
