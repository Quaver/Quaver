using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using Quaver.Config;
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
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     Reference to the gameplay screen that was just played.
        /// </summary>
        private GameplayScreen GameplayScreen { get; }

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
        private string SongTitle => $"{GameplayScreen.Map.Artist} - {GameplayScreen.Map.Title} [{GameplayScreen.Map.DifficultyName}]";

        /// <summary>
        ///     MD5 Hash of the map played.
        /// </summary>
        private string Md5 => GameplayScreen.MapHash;
        
        /// <summary>
        ///     The user's scroll speed.
        /// </summary>
        private int ScrollSpeed => GameplayScreen.Map.Mode == GameMode.Keys4 ? ConfigManager.ScrollSpeed4K.Value : ConfigManager.ScrollSpeed7K.Value;

        /// <summary>
        ///     The replay that was just played.
        /// </summary>
        private Replay Replay { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="gameplay"></param>
        public ResultsScreen(GameplayScreen gameplay) => GameplayScreen = gameplay;
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {
            Container = new Container();
            
#region SPRITE_CREATION           
            CreateBackButton();
            CreateScreenText();
            CreateGrade();
            
            // Create Screen Transitioner. Draw Last!
            ScreenTransitioner = new Sprite
            {
                Parent = Container,
                Tint = Color.Black,
                Alpha = 1,
                ScaleX = 1,
                ScaleY = 1
            };
#endregion
            UpdateReady = true;
            ChangeDiscordPresence();
            PlayApplauseEffect();
            
            // Populate the replay with values from the score processor.
            Replay = GameplayScreen.ReplayCapturer.Replay;              
            Replay.FromScoreProcessor(GameplayScreen.Ruleset.ScoreProcessor);
            
            // Submit score
            SubmitScore();
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

                ScreenTransitioner.FadeOut(dt, 240);      
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
                if (TimeSinceExitingScreen >= 1000)
                    GameBase.GameStateManager.ChangeState(new SongSelectState());
            }
        }

        /// <summary>
        ///     Changes discord rich presence to show results.
        /// </summary>
        private void ChangeDiscordPresence()
        {
            var state = GameplayScreen.Failed ? "Fail" : "Pass";
            var score = $"{GameplayScreen.Ruleset.ScoreProcessor.Score / 1000}k";
            var acc = $"{StringHelper.AccuracyToString(GameplayScreen.Ruleset.ScoreProcessor.Accuracy)}";
            var grade = GameplayScreen.Failed ? "F" : GradeHelper.GetGradeFromAccuracy(GameplayScreen.Ruleset.ScoreProcessor.Accuracy).ToString();
            var combo = $"{GameplayScreen.Ruleset.ScoreProcessor.MaxCombo}x";
            
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
                IsExitingScreen = true;
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundBack);  
                ApplauseSound.Stop();
            };
            
            var watchReplay = new TextButton(new Vector2(200, 40), "Watch Replay")
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                PosY = 80
            };

            watchReplay.Clicked += (o, e) =>
            {
                IsExitingScreen = true;
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
                
                Task.Run(async () =>
                {
                    var replayToLoad = GameplayScreen.InReplayMode ? GameplayScreen.LoadedReplay : GameplayScreen.ReplayCapturer.Replay;
                    
                    GameBase.GameStateManager.ChangeState(new GameplayScreen(GameplayScreen.Map, GameplayScreen.MapHash, 
                            await LocalScoreCache.FetchMapScores(GameplayScreen.MapHash), replayToLoad));
                }).Wait();            
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
                Text = $"Mapped By: {GameplayScreen.Map.Creator}"
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

            for (var i = 0; i < GameplayScreen.Ruleset.ScoreProcessor.JudgementWindow.Count; i++)
            {
                var judgement = (Judgement) i;
                
                Judgements.Add(new SpriteText()
                {
                    Parent = Container,
                    Alignment = Alignment.MidCenter,
                    PosY = 35 * i + -150,
                    Font = QuaverFonts.AssistantRegular16,
                    Text = $"{judgement.ToString()}: {GameplayScreen.Ruleset.ScoreProcessor.CurrentJudgements[judgement]}",
                    TextColor = GameBase.Skin.Keys[GameplayScreen.Map.Mode].JudgeColors[judgement]
                });
            }
        }

        /// <summary>
        ///     Creates the achieved grade sprite.
        /// </summary>
        private void CreateGrade()
        {
            Texture2D gradeTexture;

            if (GameplayScreen.Failed)
                gradeTexture = GameBase.Skin.Grades[Grade.F];
            else
                gradeTexture = GameBase.Skin.Grades[GradeHelper.GetGradeFromAccuracy(GameplayScreen.Ruleset.ScoreProcessor.Accuracy)];
            
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
            
            if (!GameplayScreen.Failed && GameplayScreen.Ruleset.ScoreProcessor.Accuracy >= 80 && !GameplayScreen.InReplayMode)
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

            var t = new Thread(() =>
            {
                // Save local score.
                Task.Run(async () =>
                {
                    try
                    {
                        var localScore = LocalScore.FromScoreProcessor(GameplayScreen.Ruleset.ScoreProcessor, Md5, ConfigManager.Username.Value, ScrollSpeed);
                        await LocalScoreCache.InsertScoreIntoDatabase(localScore);         
                        Logger.LogSuccess($"Successfully saved local score to the database", LogType.Runtime);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"There was a fatal error when saving the local score!" + e.Message, LogType.Runtime);
                    }
                });

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
            
#if DEBUG
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
#endif
            });
            
            t.Start();
        }
    }
}