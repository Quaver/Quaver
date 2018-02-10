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
using Quaver.Graphics.Particles;

namespace Quaver.GameState.Gameplay
{
    /// <summary>
    /// This class handles the interaction between note and input.
    /// </summary>
    class GameplayManager : IHelper
    {
        //Helper classes
        private AccuracyBoxUI AccuracyBoxUI { get; set; }
        private NoteManager NoteManager { get; set; }
        private Playfield Playfield { get; set; }
        private Timing Timing { get; set; }
        private ScoreManager ScoreManager { get; set; }
        private PlayfieldUI PlayfieldUI { get; set; }
        private ParticleManager ParticleManager { get; set; }

        //todo: initialize and implement these later
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
        ///     The replay helper for this instance.
        /// </summary>
        private ReplayHelper ReplayHelper { get; set; }

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

        private bool DrawPlayfieldFirst { get; set; }

        //todo: remove. TEST.
        private Sprite SvInfoTextBox { get; set; }
        private TextBoxSprite SVText { get; set; }
        private TextButton TestButton { get; set; }

        //Rendering
        private const int RENDER_SAMPLES = 8;
        private int CurrentRenderIndex { get; set; }
        private RenderTarget2D[] RenderedHitObjects { get; set; }
        private RenderTarget2D RenderedPlayfield { get; set; }
        private Color[] RenderedAlphas { get; set; }

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
            ParticleManager = new ParticleManager();
            ScoreManager = new ScoreManager();
            InputManager = new GameplayInputManager();
            ReplayHelper = new ReplayHelper();
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
            CurrentRenderIndex = 0;
            RenderedPlayfield = new RenderTarget2D(GameBase.GraphicsDevice, GameBase.GraphicsDevice.Viewport.Width, GameBase.GraphicsDevice.Viewport.Height);
            RenderedAlphas = new Color[RENDER_SAMPLES];
            RenderedHitObjects = new RenderTarget2D[RENDER_SAMPLES];
            for (var i = 0; i < RenderedHitObjects.Length; i++)
            {
                RenderedHitObjects[i] = new RenderTarget2D(GameBase.GraphicsDevice, GameBase.GraphicsDevice.Viewport.Width, GameBase.GraphicsDevice.Viewport.Height);
                RenderedAlphas[i] = i == RenderedHitObjects.Length - 1 ? Color.White : Color.White * (1f / (1 + i));
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
            IntroSkippable = (GameBase.SelectedBeatmap.Qua.HitObjects[0].StartTime - CurrentSongTime >= Timing.SONG_SKIP_OFFSET + 2000);

            // Update Helper Classes
            if (!Paused)
            {
                NoteManager.CurrentSongTime = CurrentSongTime;
                Playfield.Update(dt);
                NoteManager.Update(dt);
                AccuracyBoxUI.Update(dt);
                PlayfieldUI.Update(dt);
                ParticleManager.Update(dt);

                // Record session with Replay Helper
                ReplayHelper.AddReplayFrames(ReplayFrames, GameBase.SelectedBeatmap.Qua, ScoreManager.Combo, Timing.ActualSongTime);
            }

            PlayfieldUI.UpdateMultiplierBars(ScoreManager.MultiplierIndex);
            PlayfieldUI.UpdateHealthBar(ScoreManager.Health);

            // Check the input for this particular game state.
            InputManager.CheckInput(IntroSkippable);

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
                ScoreManager.PlayTimeTotal = CurrentSongTime * GameBase.AudioEngine.PlaybackRate;
                GameBase.GameStateManager.ChangeState(new ScoreScreenState(BeatmapMd5, ScoreManager, GameBase.SelectedBeatmap.Artist, GameBase.SelectedBeatmap.Title, GameBase.SelectedBeatmap.DifficultyName, ReplayFrames));
            }
        }

