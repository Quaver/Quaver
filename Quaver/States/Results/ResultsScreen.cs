using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
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
using Quaver.Graphics.UI;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.States.Gameplay.Replays;
using Quaver.States.Select;
using AudioEngine = Quaver.Audio.AudioEngine;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;

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
        private ScoreProcessor ScoreProcessor { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="gameplay"></param>
        public ResultsScreen(GameplayScreen gameplay)
        {
            GameplayScreen = gameplay;
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

        /// <summary>
        ///     When loading up the results screen with a local score.
        /// </summary>
        /// <param name="score"></param>
        public ResultsScreen(LocalScore score)
        {
            GameBase.SelectedMap.Qua = GameBase.SelectedMap.LoadQua();
            Qua = GameBase.SelectedMap.Qua;
            
            var localPath = $"{ConfigManager.DataDirectory.Value}/r/{score.Id}.qr";
            
            // Try to find replay w/ local score id.
            // Otherwise we want to find 
            if (File.Exists(localPath))
            {
                Replay = new Replay(localPath);
            }
            // Otherwise we want to create an artificial replay.
            else
            {
                Replay = new Replay(score.Mode, score.Name, score.Mods, score.MapMd5)
                {
                    Date = Convert.ToDateTime(score.DateTime, CultureInfo.InvariantCulture),
                    Score = score.Score,
                    Accuracy = (float) score.Accuracy,
                    MaxCombo = score.MaxCombo,
                    CountMarv = score.CountMarv,
                    CountPerf = score.CountPerf,
                    CountGreat = score.CountGreat,
                    CountGood = score.CountGood,
                    CountOkay = score.CountOkay,
                    CountMiss = score.CountMiss
                };
            }
                    
            ScoreProcessor = new ScoreProcessorKeys(Replay);
            Type = ResultsScreenType.FromLocalScore;
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
                case ResultsScreenType.FromLocalScore:
                    InitializeFromLocalScore();
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
            HandleInput();
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
            DiscordManager.Presence.Timestamps = null;

            // Don't change if we're loading in from a replay file.
            if (Type == ResultsScreenType.FromReplayFile || GameplayScreen.InReplayMode)
            {
                DiscordManager.Presence.Details = "Idle";
                DiscordManager.Presence.State = "In the menus";
                DiscordManager.Client.SetPresence(DiscordManager.Presence);
                return;
            }

            
            var state = GameplayScreen.Failed ? "Fail" : "Pass";
            var score = $"{ScoreProcessor.Score / 1000}k";
            var acc = $"{StringHelper.AccuracyToString(ScoreProcessor.Accuracy)}";
            var grade = GameplayScreen.Failed ? "F" : GradeHelper.GetGradeFromAccuracy(ScoreProcessor.Accuracy).ToString();
            var combo = $"{ScoreProcessor.MaxCombo}x";

            DiscordManager.Presence.State = $"{state}: {grade} {score} {acc} {combo}";
            DiscordManager.Client.SetPresence(DiscordManager.Presence);
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

            // Don't add replay specific buttons if there is no data.
            if (!Replay.HasData)
                return;
            
            var watchReplay = new TextButton(new Vector2(200, 40), "Watch Replay")
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                PosY = 80
            };

            watchReplay.Clicked += (o, e) =>
            {                
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
                    case ResultsScreenType.FromLocalScore:
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

                try
                {
                    AudioEngine.Fade(0, 500);
                }
                catch (AudioEngineException ex) {}
                
                IsExitingScreen = true;     
            };

            if (Replay.Mods.HasFlag(ModIdentifier.Autoplay))
                return;
            
            var export = new TextButton(new Vector2(200, 40), "Export Replay")
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                PosY = 160
            };

            export.Clicked += (o, e) =>
            {
                ExportReplay();
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
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
                Font = Fonts.AssistantRegular16,
                Text = SongTitle
            };
            
            Mapper = new SpriteText()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosY = -250,
                Font = Fonts.AssistantRegular16,
                Text = $"Mapped By: {Qua.Creator}"
            };

            Date = new SpriteText()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosY = -200,
                Font = Fonts.AssistantRegular16,
                Text = $"Played By: {Replay.PlayerName} At: "
            };
            
            Date.Text += GameplayScreen != null && GameplayScreen.HasQuit ? $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}" 
                                                        : $"{Replay.Date.ToShortDateString()} {Replay.Date.ToShortTimeString()}";
            var score = new SpriteText()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosY = -150,
                Font = Fonts.AssistantRegular16,
                Text = $"Score: {ScoreProcessor.Score:N0}",
                TextColor = Colors.MainAccent
            };
            
            var acc = new SpriteText()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosY = -100,
                Font = Fonts.AssistantRegular16,
                Text = $"Accuracy: {StringHelper.AccuracyToString(ScoreProcessor.Accuracy)}",
                TextColor = Colors.SecondaryAccent
            };
            
            var maxCombo = new SpriteText()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosY = -50,
                Font = Fonts.AssistantRegular16,
                Text = $"Max Combo: {ScoreProcessor.MaxCombo:N0}x",
                TextColor = Colors.Negative
            };
            
            Judgements = new List<SpriteText>();

            for (var i = 0; i < ScoreProcessor.JudgementWindow.Count; i++)
            {
                var judgement = (Judgement) i;
                
                Judgements.Add(new SpriteText()
                {
                    Parent = Container,
                    Alignment = Alignment.MidCenter,
                    PosY = 35 * i + 0,
                    Font = Fonts.AssistantRegular16,
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
            // Keep the same replay and score processor if the user was watching a replay before.
            if (GameplayScreen.InReplayMode)
            {
                Replay = GameplayScreen.LoadedReplay;

                if (Replay.Mods.HasFlag(ModIdentifier.Autoplay))
                    ScoreProcessor = GameplayScreen.Ruleset.ScoreProcessor;
                else
                    ScoreProcessor = new ScoreProcessorKeys(Replay);
            }
            // Otherwise the replay and processor should be the one that the user just played.
            else
            {
                // Populate the replay with values from the score processor.
                Replay = GameplayScreen.ReplayCapturer.Replay;
                ScoreProcessor = GameplayScreen.Ruleset.ScoreProcessor;
                
                Replay.FromScoreProcessor(ScoreProcessor);
            }

            ChangeDiscordPresence();
            PlayApplauseEffect();
            
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
        ///     Initialize the screen from a local score.
        /// </summary>
        private void InitializeFromLocalScore() {}
        
        /// <summary>
        ///     Saves a local score to the database.
        /// </summary>
        private void SaveLocalScore()
        {
            Task.Run(async () =>
            {
                var scoreId = 0;
                try
                {
                    var localScore = LocalScore.FromScoreProcessor(ScoreProcessor, Md5, ConfigManager.Username.Value, ScrollSpeed);                    
                    scoreId = await LocalScoreCache.InsertScoreIntoDatabase(localScore);         
                }
                catch (Exception e)
                {
                    Logger.LogError($"There was a fatal error when saving the local score!" + e.Message, LogType.Runtime);
                }
                
                try
                {
                    Replay.Write($"{ConfigManager.DataDirectory}/r/{scoreId}.qr");
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
        
        /// <summary>
        ///     Handles input for the entire screen.
        /// </summary>
        private void HandleInput()
        {
            if (InputHelper.IsUniqueKeyPress(Keys.F2))
                ExportReplay();
        }

        /// <summary>
        ///     Exports the currently looked at replay.
        /// </summary>
        private void ExportReplay()
        {
            if (!Replay.HasData)
            {
                Logger.LogError($"Replay doesn't have any data", LogType.Runtime);
                return;
            }

            if (Replay.Mods.HasFlag(ModIdentifier.Autoplay))
            {
                Logger.LogError($"Exporting autoplay replays is disabled", LogType.Runtime);
                return;             
            }
            
            Logger.LogImportant($"Just a second... We're exporting your replay!", LogType.Network, 2.0f);

            Task.Run(() =>
            {
                var path = $@"{ConfigManager.ReplayDirectory.Value}/{Replay.PlayerName} - {SongTitle} - {DateTime.Now:yyyyddMMhhmmss}{GameBase.GameTime.ElapsedMilliseconds}.qr";
                Replay.Write(path);
            
                // Open containing folder
                Process.Start("explorer.exe", "/select, \"" + path.Replace("/", "\\") + "\"");
            
                Logger.LogSuccess($"Replay successfully exported", LogType.Runtime);
            });
        }
    }
}