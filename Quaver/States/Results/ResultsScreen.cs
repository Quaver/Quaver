using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UserInterface;
using Quaver.Main;
using Quaver.States.Gameplay;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Discord;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.States.Gameplay.Replays;
using Quaver.States.Select;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.States.Results
{
    internal class ResultsScreen : IGameState
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public State CurrentState { get; set; } = State.Results;

        /// <summary>
        ///     The type of results screen.
        /// </summary>
        private ResultsScreenType Type { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     Reference to the gameplay screen that was just played.
        /// </summary>
        private GameplayScreen GameplayScreen { get; }

        /// <summary>
        ///     The .qua that this is results screen is referencing to.
        /// </summary>
        private Qua Qua { get; set; }

        /// <summary>
        ///     Container for all sprites.
        /// </summary>
        private Container Container { get; set; }

        /// <summary>
        ///     Transitioner for this screen.
        /// </summary>
        private Sprite ScreenTransitioner { get; set; }

        /// <summary>
        ///     Back to menu button.
        /// </summary>
        private TextButton Back { get; set; }

        /// <summary>
        ///     If we're currently exiting the screen.
        /// </summary>
        private bool IsExitingScreen { get; set; }

        /// <summary>
        ///     When exiting the screen, this is the action to perform.
        /// </summary>
        private EventHandler OnExitingScreen { get; set; }

        /// <summary>
        ///     When we invoke the OnExitingScreen event for the first time, this'll be set to true
        ///     to avoid spam calling the method, since it's called in update.
        /// </summary>
        private bool ExitHandlerExecuted { get; set;  }

        /// <summary>
        ///     When the user is exiting the screen, this counter will determine when
        ///     to switch to the next screen.
        /// </summary>
        private double TimeSinceExitingScreen { get; set; }

        /// <summary>
        ///     Applause sound effect.
        /// </summary>
        private SoundEffectInstance ApplauseSound { get; set; }

        /// <summary>
        ///     Displays the song title
        /// </summary>
        private SpriteText SongInfo { get; set; }

        /// <summary>
        ///     The mapper of the map.
        /// </summary>
        private SpriteText Mapper { get; set; }

        /// <summary>
        ///     Date
        /// </summary>
        private SpriteText Date { get; set; }

        /// <summary>
        ///     Judgement Texts
        /// </summary>
        private List<SpriteText> Judgements { get; set; }

        /// <summary>
        ///     Song title + Difficulty name.
        /// </summary>
        private string SongTitle => $"{Qua.Artist} - {Qua.Title} [{Qua.DifficultyName}]";

        /// <summary>
        ///     MD5 Hash of the map played.
        /// </summary>
        private string Md5 => GameplayScreen.MapHash;
        
        /// <summary>
        ///     The user's scroll speed.
        /// </summary>
        private int ScrollSpeed => Qua.Mode == GameMode.Keys4 ? ConfigManager.ScrollSpeed4K.Value : ConfigManager.ScrollSpeed7K.Value;

        /// <summary>
        ///     The replay that was just played.
        /// </summary>
        private Replay Replay { get; set; }

        /// <summary>
        ///     Score processor.        
        /// </summary>
        private ScoreProcessor ScoreProcessor { get; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="gameplay"></param>
        public ResultsScreen(GameplayScreen gameplay)
        {
            GameplayScreen = gameplay;
            ScoreProcessor = GameplayScreen.Ruleset.ScoreProcessor;
            Qua = GameplayScreen.Map;
            Type = ResultsScreenType.FromGameplay;
        }

        /// <summary>
        ///     When going to the results screen with just a replay.
        /// </summary>
        /// <param name="replay"></param>
        public ResultsScreen(Replay replay)
        {
            Replay = replay;
            ScoreProcessor = new ScoreProcessorKeys(Replay);
            Type = ResultsScreenType.FromReplayFile;
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {
            Container = new Container();
            
            // Initialize the state depending on if we're coming from the gameplay screen
            // or loading up a replay file.
            switch (Type)
            {
                case ResultsScreenType.FromGameplay:
                    InitializeFromGameplay();
                    break;
                case ResultsScreenType.FromReplayFile:
                    InitializeFromReplayFile();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
#region SPRITE_CREATION           
            CreateBackButton();
            CreateScreenText();
            CreateGrade();
            
            // Create Screen Transitioner. Draw Last!
            ScreenTransitioner = new Sprite
            {
                Parent = Container,
                Tint = Color.Black,
                ScaleX = 1,
                ScaleY = 1
            };
            
            BackgroundManager.Blacken();
#endregion
              
            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <exception cref="!:NotImplementedException"></exception>
        public void UnloadContent()
        {
            Container.Destroy();
        }

         /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {            
            Container.Update(dt);
            HandleScreenTransitions(dt);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.Black);
            GameBase.SpriteBatch.Begin();
            
            BackgroundManager.Draw();
            Container.Draw();
            
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Handles all screen tra
        /// </summary>
        /// <param name="dt"></param>
        private void HandleScreenTransitions(double dt)
        {
            // Allow the cursor to be shown again regardless
            GameBase.Cursor.FadeIn(dt, 240);

            // Fade-In
            if (!IsExitingScreen)
            {
                // Fade background back in.        
                BackgroundManager.Readjust();
                ScreenTransitioner.FadeOut(dt, 120);      
            }
            // Exiting Screen
            else
            {
                // Add to the time if the user is exiting the screen in any way.
                TimeSinceExitingScreen += dt;
                
                // Fade BG
                BackgroundManager.Blacken();
                
                // Fade Screen
                ScreenTransitioner.FadeIn(dt, 120);   
                
                // Switch to the song select state after a second.
                if (!(TimeSinceExitingScreen >= 500) || ExitHandlerExecuted) 
                    return;
                
                OnExitingScreen?.Invoke(this, new EventArgs());
                ExitHandlerExecuted = true;
            }
        }

        /// <summary>
        ///     Changes discord rich presence to show results.
        /// </summary>
        private void ChangeDiscordPresence()
        {
            // Don't change if we're loading in from a replay file.
            if (Type == ResultsScreenType.FromReplayFile || GameplayScreen.InReplayMode)
                return;
            
            var state = GameplayScreen.Failed ? "Fail" : "Pass";
            var score = $"{ScoreProcessor.Score / 1000}k";
            var acc = $"{StringHelper.AccuracyToString(ScoreProcessor.Accuracy)}";
            var grade = GameplayScreen.Failed ? "F" : GradeHelper.GetGradeFromAccuracy(ScoreProcessor.Accuracy).ToString();
            var combo = $"{ScoreProcessor.MaxCombo}x";
            
            DiscordController.ChangeDiscordPresence(SongTitle, $"{state}: {score} {acc} {grade} {combo}");
        }
        
        /// <summary>
        ///     When the back button is clicked. It should start the screen exiting process.
        /// </summary>
        private void CreateBackButton()
        {
            Back = new TextButton(new Vector2(200, 40), "Back To Menu")
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
            };

            Back.Clicked += (o, e) =>
            {             
                ExitScreen((obj, sender) => GameBase.GameStateManager.ChangeState(new SongSelectState()));                
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundBack); 
                
                ApplauseSound?.Stop();
            };
            
            var watchReplay = new TextButton(new Vector2(200, 40), "Watch Replay")
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                PosY = 80
            };

            watchReplay.Clicked += (o, e) =>
            {
                GameBase.AudioEngine.Pause();
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
                
                switch (Type)
                {
                    // If user is coming from gameplay, so use that generated replay.
                    case ResultsScreenType.FromGameplay:
                        var replayToLoad = GameplayScreen.InReplayMode ? GameplayScreen.LoadedReplay : GameplayScreen.ReplayCapturer.Replay;
                    
                        ExitScreen(async (obj, sender) =>
                        {
                            var scores = await LocalScoreCache.FetchMapScores(GameplayScreen.MapHash);
                            var screen = new GameplayScreen(Qua, GameplayScreen.MapHash, scores, replayToLoad);
                            
                            GameBase.GameStateManager.ChangeState(screen);
                        });
                        break;
                    // If user is loading up from a replay file, use that incoming replay file.
                    case ResultsScreenType.FromReplayFile:
                        ExitScreen(async (obj, sender) =>
                        {
                            var scores = await LocalScoreCache.FetchMapScores(GameBase.SelectedMap.Md5Checksum);
                            var screen = new GameplayScreen(Qua, GameBase.SelectedMap.Md5Checksum, scores, Replay);
                            
                            GameBase.GameStateManager.ChangeState(screen);
                        });          
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                                  
                IsExitingScreen = true;     
            };
        }

        /// <summary>
        ///     Creates the text for the results screen
        /// </summary>
        private void CreateScreenText()
        {
            SongInfo = new SpriteText()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosY = -300,
                Font = QuaverFonts.AssistantRegular16,
                Text = SongTitle
            };
            
            Mapper = new SpriteText()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosY = -250,
                Font = QuaverFonts.AssistantRegular16,
                Text = $"Mapped By: {Qua.Creator}"
            };

            Date = new SpriteText()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosY = -200,
                Font = QuaverFonts.AssistantRegular16,
                Text = $"Played At: {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}"
            };
            
            Judgements = new List<SpriteText>();

            for (var i = 0; i < ScoreProcessor.JudgementWindow.Count; i++)
            {
                var judgement = (Judgement) i;
                
                Judgements.Add(new SpriteText()
                {
                    Parent = Container,
                    Alignment = Alignment.MidCenter,
                    PosY = 35 * i + -150,
                    Font = QuaverFonts.AssistantRegular16,
                    Text = $"{judgement.ToString()}: {ScoreProcessor.CurrentJudgements[judgement]}",
                    TextColor = GameBase.Skin.Keys[Qua.Mode].JudgeColors[judgement]
                });
            }
        }

        /// <summary>
        ///     Creates the achieved grade sprite.
        /// </summary>
        private void CreateGrade()
        {
            Texture2D gradeTexture;

            if (GameplayScreen != null && GameplayScreen.Failed)
                gradeTexture = GameBase.Skin.Grades[Grade.F];
            else
                gradeTexture = GameBase.Skin.Grades[GradeHelper.GetGradeFromAccuracy(ScoreProcessor.Accuracy)];
            
            var grade = new Sprite()
            {
                Parent = Container,
                Image = gradeTexture,
                Size = new UDim2D(gradeTexture.Width * 0.5f, gradeTexture.Height * 0.5f),
                Alignment = Alignment.MidRight
            };

            grade.PosX = -grade.SizeX;
        }
        
        /// <summary>
        ///     Plays the appluase sound effect.
        /// </summary>
        private void PlayApplauseEffect()
        {
            ApplauseSound = GameBase.Skin.SoundApplause.CreateInstance();
            
            if (!GameplayScreen.Failed && ScoreProcessor.Accuracy >= 80 && !GameplayScreen.InReplayMode)
                ApplauseSound.Play();
        }

        /// <summary>
        ///     Goes through the score submission process.
        /// </summary>
        private void SubmitScore()
        {
            // Don't save scores if the user quit themself.
            if (GameplayScreen.HasQuit || GameplayScreen.InReplayMode)
                return;

            // Run all of these tasks inside of a new thread to avoid blocks.
            var t = new Thread(() =>
            {
                SaveLocalScore();
                SaveReplay(); 
#if DEBUG
                SaveDebugReplayData();
#endif
            });
            
            t.Start();
        }

        /// <summary>
        ///     Initializes the results screen if we're coming from the gameplay screen.
        /// </summary>
        private void InitializeFromGameplay()
        {
            ChangeDiscordPresence();
            PlayApplauseEffect();

            // Populate the replay with values from the score processor.
            Replay = GameplayScreen.ReplayCapturer.Replay;
            Replay.FromScoreProcessor(ScoreProcessor);

            // Submit score
            SubmitScore();
        }

        /// <summary>
        ///     Initialize the screen if we're coming from a replay file.
        /// </summary>
        private void InitializeFromReplayFile()
        {
            var mapset = GameBase.Mapsets.FirstOrDefault(x => x.Maps.Any(y => y.Md5Checksum == Replay.MapMd5));

            // Send the user back to the song select screen with an error if there was no found mapset.
            if (mapset == null)
            {
                Logger.LogError($"You do not have the map that this replay is for", LogType.Runtime);
                GameBase.GameStateManager.ChangeState(new SongSelectState());
                return;
            }

            // Find the map that actually has the correct hash.
            var map = mapset.Maps.Find(x => x.Md5Checksum == Replay.MapMd5);         
            Map.ChangeSelected(map);
            
            // Load up the .qua file and change the selected map's Qua.
            Qua = map.LoadQua();
            GameBase.SelectedMap.Qua = Qua;
                            
            // Make sure the background is loaded, we don't run this async because we
            // want it to be loaded when the user starts the map.
            BackgroundManager.LoadBackground();
            BackgroundManager.Change(GameBase.CurrentBackground);
                    
            // Reload and play song.
            try
            {
                GameBase.AudioEngine.ReloadStream();
                GameBase.AudioEngine.Play();
            }
            catch (AudioEngineException e)
            {
                // No need to handle here.
            }
        }

        /// <summary>
        ///     Saves a local score to the database.
        /// </summary>
        private void SaveLocalScore()
        {
            // Save local score.
            Task.Run(async () =>
            {
                try
                {
                    var localScore = LocalScore.FromScoreProcessor(ScoreProcessor, Md5, ConfigManager.Username.Value, ScrollSpeed);
                    await LocalScoreCache.InsertScoreIntoDatabase(localScore);         
                }
                catch (Exception e)
                {
                    Logger.LogError($"There was a fatal error when saving the local score!" + e.Message, LogType.Runtime);
                }
            });
        }

        /// <summary>
        ///     Saves a replay to the data path.
        /// </summary>
        private void SaveReplay()
        {
            // Save replay.
            Task.Run(() =>
            {
                try
                {
                    Replay.Write($@"c:\users\admin\desktop\replay.qr");
                }
                catch (Exception e)
                {
                    Logger.LogError($"There was an error when writing the replay: " + e, LogType.Runtime);
                }
            });      
        }

        /// <summary>
        ///     Saves replay data related to debugging.
        /// </summary>
        private void SaveDebugReplayData()
        {
            // Save debug replay and hit stat data.
            Task.Run(() =>
            {
                try
                {
                    File.WriteAllText($"{ConfigManager.DataDirectory.Value}/replay_debug.txt", Replay.FramesToString(true));

                    var hitStats = "";
                    GameplayScreen.Ruleset.ScoreProcessor.Stats.ForEach(x => hitStats += $"{x.ToString()}\r\n");
                    File.WriteAllText($"{ConfigManager.DataDirectory.Value}/replay_debug_hitstats.txt", hitStats);
                }
                catch (Exception e)
                {
                    Logger.LogError($"There was an error when writing debug replay files: {e}", LogType.Runtime);
                }
            });
        }

        /// <summary>
        ///     Exits the screen with an action to perform when it has finished.
        /// </summary>
        /// <param name="action"></param>
        private void ExitScreen(EventHandler action)
        {
            OnExitingScreen = null;
            OnExitingScreen += action;

            IsExitingScreen = true;
        }
    }
}