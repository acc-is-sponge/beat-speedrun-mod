using System;
using BS_Utils.Gameplay;
using BS_Utils.Utilities;
using BeatSpeedrun.Models.Speedrun;
using Zenject;
using BeatSpeedrun.Models;
using System.Threading.Tasks;
using IPA.Utilities;

namespace BeatSpeedrun.Managers
{
    internal class LevelPlayManager : IInitializable, IDisposable
    {
        private readonly CurrentSpeedrunManager _currrntSpeedrunManager;
        private readonly CustomLevelLoader _customLevelLoader;
        private readonly PlayerDataModel _playerDataModel;

        public LevelPlayManager(
            CurrentSpeedrunManager currrntSpeedrunManager,
            CustomLevelLoader customLevelLoader,
            PlayerDataModel playerDataModel)
        {
            _currrntSpeedrunManager = currrntSpeedrunManager;
            _customLevelLoader = customLevelLoader;
            _playerDataModel = playerDataModel;
        }

        public void Initialize()
        {
            BSEvents.gameSceneLoaded += OnGameSceneLoaded;
            BSEvents.levelCleared += OnLevelCleared;
            BSEvents.songPaused += OnSongPaused;
        }

        public void Dispose()
        {
            BSEvents.songPaused -= OnSongPaused;
            BSEvents.levelCleared -= OnLevelCleared;
            BSEvents.gameSceneLoaded -= OnGameSceneLoaded;
        }

        private bool _didPauseOnThisGame;

        private void OnGameSceneLoaded()
        {
            Plugin.Log.Debug("OnGameSceneLoaded. reset UsePause flag");
            _didPauseOnThisGame = false;
        }

        private void OnSongPaused()
        {
            Plugin.Log.Info("OnSongPaused. set UsePause flag");
            _didPauseOnThisGame = true;
        }

        private void OnLevelCleared(
            StandardLevelScenesTransitionSetupDataSO so,
            LevelCompletionResults results)
        {
            _ = AddScoreAsync(so, results);
        }

        private async Task AddScoreAsync(
            StandardLevelScenesTransitionSetupDataSO so,
            LevelCompletionResults results)
        {
            try
            {
                var speedrun = _currrntSpeedrunManager.Current;
                if (speedrun == null) return;

                if (ScoreSubmission.Disabled || ScoreSubmission.WasDisabled || ScoreSubmission.ProlongedDisabled)
                {
                    Plugin.Log.Debug($"ScoreSubmission is disabled by {ScoreSubmission.LastDisabledModString}. skipping...");
                    return;
                }

                if (so.gameMode != "Solo" && so.gameMode != "Multiplayer")
                {
                    Plugin.Log.Debug("Currently only Solo/Multiplayer gameMode is supported. skipping...");
                    return;
                }

                if (so.practiceSettings != null)
                {
                    Plugin.Log.Debug($"PracticeSettings is on. skipping...");
                    return;
                }

                if (results.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared)
                {
                    Plugin.Log.Debug($"levelEndStateType={results.levelEndStateType}. skipping...");
                    return;
                }

                if (results.gameplayModifiers.zenMode)
                {
                    Plugin.Log.Debug($"Zen mode. skipping...");
                    return;
                }

                var level = so.difficultyBeatmap.level;

                if (!(level is CustomBeatmapLevel))
                {
                    Plugin.Log.Debug($"Currently only CustomBeatmapLevel is supported. skipping...");
                    return;
                }

                var defaultEnvironment = FieldAccessor<CustomLevelLoader, EnvironmentInfoSO>.Get(_customLevelLoader, "_defaultEnvironmentInfo");
                var beatmapData = await so.difficultyBeatmap.GetBeatmapDataAsync(defaultEnvironment, _playerDataModel.playerData.playerSpecificSettings);

                var maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(beatmapData);
                if (maxScore <= 0) return;

                var levelInfo = level.levelID.Split('_');
                var difficulty = so.difficultyBeatmap.difficulty;
                var characteristic = $"Solo{so.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}";
                var difficultyRaw = new DifficultyRaw(difficulty, characteristic);
                var accuracy = (float)results.multipliedScore / maxScore;
                var modifiers = new SnapshotModifiers
                {
                    UsePause = _didPauseOnThisGame,
                    BatteryEnergy = results.gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery,
                    NoFail = results.gameplayModifiers.noFailOn0Energy && results.energy == 0,
                    InstaFail = results.gameplayModifiers.instaFail,
                    NoObstacles = results.gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles,
                    NoBombs = results.gameplayModifiers.noBombs,
                    StrictAngles = results.gameplayModifiers.strictAngles,
                    DisappearingArrows = results.gameplayModifiers.disappearingArrows,
                    FasterSong = results.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Faster,
                    SlowerSong = results.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Slower,
                    NoArrows = results.gameplayModifiers.noArrows,
                    GhostNotes = results.gameplayModifiers.ghostNotes,
                    SuperFastSong = results.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.SuperFast,
                    ProMode = results.gameplayModifiers.proMode,
                    SmallCubes = results.gameplayModifiers.smallCubes,
                };
                var score = new SnapshotSongScore
                {
                    CompletedAt = DateTime.UtcNow,
                    SongHash = levelInfo[2],
                    DifficultyRaw = difficultyRaw.ToString(),
                    BaseAccuracy = accuracy,
                    BadCutCount = results.badCutsCount,
                    MissNoteCount = results.missedCount,
                    FullCombo = results.fullCombo,
                    Modifiers = modifiers,
                };

                Plugin.Log.Debug("Trying to add a song score to the current speedrun...");
                speedrun.AddScore(score);
                _currrntSpeedrunManager.Save();
                Plugin.Log.Debug("Done.");

                _currrntSpeedrunManager.NotifyUpdated();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Failed to add score:\n{ex}");
            }
        }
    }
}