        public void Draw()
        {
            // Update Render Index
            CurrentRenderIndex ++;
            if (CurrentRenderIndex >= RenderedHitObjects.Length) CurrentRenderIndex = 0;

            // Render Current NoteManager Frame
            GameBase.GraphicsDevice.SetRenderTarget(RenderedHitObjects[CurrentRenderIndex]);
            GameBase.GraphicsDevice.Clear(Color.Transparent);
            GameBase.SpriteBatch.Begin();
            NoteManager.Draw();
            GameBase.SpriteBatch.End();

            // Render Entire Playfield with NoteManagers blurred
            int alphaIndex = 0;
            GameBase.GraphicsDevice.SetRenderTarget(RenderedPlayfield);
            GameBase.GraphicsDevice.Clear(Color.Transparent);
            GameBase.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            Playfield.DrawBgMask();
            if (DrawPlayfieldFirst) Playfield.Draw();
            for (int i = CurrentRenderIndex - 1; i >= 0; i--)
            {
                GameBase.SpriteBatch.Draw(RenderedHitObjects[i], Vector2.Zero, RenderedAlphas[alphaIndex]);
                alphaIndex++;
            }
            for (int i = RenderedHitObjects.Length - 1; i >= CurrentRenderIndex; i--)
            {
                GameBase.SpriteBatch.Draw(RenderedHitObjects[i], Vector2.Zero, RenderedAlphas[alphaIndex]);
                alphaIndex++;
            }
            if (!DrawPlayfieldFirst) Playfield.Draw();
            ParticleManager.Draw();
            GameBase.SpriteBatch.End();

            // Render everything in order
            GameBase.GraphicsDevice.SetRenderTarget(GameBase.MainRenderTarget);
            GameBase.GraphicsDevice.Clear(Color.Transparent);
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

            // Initialize Note Manager and Playfield
            float playfieldPadding = 0;
            float receptorPadding = 0;
            float laneSize = 0;
            float playfieldSize = 0;
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            //the hit position is determined by the receptor and object of the first lane
            //the math here is kinda ugly, i plan on cleaning this up later
            //todo: clean up this code a bit
            {
                case GameModes.Keys4:
                    // Calculate References
                    GameplayReferences.ReceptorXPosition = new float[4];
                    laneSize = (int)(GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale);
                    playfieldPadding = (int)(GameBase.LoadedSkin.BgMaskPadding4K * GameBase.WindowUIScale);
                    receptorPadding = (int)(GameBase.LoadedSkin.NotePadding4K * GameBase.WindowUIScale);
                    DrawPlayfieldFirst = !GameBase.LoadedSkin.ReceptorsOverHitObjects4K;

                    // Update Playfield
                    Playfield.ReceptorYPosition = Config.Configuration.DownScroll4k  //todo: use list for scaling
                        ? GameBase.WindowRectangle.Height - (GameBase.LoadedSkin.ReceptorPositionOffset4K * GameBase.WindowUIScale + (laneSize * GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width))
                        : GameBase.LoadedSkin.ReceptorPositionOffset4K * GameBase.WindowUIScale;
                    Playfield.ColumnLightingPosition = Config.Configuration.DownScroll4k
                        ? Playfield.ReceptorYPosition
                        : Playfield.ReceptorYPosition
                        + GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale
                        * (float)(((double)GameBase.LoadedSkin.NoteReceptorsUp4K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[0].Width)
                        - ((double)GameBase.LoadedSkin.NoteHitObjects4K[0][0].Height / GameBase.LoadedSkin.NoteHitObjects4K[0][0].Width));
                    Console.WriteLine(Playfield.ColumnLightingPosition);

                    // Update Note Manager
                    NoteManager.ScrollSpeed = GameBase.WindowUIScale * Configuration.ScrollSpeed4k / (20f * GameBase.AudioEngine.PlaybackRate);
                    NoteManager.DownScroll = Configuration.DownScroll4k;
                    NoteManager.LaneSize = GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale;
                    NoteManager.HitPositionOffset = Config.Configuration.DownScroll4k
                        ? Playfield.ReceptorYPosition + ((Configuration.UserHitPositionOffset4k + GameBase.LoadedSkin.HitPositionOffset4K) * GameBase.WindowUIScale)
                        : Playfield.ReceptorYPosition - ((Configuration.UserHitPositionOffset4k + GameBase.LoadedSkin.HitPositionOffset4K) * GameBase.WindowUIScale)
                        + GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale
                        * (float)(((double)GameBase.LoadedSkin.NoteReceptorsUp4K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[0].Width)
                        - ((double)GameBase.LoadedSkin.NoteHitObjects4K[0][0].Height / GameBase.LoadedSkin.NoteHitObjects4K[0][0].Width));

                    // Update Score Manager
                    ScoreManager.ScrollSpeed = Configuration.ScrollSpeed4k;
                    break;
                case GameModes.Keys7:
                    // Calculate References
                    GameplayReferences.ReceptorXPosition = new float[7];
                    laneSize = (int)(GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale);
                    playfieldPadding = (int)(GameBase.LoadedSkin.BgMaskPadding7K * GameBase.WindowUIScale);
                    receptorPadding = (int)(GameBase.LoadedSkin.NotePadding7K * GameBase.WindowUIScale);
                    DrawPlayfieldFirst = !GameBase.LoadedSkin.ReceptorsOverHitObjects7K;

                    // Update Playfield
                    Playfield.ReceptorYPosition = Config.Configuration.DownScroll7k  //todo: use list for scaling
                        ? GameBase.WindowRectangle.Height - (GameBase.LoadedSkin.ReceptorPositionOffset7K * GameBase.WindowUIScale + (laneSize * GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width))
                        : GameBase.LoadedSkin.ReceptorPositionOffset7K * GameBase.WindowUIScale;
                    Playfield.ColumnLightingPosition = Config.Configuration.DownScroll7k
                        ? Playfield.ReceptorYPosition
                        : Playfield.ReceptorYPosition 
                        + GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale
                        * ((GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width)
                        - (GameBase.LoadedSkin.NoteHitObjects7K[0].Height / GameBase.LoadedSkin.NoteHitObjects7K[0].Width));

                    // Update Note Manager
                    NoteManager.ScrollSpeed = GameBase.WindowUIScale * Configuration.ScrollSpeed7k / (20f * GameBase.AudioEngine.PlaybackRate);
                    NoteManager.DownScroll = Configuration.DownScroll7k;
                    NoteManager.LaneSize = GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale;
                    NoteManager.HitPositionOffset = Config.Configuration.DownScroll7k
                        ? Playfield.ReceptorYPosition + ((Configuration.UserHitPositionOffset7k + GameBase.LoadedSkin.HitPositionOffset7K) * GameBase.WindowUIScale)
                        : Playfield.ReceptorYPosition - ((Configuration.UserHitPositionOffset7k + GameBase.LoadedSkin.HitPositionOffset7K) * GameBase.WindowUIScale)
                        + GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale
                        * (float)(((double)GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width)
                        - ((double)GameBase.LoadedSkin.NoteHitObjects7K[0].Height / GameBase.LoadedSkin.NoteHitObjects7K[0].Width));

                    // Update Score Manager
                    ScoreManager.ScrollSpeed = Configuration.ScrollSpeed7k;
                    break;
            }

            // Calculate Config + Skin stuff
            playfieldSize = ((laneSize + receptorPadding) * GameplayReferences.ReceptorXPosition.Length) + (playfieldPadding * 2) - receptorPadding;
            NoteManager.PlayfieldSize = playfieldSize;
            PlayfieldUI.PlayfieldSize = playfieldSize;
            Playfield.PlayfieldSize = playfieldSize;
            Playfield.LaneSize = laneSize;
            Playfield.PlayfieldPadding = playfieldPadding;
            Playfield.ReceptorPadding = receptorPadding;
            //MeasureBarManager.PlayfieldSize = playfieldSize;

            // Get SV data
            NoteManager.SvQueue = Timing.GetSVQueue(qua);
            NoteManager.SvCalc = Timing.GetSVCalc(NoteManager.SvQueue);

            // Initialize class components
            Playfield.Initialize(state);
            Timing.Initialize(state);
            AccuracyBoxUI.Initialize(state);
            PlayfieldUI.Initialize(state);
            NoteManager.Initialize(state);
            ParticleManager.Initialize(state);

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
                // Play the correct hitsound on key press
                PlayHitsound(noteIndex);

                // Check which HitWindow this object's timing is in
                for (i = 0; i < 5; i++)
                {
                    if (Math.Abs(NoteManager.HitObjectPool[noteIndex].StartTime - CurrentSongTime) <= ScoreManager.HitWindowPress[i])
                    {
                        // Update ScoreManager and UI if note was pressed on time
                        ScoreManager.Count(i, false, NoteManager.HitObjectPool[noteIndex].StartTime - CurrentSongTime, CurrentSongTime * GameBase.AudioEngine.PlaybackRate);
                        AccuracyBoxUI.UpdateAccuracyBox(i, ScoreManager.JudgePressSpread[i], ScoreManager.JudgeReleaseSpread[i], ScoreManager.JudgeCount, ScoreManager.ScoreTotal, ScoreManager.Accuracy);
                        AccuracyBoxUI.UpdateGradeBar(ScoreManager.GetAccGradeIndex(), ScoreManager.GetRelativeAccScale());
                        PlayfieldUI.UpdateJudge(i, ScoreManager.Combo, false, NoteManager.HitObjectPool[noteIndex].StartTime - CurrentSongTime);

                        // If the player is spamming
                        if (i >= 3)
                        {
                            //If the object is an LN, don't forget to count it
                            if (NoteManager.HitObjectPool[noteIndex].IsLongNote)
                                ReleaseSkipped(null, null);

                            NoteManager.KillNote(noteIndex);
                            //NoteManager.RecycleNote(noteIndex);
                        }
                        else
                        {
                            // Create a Hit Burst instance
                            ParticleManager.CreateHitBurst(NoteManager.NoteBurstRectangle[keyLane.GetKey()], keyLane.GetKey());

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
                GameBase.LoadedSkin.SoundComboBreak.Play(GameBase.SoundEffectVolume, 0, 0);

            // Manage UI Helpers + Update Score Manager
            ScoreManager.Count(5, false, 0, CurrentSongTime * GameBase.AudioEngine.PlaybackRate);
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
                var skipTime = GameBase.SelectedBeatmap.Qua.HitObjects[0].StartTime - Timing.SONG_SKIP_OFFSET + AudioEngine.BassDelayOffset;

                // Skip to 3 seconds before the notes start
                try
                {
                    // Add the skip frame here.
                    ReplayHelper.AddReplayFrames(ReplayFrames, GameBase.SelectedBeatmap.Qua, ScoreManager.Combo, Timing.ActualSongTime, true);

                    // Skip to the time if the audio already played once. If it hasn't, then play it.
                    if (GameBase.AudioEngine.HasPlayed)
                        GameBase.AudioEngine.ChangeSongPosition(skipTime);
                    else
                        GameBase.AudioEngine.Play(skipTime);

                    // Set the actual song time to the position in the audio if it was successful.
                    Timing.ActualSongTime = GameBase.AudioEngine.Position;
                }
                catch (AudioEngineException ex)
                {
                    Logger.LogWarning("Trying to skip with no audio file loaded. Still continuing..", LogType.Runtime);

                    // If there is no audio file, make sure the actual song time is set to the skip time.
                    Timing.ActualSongTime = skipTime;
                }
                finally
                {
                    Timing.SongIsPlaying = true;
                    DiscordController.ChangeDiscordPresenceGameplay(true);
                }
            }
        }

        /// <summary>
        ///     Toggles the pausing of the song and sets Discord Rich Presence
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PauseSong(object sender, EventArgs e)
        {
            // If the game is paused, it will unpause.
            if (Paused)
            {
                Paused = false;
                GameBase.AudioEngine.Resume();

                // Set Discord Rich Presence back to the correct state
                DiscordController.ChangeDiscordPresenceGameplay(true);
            }
            // If the game is not paused, it will pause.
            else
            {
                Paused = true;
                GameBase.AudioEngine.Pause();

                // Set Discord Rich Presence to a paused state
                var rpc = $"{GameBase.SelectedBeatmap.Qua.Artist} - {GameBase.SelectedBeatmap.Qua.Title} [{GameBase.SelectedBeatmap.Qua.DifficultyName}]";
                DiscordController.ChangeDiscordPresence(rpc, "Paused");
            }
        }

        /// <summary>
        ///     Plays the correct hitsounds based on the note index of the HitObject pool
        /// </summary>
        private void PlayHitsound(int noteIndex)
        {
            var hitObject = NoteManager.HitObjectPool[noteIndex];

            // Normal
            if (hitObject.HitSounds == 0 || (HitSounds.Normal & hitObject.HitSounds) != 0)
                GameBase.LoadedSkin.SoundHit.Play(0.4f, 0, 0);

            // Clap
            if ((HitSounds.Clap & hitObject.HitSounds) != 0)
                GameBase.LoadedSkin.SoundHitClap.Play(GameBase.SoundEffectVolume, 0, 0);

            // Whistle
            if ((HitSounds.Whistle & hitObject.HitSounds) != 0)
                GameBase.LoadedSkin.SoundHitWhistle.Play(GameBase.SoundEffectVolume, 0, 0);

            // Finish
            if ((HitSounds.Finish & hitObject.HitSounds) != 0)
                GameBase.LoadedSkin.SoundHitFinish.Play(GameBase.SoundEffectVolume, 0, 0);
        }
    }
}
