using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Graphics.Backgrounds;
using Quaver.Graphics.Notifications;
using Quaver.Modifiers;
using Quaver.Screens.Gameplay;
using Quaver.Screens.Select;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Screens;

namespace Quaver.Screens.Result
{
    public class ResultScreen : QuaverScreen
    {
        /// <summary>
        ///     The type of results screen that this is for.
        /// </summary>
        public ResultScreenType ResultsType { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Results;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///    Reference to the gameplay screen, if the user was previous playing
        /// </summary>
        public GameplayScreen Screen { get; }

        /// <summary>
        ///     Reference to the score if the user is coming from a scoreboard score.
        /// </summary>
        public Score Score { get; }

        /// <summary>
        ///     Reference to the replay, if the user is coming from a replay file.
        /// </summary>
        public Replay Replay { get; private set; }

        /// <summary>
        ///     The ScoreProcesor that is used as a container for all scoring values
        /// </summary>
        public ScoreProcessor ScoreProcessor { get; private set; }

        /// <summary>
        ///     Reference to the map that was played for this score.
        /// </summary>
        public Map Map => MapManager.Selected.Value;

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultScreen(GameplayScreen screen)
        {
            Screen = screen;
            ResultsType = ResultScreenType.Gameplay;
            ScoreProcessor = Screen.Ruleset.ScoreProcessor;

            InitializeIfGameplayType();
            View = new ResultScreenView(this);
        }

        /// <summary>
        /// </summary>
        /// <param name="score"></param>
        public ResultScreen(Score score)
        {
            Score = score;
            ResultsType = ResultScreenType.Score;
            ScoreProcessor = new ScoreProcessorKeys(score.ToReplay());

            InitializeIfScoreType();
            View = new ResultScreenView(this);
        }

        /// <summary>
        /// </summary>
        /// <param name="replay"></param>
        public ResultScreen(Replay replay)
        {
            Replay = replay;
            Console.WriteLine(replay.Date);
            ResultsType = ResultScreenType.Replay;
            ScoreProcessor = new ScoreProcessorKeys(replay);

            InitializeIfReplayType();
            View = new ResultScreenView(this);
        }

        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            MakeCursorVisible();

            BackgroundHelper.Background.BrightnessSprite.ClearAnimations();
            BackgroundHelper.Background.BrightnessSprite.Alpha = 1;

            var view = View as ResultScreenView;
            view?.HandleBackgroundChange();

            base.OnFirstUpdate();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();
            base.Update(gameTime);
        }

        /// <summary>
        ///     Handles input for the entire screen.
        /// </summary>
        private void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Exit(() => new SelectScreen());
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InMenus, -1, "", (byte) GameMode.Keys4, "", 0);

        /// <summary>
        ///     Makes sure the cursor is visible, as it is usually hidden in gameplay)
        /// </summary>
        private void MakeCursorVisible()
        {
            var game = GameBase.Game as QuaverGame;
            var cursor = game?.GlobalUserInterface.Cursor;
            cursor.Alpha = 1;
        }

        /// <summary>
        ///     If we're coming from gameplay, the screen gets intiialized this way.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void InitializeIfGameplayType()
        {
            if (ResultsType != ResultScreenType.Gameplay)
                throw new InvalidOperationException("Cannot call this if ResultsType is not Gameplay");

            // Stop `capturing` the replay. Not sure if this is necessary since update isn't even called and
            // it wouldn't be capturing anyway.
            Screen.ReplayCapturer.ShouldCapture = false;

            // Keep the same replay and score processor if the user was watching a replay before,
            if (Screen.InReplayMode)
            {
                Replay = Screen.LoadedReplay;
                ScoreProcessor = Replay.Mods.HasFlag(ModIdentifier.Autoplay) ? Screen.Ruleset.ScoreProcessor : new ScoreProcessorKeys(Replay);
                ScoreProcessor.Stats = Screen.Ruleset.ScoreProcessor.Stats;

                // Remove all modifiers that was played during the replay, so the user doesn't still have them
                // activated when they go back to select.
                ModManager.RemoveAllMods();
                return;
            }

            // User has played the map, and wasn't watching a replay, so we can proceed normally.

            // Set the replay that the user has generated
            Replay = Screen.ReplayCapturer.Replay;
            Replay.PauseCount = Screen.PauseCount;

            ScoreProcessor = Screen.Ruleset.ScoreProcessor;

            // Remove paused modifier if enabled.
            if (ModManager.IsActivated(ModIdentifier.Paused))
                ModManager.RemoveMod(ModIdentifier.Paused);

            SubmitScore();
        }

        /// <summary>
        ///     If we're coming from a `Score`, then the screen should be initialized with this method
        /// </summary>
        private void InitializeIfScoreType()
        {
            if (ResultsType != ResultScreenType.Score)
                throw new InvalidOperationException("Cannot call this if ResultsType is not Score");
        }

        /// <summary>
        ///     If we're coming from a `replay`, then the screen should be initialized with this method
        /// </summary>
        private void InitializeIfReplayType()
        {
            if (ResultsType != ResultScreenType.Replay)
                throw new InvalidOperationException("Cannot call this if ResultsType is not Replay");
        }

        /// <summary>
        ///     Submits a score locally & to the server if applicable.
        ///     Also handles replay saving.
        /// </summary>
        private void SubmitScore()
        {
            NotificationManager.Show(NotificationLevel.Info, "Submitting score");
        }
    }
}