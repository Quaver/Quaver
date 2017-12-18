using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
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

namespace Quaver.GameState.Gameplay
{
    /// <summary>
    /// This class handles the interaction between note and input.
    /// </summary>
    class NoteManager : IHelper
    {
        public GameplayUI GameplayUI { get; set; }

        public NoteRendering NoteRendering { get; set; }

        public Playfield Playfield { get; set; }

        public Timing Timing { get; set; }

        public ScoreManager ScoreManager { get; set; }

        /// <summary>
        ///     The MD5 Hash of the played beatmap.
        /// </summary>
        private string BeatmapMd5 { get; set; }

        /// <summary>
        ///     The input manager for this game state.
        /// </summary>
        private GameplayInputManager InputManager { get; set; }

        /// <summary>
        ///     Holds the list of replay frames for this state.
        /// </summary>
        private List<ReplayFrame> ReplayFrames { get; set; }

        /// <summary>
        ///     Keeps track of whether or not the song intro is current skippable.
        /// </summary>
        private bool IntroSkippable { get; set; }

        //todo: remove. TEST.
        private Sprite TextUnder { get; set; }
        private TextBoxSprite SVText { get; set; }
        private Button TestButton { get; set; }

        /// <summary>
        ///     Constructor, data passed in from loading state
        /// </summary>
        /// <param name="beatmapMd5"></param>
        public NoteManager(string beatmapMd5)
        {
            BeatmapMd5 = beatmapMd5;
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

        public void Draw()
        {
            TestButton.Draw();
            TextUnder.Draw();
            Playfield.DrawUnder();
            NoteRendering.Draw();
            Playfield.DrawOver();
            GameplayUI.Draw();
            TestButton.Draw();
        }

        public void Initialize(IGameState playScreen)
        {
            // Create Class Components
            GameplayUI = new GameplayUI();
            NoteRendering = new NoteRendering();
            Playfield = new Playfield();
            Timing = new Timing();
            ScoreManager = new ScoreManager();

            // Initialize Gameplay
            
            InitializeGameplay(playScreen);

            //Todo: Remove. TEST.
            TestButton = new TextButton(new Vector2(200, 30), "BACK")
            {
                Image = GameBase.LoadedSkin.ColumnTimingBar,
                Alignment = Alignment.TopCenter
            };
            TestButton.Clicked += BackButtonClick;

            TextUnder = new Sprite()
            {
                Image = GameBase.UI.HollowBox,
                Tint = Color.Blue,
                Size = new Vector2(250, 200),
                Alignment = Alignment.TopRight
            };
        }

        /// <summary>
        /// This method gets called when a key gets pressed.
        /// </summary>
        /// <param name="keyLane"></param>
        public void Input(int keyLane, bool keyDown)
        {
            // Update Receptor in Playfield
            Playfield.UpdateReceptor(keyLane, keyDown);

            //Check for Note press/LN press
            if (keyDown)
            {
                //Reference Variables
                int noteIndex = -1;
                int i;

                //Search for closest HitObject that is inside the HitTiming Window
                for (i = 0; i < NoteRendering.HitObjectPoolSize && i < NoteRendering.HitObjectPool.Count; i++)
                {
                    if (NoteRendering.HitObjectPool[i].KeyLane == keyLane + 1 && NoteRendering.HitObjectPool[i].StartTime - Timing.CurrentSongTime > -ScoreManager.HitWindowPress[4])
                    {
                        noteIndex = i;
                        break;
                    }
                }

                //If such HitObject exists, it will do key-press stuff to it
                if (noteIndex > -1)
                {
                    //Check which HitWindow this object's timing is in
                    for (i = 0; i < 5; i++)
                    {
                        if (Math.Abs(NoteRendering.HitObjectPool[noteIndex].StartTime - Timing.CurrentSongTime) <= ScoreManager.HitWindowPress[i])
                        {
                            //Score manager stuff
                            ScoreManager.Count(i, false, NoteRendering.HitObjectPool[noteIndex].StartTime - Timing.CurrentSongTime, Timing.CurrentSongTime/ SongManager.Length);
                            GameplayUI.UpdateAccuracyBox(i);
                            Playfield.UpdateJudge(i, false, NoteRendering.HitObjectPool[noteIndex].StartTime - Timing.CurrentSongTime);

                            // If the player is spamming
                            if (i >= 3)
                                NoteRendering.KillNote(noteIndex);
                            else
                            {
                                //If the object is an LN, hold it at the receptors
                                if (NoteRendering.HitObjectPool[noteIndex].IsLongNote) NoteRendering.HoldNote(noteIndex);

                                //If the object is not an LN, recycle it.
                                else NoteRendering.RecycleNote(noteIndex);
                            }

                            break;
                        }
                    }
                }
            }
            //Check for LN release
            else
            {
                //Reference Variables
                int noteIndex = -1;
                int i;

                //Search for closest HitObject that is inside the HitTiming Window
                for (i = 0; i < NoteRendering.HitObjectHold.Count; i++)
                {
                    if (NoteRendering.HitObjectHold[i].KeyLane == keyLane + 1)
                    {
                        noteIndex = i;
                        break;
                    }
                }

                //If such HitObject exists, it will do key-press stuff to it
                if (noteIndex > -1)
                {
                    //Check which HitWindow this object's timing is in.
                    //Since it's an LN, the hit window is increased by 1.25x.
                    //Only checks MARV/PERF/GREAT/GOOD
                    int releaseTiming = -1;
                    for (i = 0; i < 4; i++)
                    {
                        if (Math.Abs(NoteRendering.HitObjectHold[noteIndex].EndTime - Timing.CurrentSongTime) <= ScoreManager.HitWindowRelease[i])
                        {
                            releaseTiming = i;
                            break;
                        }
                    }

                    //If LN has been released during a HitWindow
                    if (releaseTiming > -1)
                    {
                        ScoreManager.Count(i, true);
                        GameplayUI.UpdateAccuracyBox(i);
                        Playfield.UpdateJudge(i, true);
                        NoteRendering.KillHold(noteIndex,true);
                    }
                    //If LN has been pressed early
                    else
                    {
                        ScoreManager.Count(5, true);
                        GameplayUI.UpdateAccuracyBox(5);
                        Playfield.UpdateJudge(5, true);
                        NoteRendering.KillHold(noteIndex);
                    }
                }
            }
        }

        public void UnloadContent()
        {
            //NoteRendering.UnloadContent();
            Timing.UnloadContent();
            Playfield.UnloadContent();
            GameplayUI.UnloadContent();
            NoteRendering.UnloadContent();

            //todo: remove this later
            TestButton.Clicked -= BackButtonClick;
            TextUnder.Destroy();
        }

        public void Update(double dt)
        {
            TestButton.Update(dt);
            TextUnder.Update(dt);
            
            // Set the current song time.
            Timing.Update(dt);
            GameplayReferences.CurrentSongTime = Timing.CurrentSongTime;

            // Check if the song is currently skippable.
            IntroSkippable = (GameBase.SelectedBeatmap.Qua.HitObjects[0].StartTime - Timing.CurrentSongTime >= 5000);

            // Update the playfield
            Playfield.Update(dt);

            // Update the Notes
            NoteRendering.Update(dt);

            // Update Data Interface
            GameplayUI.Update(dt);

            // Check the input for this particular game state.
            InputManager.CheckInput(IntroSkippable, ReplayFrames);

            // Update Loggers. todo: remove
            Logger.Update("KeyCount", $"Key Count: {GameBase.SelectedBeatmap.Qua.KeyCount}");
            Logger.Update("SongPos", "Current Track Position: " + NoteRendering.TrackPosition);
            Logger.Update("Skippable", $"Intro Skippable: {IntroSkippable}");

            //Todo: remove. TEST.
            TextUnder.Update(dt);
            

            if (Timing.PlayingIsDone)
                GameBase.GameStateManager.ChangeState(new ScoreScreenState(BeatmapMd5, ScoreManager, GameBase.SelectedBeatmap.Artist, GameBase.SelectedBeatmap.Title, GameBase.SelectedBeatmap.DifficultyName, ReplayFrames));
                
        }

        /// <summary>
        ///     Solely responsible for intializing gameplay aspects
        /// </summary>
        private void InitializeGameplay(IGameState state)
        {
            //Initialize Score Manager
            //todo: temp
            
            var count = 0;
            var total = GameBase.SelectedBeatmap.Qua.HitObjects.Count;

            foreach (var ho in GameBase.SelectedBeatmap.Qua.HitObjects)
            {
                if (ho.EndTime > ho.StartTime) count++;
            }

            ScoreManager = new ScoreManager();
            ScoreManager.Initialize(total + count, GameBase.SelectedBeatmap.Qua.Judge); //TODO: ADD RELEASE COUNTS AS WELL

            //Initialize class components
            Playfield.Initialize(state);
            Timing.Initialize(state);
            NoteRendering.Initialize(state);
            GameplayUI.Initialize(state);

            // Create Gameplay classes
            InputManager = new GameplayInputManager(this); //todo: idk wtf this does yet - staravia
            ReplayFrames = new List<ReplayFrame>();


            //todo: remove this. used for logging.
            // Create loggers
            Logger.Add("KeyCount", "", Color.Pink);
            Logger.Add("SongPos", "", Color.White);
            Logger.Add("Skippable", "", CustomColors.NameTagAdmin);
            Logger.Add("JudgeDifficulty", "", CustomColors.NameTagModerator);

            // Update hit window logger
            var loggertext = "Hitwindow: Judge: " + ScoreManager.JudgeDifficulty + "   Press: ";
            foreach (var a in ScoreManager.HitWindowPress) loggertext += Math.Floor(a) + "ms, ";
            loggertext += "   Release: ";
            foreach (var a in ScoreManager.HitWindowRelease) loggertext += Math.Floor(a) + "ms, ";

            // Logger.Update("JudgeDifficulty", loggertext);
        }
    }
}
