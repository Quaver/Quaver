using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Graphics.UserInterface;
using Quaver.Input;
using Quaver.Logging;
using Quaver.Replays;
using Quaver.States.Gameplay.Mania.Components;
using Quaver.States.Gameplay.Mania.Components.Scoring;
using Quaver.States.Gameplay.Mania.Components.Timing;
using Quaver.States.Gameplay.Mania.UI.Accuracy;
using Quaver.States.Gameplay.Mania.UI.HitObjects;
using Quaver.States.Gameplay.Mania.UI.Particles;
using Quaver.States.Gameplay.Mania.UI.Playfield;
using Quaver.States.Gameplay.Mania.UI.Progress;
using Quaver.States.Results;
using Quaver.States.Select;

namespace Quaver.States.Gameplay.Mania
{
    /// <summary>
    /// This class handles the interaction between note and input.
    /// </summary>
    internal class ManiaGameplayManager : IGameStateComponent
    {
        //Helper classes
        private ManiaAccuracyBox ManiaAccuracyBox { get; set; }
        private ManiaNoteManager ManiaNoteManager { get; set; }
        private ManiaPlayfield ManiaPlayfield { get; set; }
        private ManiaTiming ManiaTiming { get; set; }
        private ManiaScoreManager ManiaScoreManager { get; set; }
        private ManiaPlayfieldUI ManiaPlayfieldUi { get; set; }
        private ManiaParticleManager ManiaParticleManager { get; set; }

