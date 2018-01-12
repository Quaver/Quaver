using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
using Quaver.Logging;
using Quaver.Skins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.States
{
    class OptionsMenuState : IGameState
    {
        //todo: documentation
        public State CurrentState { get; set; }
        public bool UpdateReady { get; set; }

        private Boundary Boundary { get; set; }
        private Boundary ButtonsContainer { get; set; }

        private TextButton BackButton { get; set; }
        private KeyBindButton[] ManiaKeys4K { get; set; }
        private KeyBindButton[] ManiaKeys7K { get; set; }
        private EventHandler[] ManiaEvent4K { get; set; }
        private EventHandler[] ManiaEvent7K { get; set; }

        private string[] AvailableSkins { get; set; }
        private List<TextButton> SkinSelectButtons { get; set; }
        private List<EventHandler> SkinSelectEvents { get; set; }

        private TextButton BackgroundBrightnessButton { get; set; }
        private TextButton FullscreenButton { get; set; }
        private TextButton LetterBoxingButton { get; set; }
        private List<TextButton> ResolutionButtons { get; set; }
        private List<EventHandler> ResolutionEvents { get; set; }
        private Point[] CommonResolutions { get; } = 
        {
            new Point(800, 600), new Point(1024, 768), new Point(1152, 864), new Point(1280, 960), new Point(1024, 600),
            new Point(1280, 720), new Point(1366, 768), new Point(1440, 900), new Point(1600, 900), new Point(1680, 1050),
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
            Logger.Log("Options Menu Button Clicked", LogColors.GameImportant, 5);

            Boundary = new Boundary();
            ButtonsContainer = new Boundary()
            {
                SizeY = 700,
                Alignment = Alignment.MidCenter,
                Parent = Boundary
            };

            CreateManiaKeyButtons();
            CreateSkinSelectButtons();
            CreateBackButton();
            CreateGraphicsOptionButtons();

            UpdateReady = true;
        }

        public void UnloadContent()
        {
            for (var i = 0; i < 10; i++)
                ResolutionButtons[i].Clicked -= ResolutionEvents[i];
            ResolutionEvents.Clear();

            BackButton.Clicked -= BackButtonClick;
            BackgroundBrightnessButton.Clicked -= OnBrightnessButtonClicked;
            FullscreenButton.Clicked -= OnFullscreenButtonClicked;
            LetterBoxingButton.Clicked -= OnLetterboxButtonClicked;

            for (var i = 0; i < 4; i++)
                ManiaKeys4K[i].KeyChanged -= ManiaEvent4K[i];
            for (var i = 0; i < 7; i++)
                ManiaKeys7K[i].KeyChanged -= ManiaEvent7K[i];

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

        /// <summary>
        ///     Create back button
        /// </summary>
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

        /// <summary>
        ///     Creates UI relating to Mania Keybinding
        /// </summary>
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
                Parent = ButtonsContainer
            };

            ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 20,
                PosY = 80,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Text = "Mania 4K",
                Parent = ButtonsContainer
            };

            ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 20,
                PosY = 150,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Text = "Mania 7K",
                Parent = ButtonsContainer
            };

            ManiaKeys4K = new KeyBindButton[4];
            ManiaEvent4K = new EventHandler[4];
            var keys = new Keys[4] { Configuration.KeyMania1, Configuration.KeyMania2, Configuration.KeyMania3, Configuration.KeyMania4 };
            for (var i=0; i<4; i++)
            {
                //todo: hook this to an event/method or something
                var index = i;
                ManiaKeys4K[i] = new KeyBindButton(new Vector2(100, 30), keys[i])
                {
                    PosY = 110,
                    PosX = (i - 1.5f) * 110f,
                    Alignment = Alignment.TopCenter,
                    Parent = ButtonsContainer
                };
                ManiaEvent4K[i] = (sender, e) => OnManiaKey4KPressed(sender, e, index);
                ManiaKeys4K[i].KeyChanged += ManiaEvent4K[i];
            }

            ManiaKeys7K = new KeyBindButton[7];
            ManiaEvent7K = new EventHandler[7];
            keys = new Keys[7] { Configuration.KeyMania7k1, Configuration.KeyMania7k2, Configuration.KeyMania7k3, Configuration.KeyMania7k4, Configuration.KeyMania7k5, Configuration.KeyMania7k6, Configuration.KeyMania7k7 };
            for (var i = 0; i < 7; i++)
            {
                //todo: hook this to an event/method or something
                var index = i;
                ManiaKeys7K[i] = new KeyBindButton(new Vector2(100, 30), keys[i])
                {
                    PosY = 180,
                    PosX = (i - 3f) * 110f,
                    Alignment = Alignment.TopCenter,
                    Parent = ButtonsContainer
                };
                ManiaEvent7K[i] = (sender, e) => OnManiaKey7KPressed(sender, e, index);
                ManiaKeys7K[i].KeyChanged += ManiaEvent7K[i];
            }
        }

        /// <summary>
        ///     Creates UI relating to Skins Options
        /// </summary>
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
                Parent = ButtonsContainer
            };

            SkinSelectButtons = new List<TextButton>();
            SkinSelectEvents = new List<EventHandler>();

            AvailableSkins = Directory.GetDirectories(Configuration.SkinDirectory);
            for (var i = 0; i < AvailableSkins.Length; i++)
                AvailableSkins[i] = new DirectoryInfo(AvailableSkins[i]).Name;

            var length = AvailableSkins.Length + 2;
            var median = length % 2 == 0 ? length / 2f - 0.5f : (float)Math.Floor(length / 2f);
            for (var i = 0; i < length; i++)
            {
                //todo: hook this to an event/method or something
                string skin;
                if (i < AvailableSkins.Length) skin = AvailableSkins[i];
                else if (i == AvailableSkins.Length) skin = "default bars";
                else skin = "default arrows";

                var button = new TextButton(new Vector2(150, 30), skin)
                {
                    PosY = 300,
                    PosX = (i - median) * 160f,
                    Alignment = Alignment.TopCenter,
                    Parent = ButtonsContainer
                };
                EventHandler newEvent = (sender, e) => OnSkinSelectButtonClicked(sender, e, skin);
                button.Clicked += newEvent;
                SkinSelectButtons.Add(button);
                SkinSelectEvents.Add(newEvent);
            }
        }

        /// <summary>
        ///     Create UI relating to Graphics Options
        /// </summary>
        private void CreateGraphicsOptionButtons()
        {
            var ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 70,
                PosY = 340,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Font = Fonts.Medium24,
                Text = "Graphics",
                Parent = ButtonsContainer
            };

            ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 20,
                PosY = 420,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Text = "Resolution",
                Parent = ButtonsContainer
            };

            ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 20,
                PosY = 530,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Text = "Other",
                Parent = ButtonsContainer
            };

            // Create Resolution Buttons
            TextButton button;
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
                    Parent = ButtonsContainer
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
                    Parent = ButtonsContainer
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
                Parent = ButtonsContainer
            };
            LetterBoxingButton.Clicked += OnLetterboxButtonClicked;

            // Create Fullscreen Option Button
            FullscreenButton = new TextButton(new Vector2(200, 30), $@"FullScreen: {Configuration.WindowFullScreen}")
            {
                PosY = 560,
                //PosX = (0.5f) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };
            FullscreenButton.Clicked += OnFullscreenButtonClicked;

            // Create Background Brightness Button
            BackgroundBrightnessButton = new TextButton(new Vector2(200, 30), $@"BG Brightness: {Configuration.BackgroundBrightness}%")
            {
                PosY = 560,
                PosX = (1) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };
            BackgroundBrightnessButton.Clicked += OnBrightnessButtonClicked;
        }

        /// <summary>
        ///     When a mania 4K keybinding gets updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="index"></param>
        private void OnManiaKey4KPressed(object sender, EventArgs e, int index)
        {
            switch (index)
            {
                case 0:
                    Configuration.KeyMania1 = ManiaKeys4K[index].CurrentKey;
                    break;
                case 1:
                    Configuration.KeyMania2 = ManiaKeys4K[index].CurrentKey;
                    break;
                case 2:
                    Configuration.KeyMania3 = ManiaKeys4K[index].CurrentKey;
                    break;
                case 3:
                    Configuration.KeyMania4 = ManiaKeys4K[index].CurrentKey;
                    break;
            }
        }

        /// <summary>
        ///     When a mania 7K keybinding gets updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="index"></param>
        private void OnManiaKey7KPressed(object sender, EventArgs e, int index)
        {
            switch (index)
            {
                case 0:
                    Configuration.KeyMania7k1 = ManiaKeys7K[index].CurrentKey;
                    break;
                case 1:
                    Configuration.KeyMania7k2 = ManiaKeys7K[index].CurrentKey;
                    break;
                case 2:
                    Configuration.KeyMania7k3 = ManiaKeys7K[index].CurrentKey;
                    break;
                case 3:
                    Configuration.KeyMania7k4 = ManiaKeys7K[index].CurrentKey;
                    break;
                case 4:
                    Configuration.KeyMania7k5 = ManiaKeys7K[index].CurrentKey;
                    break;
                case 5:
                    Configuration.KeyMania7k6 = ManiaKeys7K[index].CurrentKey;
                    break;
                case 6:
                    Configuration.KeyMania7k7 = ManiaKeys7K[index].CurrentKey;
                    break;
            }
        }

        /// <summary>
        ///     Gets called when brightness button gets clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            BackgroundManager.Blacken();
            BackgroundManager.Readjust();
        }

        /// <summary>
        ///     Gets called everytime fullscreen button gets clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        ///     Gets called everytime letterbox button gets clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        ///     Gets called when a resolution option button gets clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="index"></param>
        private void OnResolutionButtonClicked(object sender, EventArgs e, int index)
        {
            GameBase.ChangeWindow(Configuration.WindowFullScreen, Configuration.WindowLetterboxed, CommonResolutions[index]);
            BackgroundManager.Readjust();
            Boundary.SizeX = GameBase.WindowRectangle.Width;
            Boundary.SizeY = GameBase.WindowRectangle.Height;
        }

        /// <summary>
        ///     Loads a skin after a button triggers this method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="skin"></param>
        private void OnSkinSelectButtonClicked(object sender, EventArgs e, string skin)
        {
            if (skin == "default arrows")
            {
                Configuration.Skin = "";
                Configuration.DefaultSkin = DefaultSkins.Arrow;
            }
            else if (skin == "default bars")
            {
                Configuration.Skin = "";
                Configuration.DefaultSkin = DefaultSkins.Bar;
            }
            else
                Configuration.Skin = skin;

            Skin.LoadSkin();
        }
    }
}
