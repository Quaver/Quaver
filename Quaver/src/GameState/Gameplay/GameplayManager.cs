using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.GameState.States;
using Quaver.Logging;
using Quaver.Audio;
using Quaver.GameState.Gameplay.PlayScreen;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
using Quaver.Graphics.Button;
using Quaver.Graphics;
using Quaver.Input;
using Quaver.Replays;
using Quaver.Config;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Discord;
using Quaver.API.Enums;

namespace Quaver.GameState.Gameplay
{
    /// <summary>
    /// This class handles the interaction between note and input.
    /// </summary>
    class GameplayManager : IHelper
    {
        private AccuracyBoxUI AccuracyBoxUI { get; set; }

        private NoteManager NoteManager { get; set; }

        private Playfield Playfield { get; set; }

        private Timing Timing { get; set; }

        private ScoreManager ScoreManager { get; set; }

        private PlayfieldUI PlayfieldUI { get; set; }

        //todo: initialize and implement these later
        private HitBurst HitBurst { get; set; }
        private Particles Particles { get; set; }
        private ScoreProgressUI ScoreProgressUI { get; set; }
        private SongProgressUI SongProgressUI { get; set; }

        /// <summary>
        ///     The MD5 Hash of the played beatmap.
        /// </summary>
        private string BeatmapMd5 { get; set; }

        /// <summary>
        ///     The current Qua file that's being red
        /// </summary>
        private Qua CurrentQua { get; set; }

        /// <summary>
        ///     The input manager for this game state.
        /// </summary>
        private GameplayInputManager InputManager { get; set; }

        /// <summary>
        ///     Holds the list of replay frames for this state.
        /// </summary>xzx
        private List<ReplayFrame> ReplayFrames { get; set; }

        /// <summary>
        ///     Keeps track of whether or not the song intro is current skippable.
        /// </summary>
        private bool IntroSkippable { get; set; }

        /// <summary>
        ///     Keeps track of whether or not the song intro was skipped.
        /// </summary>
        private bool IntroSkipped { get; set; }

        /// <summary>
        ///     The Current Song Time
        /// </summary>
        private double CurrentSongTime { get; set; }

        /// <summary>
        ///     Is determined by whether the game is paused or not.
        /// </summary>
        internal bool Paused { get; private set; }

        //todo: remove. TEST.
        private Sprite SvInfoTextBox { get; set; }
        private TextBoxSprite SVText { get; set; }
        private TextButton TestButton { get; set; }

        //Rendering
        private RenderTarget2D[] RenderedHitObjects { get; set; } = new RenderTarget2D[8];
        private RenderTarget2D RenderedPlayfield { get; set; }
        private Color[] RenderedAlphas { get; set; } = new Color[8];

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="qua"></param>
        public GameplayManager(Qua qua, string beatmapMd5)
        {
            // Pass Parameters
            BeatmapMd5 = beatmapMd5;
            CurrentQua = qua;

            // Create Class Components
            AccuracyBoxUI = new AccuracyBoxUI();
            NoteManager = new NoteManager(qua);
            Playfield = new Playfield();
            PlayfieldUI = new PlayfieldUI();
            Timing = new Timing(qua);
            ScoreManager = new ScoreManager();
            InputManager = new GameplayInputManager();
            ReplayFrames = new List<ReplayFrame>();

            // Initialize Gameplay
            InitializeGameplay(null, qua);

            // Hook InputManager
            InputManager.ManiaKeyPress += ManiaKeyDown;
            InputManager.ManiaKeyRelease += ManiaKeyUp;
            InputManager.SkipSong += SkipSong;
            InputManager.PauseSong += PauseSong;

            // Hook Missed Note Events
            NoteManager.PressMissed += PressMissed;
            NoteManager.ReleaseSkipped += ReleaseSkipped;
            NoteManager.ReleaseMissed += ReleaseMissed;

            // Initialize Rendering
            RenderedPlayfield = new RenderTarget2D(GameBase.GraphicsDevice, GameBase.GraphicsDevice.Viewport.Width, GameBase.GraphicsDevice.Viewport.Height);
            for (var i = 0; i < RenderedHitObjects.Length; i++)
            {
                RenderedHitObjects[i] = new RenderTarget2D(GameBase.GraphicsDevice, GameBase.GraphicsDevice.Viewport.Width, GameBase.GraphicsDevice.Viewport.Height);
                RenderedAlphas[i] = Color.White * (1f / (1 + i));
            }
        }