        //todo: initialize and implement these later
        private ManiaScoreProgress ManiaScoreProgress { get; set; }
        private ManiaSongProgress ManiaSongProgress { get; set; }

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
        private QuaverSprite SvInfoTextBox { get; set; }
        private QuaverTextbox SvQuaverText { get; set; }
        private QuaverTextButton TestButton { get; set; }

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
        public ManiaGameplayManager(Qua qua, string beatmapMd5)
        {
            // Pass Parameters
            BeatmapMd5 = beatmapMd5;
            CurrentQua = qua;

            // Create Class Components
            ManiaAccuracyBox = new ManiaAccuracyBox();
            ManiaNoteManager = new ManiaNoteManager(qua);
            ManiaPlayfield = new ManiaPlayfield();
            ManiaPlayfieldUi = new ManiaPlayfieldUI();
            ManiaTiming = new ManiaTiming(qua);
            ManiaParticleManager = new ManiaParticleManager();
            ManiaScoreManager = new ManiaScoreManager();
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
            ManiaNoteManager.PressMissed += PressMissed;
            ManiaNoteManager.ReleaseSkipped += ReleaseSkipped;
            ManiaNoteManager.ReleaseMissed += ReleaseMissed;

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
            TestButton = new QuaverTextButton(new Vector2(200, 30), "BACK")
            {
                Alignment = Alignment.MidLeft
            };
            TestButton.Clicked += BackButtonClick;

            SvInfoTextBox = new QuaverSprite()
            {
                Image = GameBase.QuaverUserInterface.HollowBox,
                Tint = Color.Blue,
                Size = new UDim2D(250, 500),
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
            ManiaNoteManager.PressMissed -= PressMissed;
            ManiaNoteManager.ReleaseSkipped -= ReleaseSkipped;
            ManiaNoteManager.ReleaseMissed -= ReleaseMissed;

            //ManiaNoteManager.UnloadContent();
            ManiaTiming.UnloadContent();
            ManiaPlayfield.UnloadContent();
            ManiaPlayfieldUi.UnloadContent();
            ManiaAccuracyBox.UnloadContent();
            ManiaNoteManager.UnloadContent();

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
            ManiaTiming.Update(dt);
            CurrentSongTime = ManiaTiming.GetCurrentSongTime();

            // Check if the song is currently skippable.
            IntroSkippable = (GameBase.SelectedBeatmap.Qua.HitObjects[0].StartTime - CurrentSongTime >= ManiaTiming.SONG_SKIP_OFFSET + 2000);

            // Update Helper Classes
            if (!Paused)
            {
                ManiaNoteManager.CurrentSongTime = CurrentSongTime;
                ManiaPlayfield.Update(dt);
                ManiaNoteManager.Update(dt);
                ManiaAccuracyBox.Update(dt);
                ManiaPlayfieldUi.Update(dt);
                ManiaParticleManager.Update(dt);

                // Record session with Replay Helper
                ReplayHelper.AddReplayFrames(ReplayFrames, GameBase.SelectedBeatmap.Qua, ManiaScoreManager.Combo, CurrentSongTime);
            }

            ManiaPlayfieldUi.UpdateMultiplierBars(ManiaScoreManager.MultiplierIndex);
            ManiaPlayfieldUi.UpdateHealthBar(ManiaScoreManager.Health);

            // Check the input for this particular game state.
            InputManager.CheckInput(IntroSkippable);

            // Update Loggers. todo: remove
            Logger.Update("KeyCount", $"Game Mode: {GameBase.SelectedBeatmap.Qua.Mode}");
            Logger.Update("SongPos", "Current Track Position: " + ManiaNoteManager.TrackPosition);
            Logger.Update("Skippable", $"Intro Skippable: {IntroSkippable}");
            Logger.Update("Paused", "Paused: " + Paused.ToString());

            //Todo: remove. below
            SvInfoTextBox.Update(dt);

            // If the song is done, it'll change state. todo: add a method for this later
            if (ManiaTiming.PlayingIsDone || ManiaScoreManager.Failed)
            {
                //Logger.Log("DONE", LogColors.GameImportant);
                ManiaScoreManager.PlayTimeTotal = CurrentSongTime * GameBase.AudioEngine.PlaybackRate;
                GameBase.GameStateManager.ChangeState(new ResultsState(BeatmapMd5, ManiaScoreManager, GameBase.SelectedBeatmap.Artist, GameBase.SelectedBeatmap.Title, GameBase.SelectedBeatmap.DifficultyName, ReplayFrames));
            }
        }

        public void Draw()
        {
            // Update Render Index
            CurrentRenderIndex ++;
            if (CurrentRenderIndex >= RenderedHitObjects.Length) CurrentRenderIndex = 0;

            // Render Current ManiaNoteManager Frame
            GameBase.GraphicsDevice.SetRenderTarget(RenderedHitObjects[CurrentRenderIndex]);
            GameBase.GraphicsDevice.Clear(Color.Transparent);
            GameBase.SpriteBatch.Begin();
            ManiaNoteManager.Draw();
            GameBase.SpriteBatch.End();

            // Render Entire ManiaPlayfield with NoteManagers blurred
            int alphaIndex = 0;
            GameBase.GraphicsDevice.SetRenderTarget(RenderedPlayfield);
            GameBase.GraphicsDevice.Clear(Color.Transparent);
            GameBase.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            ManiaPlayfield.DrawBgMask();
            if (DrawPlayfieldFirst) ManiaPlayfield.Draw();
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
            if (!DrawPlayfieldFirst) ManiaPlayfield.Draw();
            ManiaParticleManager.Draw();
            GameBase.SpriteBatch.End();

            // Render everything in order
            GameBase.GraphicsDevice.SetRenderTarget(GameBase.MainRenderTarget);
            GameBase.GraphicsDevice.Clear(Color.Transparent);
            GameBase.SpriteBatch.Begin();
            BackgroundManager.Draw();
            GameBase.SpriteBatch.Draw(RenderedPlayfield, Vector2.Zero, Color.White);
            ManiaPlayfieldUi.Draw();
            ManiaAccuracyBox.Draw();
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

            ManiaScoreManager = new ManiaScoreManager();
            ManiaScoreManager.Initialize(total + count); //TODO: ADD RELEASE COUNTS AS WELL

            // Declare Gameplay References
            ManiaNoteManager.PressWindowLatest = ManiaScoreManager.HitWindowPress[4];
            ManiaNoteManager.ReleaseWindowLatest = ManiaScoreManager.HitWindowRelease[3];

            // Initialize Note Manager and ManiaPlayfield
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
                    ManiaGameplayReferences.ReceptorXPosition = new float[4];
                    laneSize = (int)(GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale);
                    playfieldPadding = (int)(GameBase.LoadedSkin.BgMaskPadding4K * GameBase.WindowUIScale);
                    receptorPadding = (int)(GameBase.LoadedSkin.NotePadding4K * GameBase.WindowUIScale);
                    DrawPlayfieldFirst = !GameBase.LoadedSkin.ReceptorsOverHitObjects4K;

                    // Update ManiaPlayfield
                    ManiaPlayfield.ReceptorYPosition = Config.ConfigManager.DownScroll4k  //todo: use list for scaling
                        ? GameBase.WindowRectangle.Height - (GameBase.LoadedSkin.ReceptorPositionOffset4K * GameBase.WindowUIScale + (laneSize * GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width))
                        : GameBase.LoadedSkin.ReceptorPositionOffset4K * GameBase.WindowUIScale;
                    ManiaPlayfield.ColumnLightingPosition = Config.ConfigManager.DownScroll4k
                        ? ManiaPlayfield.ReceptorYPosition
                        : ManiaPlayfield.ReceptorYPosition
                        + GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale
                        * (float)(((double)GameBase.LoadedSkin.NoteReceptorsUp4K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[0].Width)
                        - ((double)GameBase.LoadedSkin.NoteHitObjects4K[0][0].Height / GameBase.LoadedSkin.NoteHitObjects4K[0][0].Width));
                    Console.WriteLine(ManiaPlayfield.ColumnLightingPosition);

                    // Update Note Manager
                    ManiaNoteManager.ScrollSpeed = GameBase.WindowUIScale * ConfigManager.ScrollSpeed4k / (20f * GameBase.AudioEngine.PlaybackRate);
                    ManiaNoteManager.DownScroll = ConfigManager.DownScroll4k;
                    ManiaNoteManager.LaneSize = GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale;
                    ManiaNoteManager.HitPositionOffset = Config.ConfigManager.DownScroll4k
                        ? ManiaPlayfield.ReceptorYPosition + ((ConfigManager.UserHitPositionOffset4k + GameBase.LoadedSkin.HitPositionOffset4K) * GameBase.WindowUIScale)
                        : ManiaPlayfield.ReceptorYPosition - ((ConfigManager.UserHitPositionOffset4k + GameBase.LoadedSkin.HitPositionOffset4K) * GameBase.WindowUIScale)
                        + GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale
                        * (float)(((double)GameBase.LoadedSkin.NoteReceptorsUp4K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[0].Width)
                        - ((double)GameBase.LoadedSkin.NoteHitObjects4K[0][0].Height / GameBase.LoadedSkin.NoteHitObjects4K[0][0].Width));

                    // Update Score Manager
                    ManiaScoreManager.ScrollSpeed = ConfigManager.ScrollSpeed4k;
                    break;
                case GameModes.Keys7:
                    // Calculate References
                    ManiaGameplayReferences.ReceptorXPosition = new float[7];
                    laneSize = (int)(GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale);
                    playfieldPadding = (int)(GameBase.LoadedSkin.BgMaskPadding7K * GameBase.WindowUIScale);
                    receptorPadding = (int)(GameBase.LoadedSkin.NotePadding7K * GameBase.WindowUIScale);
                    DrawPlayfieldFirst = !GameBase.LoadedSkin.ReceptorsOverHitObjects7K;

                    // Update ManiaPlayfield
                    ManiaPlayfield.ReceptorYPosition = Config.ConfigManager.DownScroll7k  //todo: use list for scaling
                        ? GameBase.WindowRectangle.Height - (GameBase.LoadedSkin.ReceptorPositionOffset7K * GameBase.WindowUIScale + (laneSize * GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width))
                        : GameBase.LoadedSkin.ReceptorPositionOffset7K * GameBase.WindowUIScale;
                    ManiaPlayfield.ColumnLightingPosition = Config.ConfigManager.DownScroll7k
                        ? ManiaPlayfield.ReceptorYPosition
                        : ManiaPlayfield.ReceptorYPosition 
                        + GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale
                        * ((GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width)
                        - (GameBase.LoadedSkin.NoteHitObjects7K[0].Height / GameBase.LoadedSkin.NoteHitObjects7K[0].Width));

                    // Update Note Manager
                    ManiaNoteManager.ScrollSpeed = GameBase.WindowUIScale * ConfigManager.ScrollSpeed7k / (20f * GameBase.AudioEngine.PlaybackRate);
                    ManiaNoteManager.DownScroll = ConfigManager.DownScroll7k;
                    ManiaNoteManager.LaneSize = GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale;
                    ManiaNoteManager.HitPositionOffset = Config.ConfigManager.DownScroll7k
                        ? ManiaPlayfield.ReceptorYPosition + ((ConfigManager.UserHitPositionOffset7k + GameBase.LoadedSkin.HitPositionOffset7K) * GameBase.WindowUIScale)
                        : ManiaPlayfield.ReceptorYPosition - ((ConfigManager.UserHitPositionOffset7k + GameBase.LoadedSkin.HitPositionOffset7K) * GameBase.WindowUIScale)
                        + GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale
                        * (float)(((double)GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width)
                        - ((double)GameBase.LoadedSkin.NoteHitObjects7K[0].Height / GameBase.LoadedSkin.NoteHitObjects7K[0].Width));

                    // Update Score Manager
                    ManiaScoreManager.ScrollSpeed = ConfigManager.ScrollSpeed7k;
                    break;
            }

            // Calculate Config + Skin stuff
            playfieldSize = ((laneSize + receptorPadding) * ManiaGameplayReferences.ReceptorXPosition.Length) + (playfieldPadding * 2) - receptorPadding;
            ManiaNoteManager.PlayfieldSize = playfieldSize;
            ManiaPlayfieldUi.PlayfieldSize = playfieldSize;
            ManiaPlayfield.PlayfieldSize = playfieldSize;
            ManiaPlayfield.LaneSize = laneSize;
            ManiaPlayfield.PlayfieldPadding = playfieldPadding;
            ManiaPlayfield.ReceptorPadding = receptorPadding;
            //ManiaMeasureBarManager.PlayfieldSize = playfieldSize;

            // Get SV data
            ManiaNoteManager.SvQueue = ManiaTiming.GetSVQueue(qua);
            ManiaNoteManager.SvCalc = ManiaTiming.GetSVCalc(ManiaNoteManager.SvQueue);

            // Initialize class components
            ManiaPlayfield.Initialize(state);
            ManiaTiming.Initialize(state);
            ManiaAccuracyBox.Initialize(state);
            ManiaPlayfieldUi.Initialize(state);
            ManiaNoteManager.Initialize(state);
            ManiaParticleManager.Initialize(state);

            //todo: remove this. used for logging.
            Logger.Add("KeyCount", "", Color.Pink);
            Logger.Add("SongPos", "", Color.White);
            Logger.Add("Skippable", "", QuaverColors.NameTagAdmin);
            Logger.Add("Paused", "", QuaverColors.NameTagModerator);
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

            // Add a special replay frame for this event.
            ReplayHelper.AddReplayFrames(ReplayFrames, GameBase.SelectedBeatmap.Qua, ManiaScoreManager.Combo, CurrentSongTime);

            //Check for Note press/LN press
            //Reference Variables
            int noteIndex = -1;
            int i;

            // Update Receptor in ManiaPlayfield
            ManiaPlayfield.UpdateReceptor(keyLane.GetKey(), true);

            // Search for closest ManiaHitObject that is inside the HitTiming Window
            for (i = 0; i < ManiaNoteManager.HitObjectPoolSize && i < ManiaNoteManager.HitObjectPool.Count; i++)
            {
                if (ManiaNoteManager.HitObjectPool[i].KeyLane == keyLane.GetKey() + 1 && ManiaNoteManager.HitObjectPool[i].StartTime - CurrentSongTime > -ManiaScoreManager.HitWindowPress[4])
                {
                    noteIndex = i;
                    break;
                }
            }

            // If such ManiaHitObject exists, it will do key-press stuff to it
            if (noteIndex > -1)
            {
                // Play the correct hitsound on key press
                PlayHitsound(noteIndex);

                // Check which HitWindow this object's timing is in
                for (i = 0; i < 5; i++)
                {
                    if (Math.Abs(ManiaNoteManager.HitObjectPool[noteIndex].StartTime - CurrentSongTime) <= ManiaScoreManager.HitWindowPress[i])
                    {
                        // Update ManiaScoreManager and QuaverUserInterface if note was pressed on time
                        ManiaScoreManager.Count(i, false, ManiaNoteManager.HitObjectPool[noteIndex].StartTime - CurrentSongTime, CurrentSongTime * GameBase.AudioEngine.PlaybackRate);
                        ManiaAccuracyBox.UpdateAccuracyBox(i, ManiaScoreManager.JudgePressSpread[i], ManiaScoreManager.JudgeReleaseSpread[i], ManiaScoreManager.JudgeCount, ManiaScoreManager.ScoreTotal, ManiaScoreManager.Accuracy);
                        ManiaAccuracyBox.UpdateGradeBar(ManiaScoreManager.GetAccGradeIndex(), ManiaScoreManager.GetRelativeAccScale());
                        ManiaPlayfieldUi.UpdateJudge(i, ManiaScoreManager.Combo, false, ManiaNoteManager.HitObjectPool[noteIndex].StartTime - CurrentSongTime);

                        // If the player is spamming
                        if (i >= 3)
                        {
                            //If the object is an LN, don't forget to count it
                            if (ManiaNoteManager.HitObjectPool[noteIndex].IsLongNote)
                                ReleaseSkipped(null, null);

                            ManiaNoteManager.KillNote(noteIndex);
                            //ManiaNoteManager.RecycleNote(noteIndex);
                        }
                        else
                        {
                            // Create a Hit Burst instance
                            ManiaParticleManager.CreateHitBurst(ManiaNoteManager.NoteBurstRectangle[keyLane.GetKey()], keyLane.GetKey());

                            // If the object is an LN, hold it at the receptors
                            if (ManiaNoteManager.HitObjectPool[noteIndex].IsLongNote) ManiaNoteManager.HoldNote(noteIndex);

                            // If the object is not an LN, recycle it.
                            else ManiaNoteManager.RecycleNote(noteIndex);
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

            // Add replay frame for the key up event
            ReplayHelper.AddReplayFrames(ReplayFrames, GameBase.SelectedBeatmap.Qua, ManiaScoreManager.Combo, CurrentSongTime);

            //Reference Variables
            int noteIndex = -1;
            int i;

            // Update Receptor in ManiaPlayfield
            ManiaPlayfield.UpdateReceptor(keyLane.GetKey(), false);

            // Search for closest ManiaHitObject that is inside the HitTiming Window
            for (i = 0; i < ManiaNoteManager.HitObjectHold.Count; i++)
            {
                if (ManiaNoteManager.HitObjectHold[i].KeyLane == keyLane.GetKey() + 1)
                {
                    noteIndex = i;
                    break;
                }
            }

            // If such ManiaHitObject exists, it will do key-press stuff to it
            if (noteIndex > -1)
            {
                //Check which HitWindow this object's timing is in.
                //Since it's an LN, the hit window is increased by 1.25x.
                //Only checks MARV/PERF/GREAT/GOOD
                int rIndex = -1;
                for (i = 0; i < 4; i++)
                {
                    if (Math.Abs(ManiaNoteManager.HitObjectHold[noteIndex].EndTime - CurrentSongTime) < ManiaScoreManager.HitWindowRelease[i])
                    {
                        rIndex = i;
                        break;
                    }
                }

                // If LN has been released during a HitWindow
                if (rIndex > -1)
                {
                    // Update ManiaScoreManager and QuaverUserInterface if note was pressed on time
                    ManiaScoreManager.Count(rIndex, true);
                    ManiaAccuracyBox.UpdateAccuracyBox(rIndex, ManiaScoreManager.JudgePressSpread[rIndex], ManiaScoreManager.JudgeReleaseSpread[rIndex], ManiaScoreManager.JudgeCount, ManiaScoreManager.ScoreTotal, ManiaScoreManager.Accuracy);
                    ManiaAccuracyBox.UpdateGradeBar(ManiaScoreManager.GetAccGradeIndex(), ManiaScoreManager.GetRelativeAccScale());
                    ManiaPlayfieldUi.UpdateJudge(rIndex, ManiaScoreManager.Combo, true);
                    ManiaNoteManager.KillHold(noteIndex, true);
                }
                // If LN has been released early
                else
                {
                    // Update ManiaScoreManager and QuaverUserInterface if note was pressed on time
                    ManiaScoreManager.Count(5, true);
                    ManiaAccuracyBox.UpdateAccuracyBox(5, ManiaScoreManager.JudgePressSpread[i], ManiaScoreManager.JudgeReleaseSpread[i], ManiaScoreManager.JudgeCount, ManiaScoreManager.ScoreTotal, ManiaScoreManager.Accuracy);
                    ManiaAccuracyBox.UpdateGradeBar(ManiaScoreManager.GetAccGradeIndex(), ManiaScoreManager.GetRelativeAccScale());
                    ManiaPlayfieldUi.UpdateJudge(5, ManiaScoreManager.Combo, true);
                    ManiaNoteManager.KillHold(noteIndex);
                }
            }
        }

        public void PressMissed(object sender, EventArgs e)
        {
            // Play Combo-Break Sound
            if (ManiaScoreManager.Combo >= 20)
                GameBase.LoadedSkin.SoundComboBreak.Play(GameBase.SoundEffectVolume, 0, 0);

            // Manage QuaverUserInterface Helpers + Update Score Manager
            ManiaScoreManager.Count(5, false, 0, CurrentSongTime * GameBase.AudioEngine.PlaybackRate);
            ManiaAccuracyBox.UpdateAccuracyBox(5, ManiaScoreManager.JudgePressSpread[5], ManiaScoreManager.JudgeReleaseSpread[5], ManiaScoreManager.JudgeCount, ManiaScoreManager.ScoreTotal, ManiaScoreManager.Accuracy);
            ManiaAccuracyBox.UpdateGradeBar(ManiaScoreManager.GetAccGradeIndex(), ManiaScoreManager.GetRelativeAccScale());
            ManiaPlayfieldUi.UpdateJudge(5, ManiaScoreManager.Combo);
        }

        public void ReleaseSkipped(object sender, EventArgs e)
        {
            ManiaScoreManager.Count(5, true);
            ManiaAccuracyBox.UpdateAccuracyBox(5, ManiaScoreManager.JudgePressSpread[5], ManiaScoreManager.JudgeReleaseSpread[5], ManiaScoreManager.JudgeCount, ManiaScoreManager.ScoreTotal, ManiaScoreManager.Accuracy);
            ManiaAccuracyBox.UpdateGradeBar(ManiaScoreManager.GetAccGradeIndex(), ManiaScoreManager.GetRelativeAccScale());
            ManiaPlayfieldUi.UpdateJudge(5, ManiaScoreManager.Combo);
        }

        public void ReleaseMissed(object sender, EventArgs e)
        {
            ManiaScoreManager.Count(4, true);
            ManiaAccuracyBox.UpdateAccuracyBox(4, ManiaScoreManager.JudgePressSpread[4], ManiaScoreManager.JudgeReleaseSpread[4], ManiaScoreManager.JudgeCount, ManiaScoreManager.ScoreTotal, ManiaScoreManager.Accuracy);
            ManiaAccuracyBox.UpdateGradeBar(ManiaScoreManager.GetAccGradeIndex(), ManiaScoreManager.GetRelativeAccScale());
            ManiaPlayfieldUi.UpdateJudge(4, ManiaScoreManager.Combo);
        }

        /// <summary>
        ///     Skips to 3 seconds before the first ManiaHitObject.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SkipSong(object sender, EventArgs e)
        {
            if (!IntroSkippable || !GameBase.KeyboardState.IsKeyDown(ConfigManager.KeySkipIntro) || IntroSkipped)
                return;

            var skipTime = GameBase.SelectedBeatmap.Qua.HitObjects[0].StartTime - ManiaTiming.SONG_SKIP_OFFSET + AudioEngine.BassDelayOffset;

            try
            {
                // Add the skip frame here.
                ReplayHelper.AddReplayFrames(ReplayFrames, GameBase.SelectedBeatmap.Qua, ManiaScoreManager.Combo, CurrentSongTime, true);

                // Skip to the time if the audio already played once. If it hasn't, then play it.
                if (GameBase.AudioEngine.HasPlayed)
                    GameBase.AudioEngine.ChangeSongPosition(skipTime);
                else
                    GameBase.AudioEngine.Play(skipTime);

                // Set the actual song time to the position in the audio if it was successful.
                ManiaTiming.ActualSongTime = GameBase.AudioEngine.Position;
            }
            catch (AudioEngineException ex)
            {
                Logger.LogWarning("Trying to skip with no audio file loaded. Still continuing..", LogType.Runtime);

                // If there is no audio file, make sure the actual song time is set to the skip time.
                var actualSongTimeOffset = 10000; // The offset between the actual song time and audio position (?)
                ManiaTiming.ActualSongTime = skipTime + actualSongTimeOffset;
            }
            finally
            {
                // Skip to 3 seconds before the notes start
                IntroSkipped = true;
                ManiaTiming.SongIsPlaying = true;
                DiscordController.ChangeDiscordPresenceGameplay(true);
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
        ///     Plays the correct hitsounds based on the note index of the ManiaHitObject pool
        /// </summary>
        private void PlayHitsound(int noteIndex)
        {
            var hitObject = ManiaNoteManager.HitObjectPool[noteIndex];

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
