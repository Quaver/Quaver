using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
using Quaver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.States
{
    class OptionsMenuState : IGameState
    {
        public State CurrentState { get; set; }
        public bool UpdateReady { get; set; }

        private Boundary Boundary { get; set; }
        private TextButton BackButton { get; set; }
        private TextButton[] ManiaKeys4K { get; set; }
        private TextButton[] ManiaKeys7K { get; set; }
        private List<TextButton> SkinSelectButton { get; set; }
        private TextButton BackgroundBrightnessButton { get; set; }
        private TextButton FullscreenButton { get; set; }
        private TextButton LetterBoxingButton { get; set; }
        private List<TextButton> ResolutionButtons { get; set; }
        private List<EventHandler> ResolutionEvents { get; set; }
        private Point[] CommonResolutions { get; } = new Point[10]
        {
            new Point(800, 600), new Point(1024, 768), new Point(1152, 864), new Point(1280, 960), new Point(1280, 1024),
            new Point(1024, 600), new Point(1280, 720), new Point(1366, 768), new Point(1440, 900), new Point(1680, 1050)
        };


        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            BackgroundManager.Draw();
            Boundary.Draw();
            GameBase.SpriteBatch.End();
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
            Logger.Log("Options Menu Button Clicked", LogColors.GameImportant, 5);
            Boundary = new Boundary();
            CreateManiaKeyButtons();
            CreateSkinSelectButtons();
            CreateBackButton();
            CreateGraphicsOptionButtons();

            UpdateReady = true;
        }

        public void UnloadContent()
        {
            for (var i=0; i<10; i++)
                ResolutionButtons[i].Clicked -= ResolutionEvents[i];
            ResolutionEvents.Clear();

            BackButton.Clicked -= BackButtonClick;
            BackgroundBrightnessButton.Clicked -= OnBrightnessButtonClicked;
            FullscreenButton.Clicked -= OnFullscreenButtonClicked;
            LetterBoxingButton.Clicked -= OnLetterboxButtonClicked;

            Boundary.Destroy();
        }

        public void Update(double dt)
        {
            Boundary.Update(dt);
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            GameBase.GameStateManager.ChangeState(new MainMenuState());
        }

        private void CreateBackButton()
        {
            //Todo: Remove. TEST.
            BackButton = new TextButton(new Vector2(200, 50), "BACK")
            {
                Alignment = Alignment.BotCenter,
                Parent = Boundary
            };
            BackButton.Clicked += BackButtonClick;
        }

        private void CreateManiaKeyButtons()
        {
            var ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 70,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Font = Fonts.Medium24,
                Text = "Key Bindings",
                Parent = Boundary
            };

            ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 20,
                PosY = 80,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Text = "Mania 4K",
                Parent = Boundary
            };

            ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 20,
                PosY = 150,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Text = "Mania 7K",
                Parent = Boundary
            };

            ManiaKeys4K = new TextButton[4];
            var keys = new Keys[4] { Configuration.KeyMania1, Configuration.KeyMania2, Configuration.KeyMania3, Configuration.KeyMania4 };
            for (var i=0; i<4; i++)
            {
                //todo: hook this to an event/method or something
                ManiaKeys4K[i] = new TextButton(new Vector2(100, 30), keys[i].ToString())
                {
                    PosY = 110,
                    PosX = (i - 1.5f) * 110f,
                    Alignment = Alignment.TopCenter,
                    Parent = Boundary
                };
            }

            ManiaKeys7K = new TextButton[7];
            keys = new Keys[7] { Configuration.KeyMania7k1, Configuration.KeyMania7k2, Configuration.KeyMania7k3, Configuration.KeyMania7k4, Configuration.KeyMania7k5, Configuration.KeyMania7k6, Configuration.KeyMania7k7 };
            for (var i = 0; i < 7; i++)
            {
                //todo: hook this to an event/method or something
                ManiaKeys7K[i] = new TextButton(new Vector2(100, 30), keys[i].ToString())
                {
                    PosY = 180,
                    PosX = (i - 3f) * 110f,
                    Alignment = Alignment.TopCenter,
                    Parent = Boundary
                };
            }
        }

        private void CreateSkinSelectButtons()
        {
            var ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 70,
                PosY = 220,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Font = Fonts.Medium24,
                Text = "Select Skin",
                Parent = Boundary
            };

            SkinSelectButton = new List<TextButton>();
            var skins = new String[4] { "skin 1", "skin 2", "skin 3", "skin 4" }; //todo: this is placeholder until i get back
            for (var i = 0; i < 4; i++)
            {
                //todo: hook this to an event/method or something
                var skin = new TextButton(new Vector2(150, 30), skins[i])
                {
                    PosY = 300,
                    PosX = (i - 1.5f) * 160f,
                    Alignment = Alignment.TopCenter,
                    Parent = Boundary
                };
                SkinSelectButton.Add(skin);
            }
        }

        private void CreateGraphicsOptionButtons()
        {
            TextButton button;
            var ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 70,
                PosY = 340,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Font = Fonts.Medium24,
                Text = "Graphics",
                Parent = Boundary
            };

            ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 20,
                PosY = 420,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Text = "Resolution",
                Parent = Boundary
            };

            ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 20,
                PosY = 530,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Text = "Other",
                Parent = Boundary
            };

            // Create Resolution Buttons
            ResolutionButtons = new List<TextButton>();
            ResolutionEvents = new List<EventHandler>();
            for (var i = 0; i < 5; i++)
            {
                //todo: hook this to an event/method or something
                var index = i;
                button = new TextButton(new Vector2(150, 30), $@"{CommonResolutions[i].X}x{CommonResolutions[i].Y}")
                {
                    PosY = 450,
                    PosX = (i - 2f) * 160f,
                    Alignment = Alignment.TopCenter,
                    Parent = Boundary
                };
                EventHandler newEvent = (sender, e) => OnResolutionButtonClicked(sender, e, index);
                button.Clicked += newEvent;
                ResolutionButtons.Add(button);
                ResolutionEvents.Add(newEvent);
            }
            for (var i = 5; i < 10; i++)
            {
                //todo: hook this to an event/method or something
                var index = i;
                button = new TextButton(new Vector2(150, 30), $@"{CommonResolutions[i].X}x{CommonResolutions[i].Y}")
                {
                    PosY = 490,
                    PosX = (i - 7f) * 160f,
                    Alignment = Alignment.TopCenter,
                    Parent = Boundary
                };
                EventHandler newEvent = (sender, e) => OnResolutionButtonClicked(sender, e, index);
                button.Clicked += newEvent;
                ResolutionButtons.Add(button);
                ResolutionEvents.Add(newEvent);
            }

            // Create Letterbox Option Button
            LetterBoxingButton = new TextButton(new Vector2(200, 30), $@"Letterboxing: {Configuration.WindowLetterboxed}")
            {
                PosY = 560,
                PosX = (-1) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = Boundary
            };
            LetterBoxingButton.Clicked += OnLetterboxButtonClicked;

            // Create Fullscreen Option Button
            FullscreenButton = new TextButton(new Vector2(200, 30), $@"FullScreen: {Configuration.WindowFullScreen}")
            {
                PosY = 560,
                //PosX = (0.5f) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = Boundary
            };
            FullscreenButton.Clicked += OnFullscreenButtonClicked;

            // Create Background Brightness Button
            BackgroundBrightnessButton = new TextButton(new Vector2(200, 30), $@"BG Brightness: {Configuration.BackgroundBrightness}%")
            {
                PosY = 560,
                PosX = (1) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = Boundary
            };
            BackgroundBrightnessButton.Clicked += OnBrightnessButtonClicked;
        }

        private void OnBrightnessButtonClicked(object sender, EventArgs e)
        {
            var brightness = Configuration.BackgroundBrightness;
            switch (brightness)
            {
                case 0:
                    Configuration.BackgroundBrightness = 5;
                    break;
                case 5:
                    Configuration.BackgroundBrightness = 10;
                    break;
                case 10:
                    Configuration.BackgroundBrightness = 20;
                    break;
                case 20:
                    Configuration.BackgroundBrightness = 40;
                    break;
                case 40:
                    Configuration.BackgroundBrightness = 60;
                    break;
                case 60:
                    Configuration.BackgroundBrightness = 80;
                    break;
                case 80:
                    Configuration.BackgroundBrightness = 100;
                    break;
                default:
                    Configuration.BackgroundBrightness = 0;
                    break;
            }
            BackgroundBrightnessButton.TextSprite.Text = $@"BG Brightness: {Configuration.BackgroundBrightness}%";
            BackgroundManager.Readjust();
        }

        private void OnFullscreenButtonClicked(object sender, EventArgs e)
        {
            var fullscreen = Configuration.WindowFullScreen;
            switch (fullscreen)
            {
                case true:
                    Configuration.WindowFullScreen = false;
                    break;
                case false:
                    Configuration.WindowFullScreen = true;
                    break;
            }
            FullscreenButton.TextSprite.Text = $@"FullScreen: {Configuration.WindowFullScreen}";
            GameBase.ChangeWindow(Configuration.WindowFullScreen, Configuration.WindowLetterboxed);
            BackgroundManager.Readjust();
        }

        private void OnLetterboxButtonClicked(object sender, EventArgs e)
        {
            var letterboxed = Configuration.WindowLetterboxed;
            switch (letterboxed)
            {
                case true:
                    Configuration.WindowLetterboxed = false;
                    break;
                case false:
                    Configuration.WindowLetterboxed = true;
                    break;
            }
            LetterBoxingButton.TextSprite.Text = $@"Letterboxing: {Configuration.WindowLetterboxed}";
            GameBase.ChangeWindow(Configuration.WindowFullScreen, Configuration.WindowLetterboxed);
            BackgroundManager.Readjust();
        }

        private void OnResolutionButtonClicked(object sender, EventArgs e, int index)
        {
            GameBase.ChangeWindow(Configuration.WindowFullScreen, Configuration.WindowLetterboxed, CommonResolutions[index]);
            BackgroundManager.Readjust();
            Boundary.SizeX = GameBase.WindowRectangle.Width;
            Boundary.SizeY = GameBase.WindowRectangle.Height;
        }
    }
}
