using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UserInterface;
using Quaver.Main;
using Quaver.States.Enums;
using Quaver.States.Gameplay;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Discord;
using Quaver.Graphics.Text;
using Quaver.Helpers;
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
            
            Console.WriteLine(GameplayScreen.Ruleset.ScoreProcessor.Stats.Count);
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
                Alignment = Alignment.BotLeft,
            };

            Back.Clicked += (o, e) =>
            {                      
                IsExitingScreen = true;
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundBack);  
                ApplauseSound.Stop();
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
            
            if (!GameplayScreen.Failed && GameplayScreen.Ruleset.ScoreProcessor.Accuracy >= 80)
                ApplauseSound.Play();
        }
    }
}