        public void Initialize(IGameState playScreen)
        {
            //Todo: Remove. TEST.
            TestButton = new TextButton(new Vector2(200, 30), "BACK")
            {
                Alignment = Alignment.MidLeft
            };
            TestButton.Clicked += BackButtonClick;

            SvInfoTextBox = new Sprite()
            {
                Image = GameBase.UI.HollowBox,
                Tint = Color.Blue,
                Size = new UDim2(250, 500),
                Alignment = Alignment.TopRight
            };
        }

        public void UnloadContent()
        {
            // Unhook InputManager
            InputManager.ManiaKeyPress -= ManiaKeyDown;
            InputManager.ManiaKeyRelease -= ManiaKeyUp;
            InputManager.SkipSong -= SkipSong;
            InputManager.PauseSong -= PauseSong;

            // Unook Missed Note Events
            NoteManager.PressMissed -= PressMissed;
            NoteManager.ReleaseSkipped -= ReleaseSkipped;
            NoteManager.ReleaseMissed -= ReleaseMissed;

            //NoteManager.UnloadContent();
            Timing.UnloadContent();
            Playfield.UnloadContent();
            PlayfieldUI.UnloadContent();
            AccuracyBoxUI.UnloadContent();
            NoteManager.UnloadContent();

            //todo: remove this later
            TestButton.Clicked -= BackButtonClick;

            SvInfoTextBox.Destroy();
        }

        public void Update(double dt)
        {
            // Update Drawables
            TestButton.Update(dt);
            SvInfoTextBox.Update(dt);
            
            // Set the current song time.
            Timing.Update(dt);
            CurrentSongTime = Timing.GetCurrentSongTime();

            // Check if the song is currently skippable.
            IntroSkippable = (GameBase.SelectedBeatmap.Qua.HitObjects[0].StartTime - CurrentSongTime >= 5000);

            // Update Helper Classes
            if (!Paused)
            {
                NoteManager.CurrentSongTime = CurrentSongTime;
                Playfield.Update(dt);
                NoteManager.Update(dt);
                AccuracyBoxUI.Update(dt);
                PlayfieldUI.Update(dt);
            }

            PlayfieldUI.UpdateMultiplierBars(ScoreManager.MultiplierIndex);
            PlayfieldUI.UpdateHealthBar(ScoreManager.Health);

            // Check the input for this particular game state.
            InputManager.CheckInput(IntroSkippable, ReplayFrames);

            // Record session with Replay Helper
            ReplayHelper.AddReplayFrames(ReplayFrames, GameBase.SelectedBeatmap.Qua);

            // Update Loggers. todo: remove
            Logger.Update("KeyCount", $"Game Mode: {GameBase.SelectedBeatmap.Qua.Mode}");
            Logger.Update("SongPos", "Current Track Position: " + NoteManager.TrackPosition);
            Logger.Update("Skippable", $"Intro Skippable: {IntroSkippable}");
            Logger.Update("Paused", "Paused: " + Paused.ToString());

            //Todo: remove. below
            SvInfoTextBox.Update(dt);

            // If the song is done, it'll change state. todo: add a method for this later
            if (Timing.PlayingIsDone || ScoreManager.Failed)
            {
                //Logger.Log("DONE", LogColors.GameImportant);
                ScoreManager.PlayTimeTotal = CurrentSongTime * GameBase.GameClock;
                GameBase.GameStateManager.ChangeState(new ScoreScreenState(BeatmapMd5, ScoreManager, GameBase.SelectedBeatmap.Artist, GameBase.SelectedBeatmap.Title, GameBase.SelectedBeatmap.DifficultyName, ReplayFrames));
            }
        }

        public void Draw()
        {
            // Move Rendered Frames from front to end of array by 1 step
            for (int i = RenderedHitObjects.Length - 1; i > 0; i--)
                RenderedHitObjects[i] = RenderedHitObjects[i - 1];

            // Render Current NoteManager Frame
            GameBase.GraphicsDevice.SetRenderTarget(RenderedHitObjects[0]);
            GameBase.GraphicsDevice.Clear(Color.White * 0);
            GameBase.SpriteBatch.Begin();
            NoteManager.Draw();
            GameBase.SpriteBatch.End();

            // Render Entire Playfield with NoteManagers blurred
            GameBase.GraphicsDevice.SetRenderTarget(RenderedPlayfield);
            GameBase.GraphicsDevice.Clear(Color.White * 0);
            GameBase.SpriteBatch.Begin();
            Playfield.Draw();
            for (int i = RenderedHitObjects.Length - 1; i >= 0; i--)
                GameBase.SpriteBatch.Draw(RenderedHitObjects[i], Vector2.Zero, RenderedAlphas[i]);
            GameBase.SpriteBatch.End();

            // Render everything in order
            GameBase.GraphicsDevice.SetRenderTarget(GameBase.MainRenderTarget);
            GameBase.GraphicsDevice.Clear(Color.White * 0);
            GameBase.SpriteBatch.Begin();
            BackgroundManager.Draw();
            GameBase.SpriteBatch.Draw(RenderedPlayfield, Vector2.Zero, Color.White);
            PlayfieldUI.Draw();
            AccuracyBoxUI.Draw();
            TestButton.Draw();
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Solely responsible for intializing gameplay aspects
        /// </summary>
        private void InitializeGameplay(IGameState state, Qua qua)
        {
            // Get Song Time
            CurrentSongTime = 0;

            // Initialize Score Manager
            // Get total judge count (press + release)
            var count = 0;
            var total = GameBase.SelectedBeatmap.Qua.HitObjects.Count;

            foreach (var ho in GameBase.SelectedBeatmap.Qua.HitObjects)
            {
                if (ho.EndTime > ho.StartTime) count++;
            }

            ScoreManager = new ScoreManager();
            ScoreManager.Initialize(total + count); //TODO: ADD RELEASE COUNTS AS WELL

            // Declare Gameplay References
            NoteManager.PressWindowLatest = ScoreManager.HitWindowPress[4];
            NoteManager.ReleaseWindowLatest = ScoreManager.HitWindowRelease[3];

            // Initialize class components
            Playfield.Initialize(state);
            Timing.Initialize(state);
            AccuracyBoxUI.Initialize(state);
            PlayfieldUI.Initialize(state);

            // Initialize Note Manager
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            //the hit position is determined by the receptor and object of the first lane
            //the math here is kinda ugly, i plan on cleaning this up later
            {
                case GameModes.Keys4:
                    ScoreManager.ScrollSpeed = Configuration.ScrollSpeed4k;
                    NoteManager.ScrollSpeed = GameBase.WindowUIScale * Configuration.ScrollSpeed4k / (20f * GameBase.GameClock);
                    NoteManager.DownScroll = Configuration.DownScroll4k;
                    NoteManager.LaneSize = GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale;
                    NoteManager.HitPositionOffset = Config.Configuration.DownScroll4k
                        ? GameplayReferences.ReceptorYOffset
                        : GameplayReferences.ReceptorYOffset
                        + GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale
                        * ((GameBase.LoadedSkin.NoteReceptorsUp4K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[0].Width)
                        - (GameBase.LoadedSkin.NoteHitObjects4K[0][0].Height / GameBase.LoadedSkin.NoteHitObjects4K[0][0].Width));
                    break;
                case GameModes.Keys7:
                    ScoreManager.ScrollSpeed = Configuration.ScrollSpeed7k;
                    NoteManager.ScrollSpeed = GameBase.WindowUIScale * Configuration.ScrollSpeed7k / (20f * GameBase.GameClock);
                    NoteManager.DownScroll = Configuration.DownScroll7k;
                    NoteManager.LaneSize = GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale;
                    NoteManager.HitPositionOffset = Config.Configuration.DownScroll7k
                        ? GameplayReferences.ReceptorYOffset
                        : GameplayReferences.ReceptorYOffset
                        + GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale
                        * ((GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width)
                        - (GameBase.LoadedSkin.NoteHitObjects7K[0].Height / GameBase.LoadedSkin.NoteHitObjects7K[0].Width));
                    break;
            }
            NoteManager.SvQueue = Timing.GetSVQueue(qua);
            NoteManager.SvCalc = Timing.GetSVCalc(NoteManager.SvQueue);
            NoteManager.Initialize(state);

            //todo: remove this. used for logging.
            Logger.Add("KeyCount", "", Color.Pink);
            Logger.Add("SongPos", "", Color.White);
            Logger.Add("Skippable", "", GameColors.NameTagAdmin);
            Logger.Add("Paused", "", GameColors.NameTagModerator);
        }

        /// <summary>
        ///     Temporary method for back button click handling todo: remove
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BackButtonClick(object sender, EventArgs e)
        {
            GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        /// <summary>
        ///     Everytime a mania key gets pressed, this method will look for the closest note and judge it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keyLane"></param>
        public void ManiaKeyDown(object sender, ManiaKeyEventArgs keyLane)
        {
            // It will not read input if the game is paused
            if (Paused)
                return;

            // Play Audio
            GameBase.LoadedSkin.SoundHit.Play((float)Configuration.VolumeGlobal / 100 * Configuration.VolumeEffect / 100, 0, 0);

            //Check for Note press/LN press
            //Reference Variables
            int noteIndex = -1;
            int i;

            // Update Receptor in Playfield
            Playfield.UpdateReceptor(keyLane.GetKey(), true);

            // Search for closest HitObject that is inside the HitTiming Window
            for (i = 0; i < NoteManager.HitObjectPoolSize && i < NoteManager.HitObjectPool.Count; i++)
            {
                if (NoteManager.HitObjectPool[i].KeyLane == keyLane.GetKey() + 1 && NoteManager.HitObjectPool[i].StartTime - CurrentSongTime > -ScoreManager.HitWindowPress[4])
                {
                    noteIndex = i;
                    break;
                }
            }

            // If such HitObject exists, it will do key-press stuff to it
            if (noteIndex > -1)
            {
                // Check which HitWindow this object's timing is in
                for (i = 0; i < 5; i++)
                {
                    if (Math.Abs(NoteManager.HitObjectPool[noteIndex].StartTime - CurrentSongTime) <= ScoreManager.HitWindowPress[i])
                    {
                        // Update ScoreManager and UI if note was pressed on time
                        ScoreManager.Count(i, false, NoteManager.HitObjectPool[noteIndex].StartTime - CurrentSongTime, CurrentSongTime * GameBase.GameClock);
                        AccuracyBoxUI.UpdateAccuracyBox(i, ScoreManager.JudgePressSpread[i], ScoreManager.JudgeReleaseSpread[i], ScoreManager.JudgeCount, ScoreManager.ScoreTotal, ScoreManager.Accuracy);
                        AccuracyBoxUI.UpdateGradeBar(ScoreManager.GetAccGradeIndex(), ScoreManager.GetRelativeAccScale());
                        PlayfieldUI.UpdateJudge(i, ScoreManager.Combo, false, NoteManager.HitObjectPool[noteIndex].StartTime - CurrentSongTime);

                        // If the player is spamming
                        if (i >= 3)
                        {
                            //todo: (Swan: This is why the game looks so weird)
                            //If the object is an LN, don't forget to count it
                            if (NoteManager.HitObjectPool[noteIndex].IsLongNote)
                                ReleaseSkipped(null, null);

                            NoteManager.KillNote(noteIndex);
                            //NoteManager.RecycleNote(noteIndex);
                        }
                        else
                        {
                            // If the object is an LN, hold it at the receptors
                            if (NoteManager.HitObjectPool[noteIndex].IsLongNote) NoteManager.HoldNote(noteIndex);

                            // If the object is not an LN, recycle it.
                            else NoteManager.RecycleNote(noteIndex);
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Everytime a mania key gets released, this method will look for the closest note and judge it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keyLane"></param>
        public void ManiaKeyUp(object sender, ManiaKeyEventArgs keyLane)
        {
            // It will not read input if the game is paused
            if (Paused)
                return;

            //Reference Variables
            int noteIndex = -1;
            int i;

            // Update Receptor in Playfield
            Playfield.UpdateReceptor(keyLane.GetKey(), false);

            // Search for closest HitObject that is inside the HitTiming Window
            for (i = 0; i < NoteManager.HitObjectHold.Count; i++)
            {
                if (NoteManager.HitObjectHold[i].KeyLane == keyLane.GetKey() + 1)
                {
                    noteIndex = i;
                    break;
                }
            }

            // If such HitObject exists, it will do key-press stuff to it
            if (noteIndex > -1)
            {
                //Check which HitWindow this object's timing is in.
                //Since it's an LN, the hit window is increased by 1.25x.
                //Only checks MARV/PERF/GREAT/GOOD
                int rIndex = -1;
                for (i = 0; i < 4; i++)
                {
                    if (Math.Abs(NoteManager.HitObjectHold[noteIndex].EndTime - CurrentSongTime) < ScoreManager.HitWindowRelease[i])
                    {
                        rIndex = i;
                        break;
                    }
                }

                // If LN has been released during a HitWindow
                if (rIndex > -1)
                {
                    // Update ScoreManager and UI if note was pressed on time
                    ScoreManager.Count(rIndex, true);
                    AccuracyBoxUI.UpdateAccuracyBox(rIndex, ScoreManager.JudgePressSpread[rIndex], ScoreManager.JudgeReleaseSpread[rIndex], ScoreManager.JudgeCount, ScoreManager.ScoreTotal, ScoreManager.Accuracy);
                    AccuracyBoxUI.UpdateGradeBar(ScoreManager.GetAccGradeIndex(), ScoreManager.GetRelativeAccScale());
                    PlayfieldUI.UpdateJudge(rIndex, ScoreManager.Combo, true);
                    NoteManager.KillHold(noteIndex, true);
                }
                // If LN has been released early
                else
                {
                    // Update ScoreManager and UI if note was pressed on time
                    ScoreManager.Count(5, true);
                    AccuracyBoxUI.UpdateAccuracyBox(5, ScoreManager.JudgePressSpread[i], ScoreManager.JudgeReleaseSpread[i], ScoreManager.JudgeCount, ScoreManager.ScoreTotal, ScoreManager.Accuracy);
                    AccuracyBoxUI.UpdateGradeBar(ScoreManager.GetAccGradeIndex(), ScoreManager.GetRelativeAccScale());
                    PlayfieldUI.UpdateJudge(5, ScoreManager.Combo, true);
                    NoteManager.KillHold(noteIndex);
                }
            }
        }

        public void PressMissed(object sender, EventArgs e)
        {
            // Play Combo-Break Sound
            if (ScoreManager.Combo >= 20)
                GameBase.LoadedSkin.SoundComboBreak.Play((float)Configuration.VolumeGlobal / 100 * Configuration.VolumeEffect / 100, 0, 0);

            // Manage UI Helpers + Update Score Manager
            ScoreManager.Count(5, false, 0, CurrentSongTime * GameBase.GameClock);
            AccuracyBoxUI.UpdateAccuracyBox(5, ScoreManager.JudgePressSpread[5], ScoreManager.JudgeReleaseSpread[5], ScoreManager.JudgeCount, ScoreManager.ScoreTotal, ScoreManager.Accuracy);
            AccuracyBoxUI.UpdateGradeBar(ScoreManager.GetAccGradeIndex(), ScoreManager.GetRelativeAccScale());
            PlayfieldUI.UpdateJudge(5, ScoreManager.Combo);
        }

        public void ReleaseSkipped(object sender, EventArgs e)
        {
            ScoreManager.Count(5, true);
            AccuracyBoxUI.UpdateAccuracyBox(5, ScoreManager.JudgePressSpread[5], ScoreManager.JudgeReleaseSpread[5], ScoreManager.JudgeCount, ScoreManager.ScoreTotal, ScoreManager.Accuracy);
            AccuracyBoxUI.UpdateGradeBar(ScoreManager.GetAccGradeIndex(), ScoreManager.GetRelativeAccScale());
            PlayfieldUI.UpdateJudge(5, ScoreManager.Combo);
        }

        public void ReleaseMissed(object sender, EventArgs e)
        {
            ScoreManager.Count(4, true);
            AccuracyBoxUI.UpdateAccuracyBox(4, ScoreManager.JudgePressSpread[4], ScoreManager.JudgeReleaseSpread[4], ScoreManager.JudgeCount, ScoreManager.ScoreTotal, ScoreManager.Accuracy);
            AccuracyBoxUI.UpdateGradeBar(ScoreManager.GetAccGradeIndex(), ScoreManager.GetRelativeAccScale());
            PlayfieldUI.UpdateJudge(4, ScoreManager.Combo);
        }

        public void SkipSong(object sender, EventArgs e)
        {
            if (IntroSkippable && GameBase.KeyboardState.IsKeyDown(Configuration.KeySkipIntro) && !IntroSkipped)
            {
                IntroSkipped = true;

                Logger.Log("Song has been successfully skipped to 3 seconds before the first HitObject.", LogColors.GameSuccess);

                // Skip to 3 seconds before the notes start
                SongManager.Load();
                SongManager.SkipTo(GameBase.SelectedBeatmap.Qua.HitObjects[0].StartTime - 3000 + SongManager.BassDelayOffset);
                SongManager.Play();

                Timing.SongIsPlaying = true;
                DiscordController.ChangeDiscordPresenceGameplay(true);
            }
        }

        public void PauseSong(object sender, EventArgs e)
        {
            // If the game is paused, it will unpause.
            if (Paused)
            {
                Paused = false;
                SongManager.Resume();
            }
            // If the game is not paused, it will pause.
            else
            {
                Paused = true;
                SongManager.Pause();
            }
        }
    }
}
