using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Graphics.Text;
using Quaver.Logging;
using Quaver.Skins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UserInterface;

namespace Quaver.GameState.States
{
    class OptionsMenuState : IGameState
    {
        //todo: documentation
        public State CurrentState { get; set; }
        public bool UpdateReady { get; set; }

        private Boundary Boundary { get; set; }
        private Boundary ButtonsContainer { get; set; }

        // Key Bindings
        private QuaverTextButton BackButton { get; set; }
        private QuaverKeybindButton[] ManiaKeys4K { get; set; }
        private QuaverKeybindButton[] ManiaKeys7K { get; set; }
        private EventHandler[] ManiaEvent4K { get; set; }
        private EventHandler[] ManiaEvent7K { get; set; }

        // Skin Options
        private string[] AvailableSkins { get; set; }
        private List<QuaverTextButton> SkinSelectButtons { get; set; }
        private List<EventHandler> SkinSelectEvents { get; set; }

        // Graphics Options
        private QuaverTextButton BackgroundBrightnessButton { get; set; }
        private QuaverTextButton FullscreenButton { get; set; }
        private QuaverTextButton LetterBoxingButton { get; set; }
        private List<QuaverTextButton> ResolutionButtons { get; set; }
        private List<EventHandler> ResolutionEvents { get; set; }
        private Point[] CommonResolutions { get; } = 
        {
            new Point(800, 600), new Point(1024, 768), new Point(1152, 864), new Point(1280, 960), new Point(1024, 600),
            new Point(1280, 720), new Point(1366, 768), new Point(1440, 900), new Point(1600, 900), new Point(1680, 1050),
        };

        // Gameplay Options
        private QuaverTextButton ScrollSpeedButton { get; set; }
        private QuaverTextButton ScrollDirection4KButton { get; set; }
        private QuaverTextButton ScrollDirection7KButton { get; set; }
        private QuaverTextButton ShowAccuracyUiButton { get; set; }
        private QuaverTextButton ShowPlayfieldUiButton { get; set; }
        private QuaverTextButton ShowNoteColoringButton { get; set; }
        private QuaverTextButton LaneSize4KButton { get; set; }
        private QuaverTextButton LaneSize7KButton { get; set; }

        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            //BackgroundManager.Draw();
            Boundary.Draw();
            GameBase.SpriteBatch.End();
        }

        public void Initialize()
        {
            Boundary = new Boundary();
            ButtonsContainer = new Boundary()
            {
                SizeY = 850,
                Alignment = Alignment.TopCenter,
                Parent = Boundary
            };

            CreateManiaKeyButtons();
            CreateSkinSelectButtons();
            CreateBackButton();
            CreateGraphicsOptionButtons();
            CreateGameplayOptionsButtons();

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
            ButtonsContainer.PosY = -(GameBase.MouseState.Position.Y / GameBase.WindowRectangle.Height) * Math.Max(ButtonsContainer.SizeY - GameBase.WindowRectangle.Height, 0);
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
            BackButton = new QuaverTextButton(new Vector2(200, 50), "BACK")
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

            ManiaKeys4K = new QuaverKeybindButton[4];
            ManiaEvent4K = new EventHandler[4];
            var keys = new Keys[4] { ConfigManager.KeyMania4k1, ConfigManager.KeyMania4k2, ConfigManager.KeyMania4k3, ConfigManager.KeyMania4k4 };
            for (var i=0; i<4; i++)
            {
                //todo: hook this to an event/method or something
                var index = i;
                ManiaKeys4K[i] = new QuaverKeybindButton(new Vector2(100, 30), keys[i])
                {
                    PosY = 110,
                    PosX = (i - 1.5f) * 110f,
                    Alignment = Alignment.TopCenter,
                    Parent = ButtonsContainer
                };
                ManiaEvent4K[i] = (sender, e) => OnManiaKey4KPressed(sender, e, index);
                ManiaKeys4K[i].KeyChanged += ManiaEvent4K[i];
            }

            ManiaKeys7K = new QuaverKeybindButton[7];
            ManiaEvent7K = new EventHandler[7];
            keys = new Keys[7] { ConfigManager.KeyMania7k1, ConfigManager.KeyMania7k2, ConfigManager.KeyMania7k3, ConfigManager.KeyMania7k4, ConfigManager.KeyMania7k5, ConfigManager.KeyMania7k6, ConfigManager.KeyMania7k7 };
            for (var i = 0; i < 7; i++)
            {
                //todo: hook this to an event/method or something
                var index = i;
                ManiaKeys7K[i] = new QuaverKeybindButton(new Vector2(100, 30), keys[i])
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

            SkinSelectButtons = new List<QuaverTextButton>();
            SkinSelectEvents = new List<EventHandler>();

            AvailableSkins = Directory.GetDirectories(ConfigManager.SkinDirectory);
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

                var button = new QuaverTextButton(new Vector2(150, 30), skin)
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
            QuaverTextButton button;
            ResolutionButtons = new List<QuaverTextButton>();
            ResolutionEvents = new List<EventHandler>();
            for (var i = 0; i < 5; i++)
            {
                //todo: hook this to an event/method or something
                var index = i;
                button = new QuaverTextButton(new Vector2(150, 30), $@"{CommonResolutions[i].X}x{CommonResolutions[i].Y}")
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
                button = new QuaverTextButton(new Vector2(150, 30), $@"{CommonResolutions[i].X}x{CommonResolutions[i].Y}")
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

            // Create Letterbox Option QuaverButton
            LetterBoxingButton = new QuaverTextButton(new Vector2(200, 30), $@"Letterboxing: {ConfigManager.WindowLetterboxed}")
            {
                PosY = 560,
                PosX = (-1) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };
            LetterBoxingButton.Clicked += OnLetterboxButtonClicked;

            // Create Fullscreen Option QuaverButton
            FullscreenButton = new QuaverTextButton(new Vector2(200, 30), $@"FullScreen: {ConfigManager.WindowFullScreen}")
            {
                PosY = 560,
                //PosX = (0.5f) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };
            FullscreenButton.Clicked += OnFullscreenButtonClicked;

            // Create Background Brightness QuaverButton
            BackgroundBrightnessButton = new QuaverTextButton(new Vector2(200, 30), $@"BG Brightness: {ConfigManager.BackgroundBrightness}%")
            {
                PosY = 560,
                PosX = (1) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };
            BackgroundBrightnessButton.Clicked += OnBrightnessButtonClicked;
        }

        private void CreateGameplayOptionsButtons()
        {
            // text info
            var ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 70,
                PosY = 600,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Font = Fonts.Medium24,
                Text = "Gameplay",
                Parent = ButtonsContainer
            };

            // scroll speed 
            ScrollSpeedButton = new QuaverTextButton(new Vector2(200, 30), $@"ScrollSpeed: {ConfigManager.ScrollSpeed4k}")
            {
                PosY = 680,
                PosX = (-2.5f) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };

            // scroll direction
            ScrollDirection4KButton = new QuaverTextButton(new Vector2(200, 30), $@"Downscroll 4K: {ConfigManager.DownScroll4k}")
            {
                PosY = 680,
                PosX = (-1.5f) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };

            ScrollDirection7KButton = new QuaverTextButton(new Vector2(200, 30), $@"Downscroll 7K: {ConfigManager.DownScroll7k}")
            {
                PosY = 680,
                PosX = (-0.5f) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };

            // note coloring
            ShowNoteColoringButton = new QuaverTextButton(new Vector2(200, 30), $@"Note snap coloring: {true}")
            {
                PosY = 680,
                PosX = (0.5f) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };

            // receptor sizes
            LaneSize4KButton = new QuaverTextButton(new Vector2(200, 30), $@"Lane Size 4K: {GameBase.LoadedSkin.ColumnSize4K}")
            {
                PosY = 680,
                PosX = (1.5f) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };

            LaneSize7KButton = new QuaverTextButton(new Vector2(200, 30), $@"Lane Size 7K: {GameBase.LoadedSkin.ColumnSize7K}")
            {
                PosY = 680,
                PosX = (2.5f) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };

            // show accuracy box
            ShowAccuracyUiButton = new QuaverTextButton(new Vector2(200, 30), $@"Show Accuracy Box: {true}")
            {
                PosY = 720,
                PosX = (-0.5f) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };

            // show playfield overlay
            ShowPlayfieldUiButton = new QuaverTextButton(new Vector2(200, 30), $@"Show Playfield Overlay: {true}")
            {
                PosY = 720,
                PosX = (0.5f) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = ButtonsContainer
            };
        }

        private void OnScrollSpeedButtonClick(object sender, EventArgs e)
        {

        }

        private void OnScrollDirection4KButtonClick(object sender, EventArgs e)
        {

        }

        private void OnScrollDirection7KButtonClick(object sender, EventArgs e)
        {

        }

        private void OnShowAccuracyUIButtonClick(object sender, EventArgs e)
        {

        }

        private void OnShowPlayfieldUIButtonClick(object sender, EventArgs e)
        {

        }

        private void OnNoteColorButtonClick(object sender, EventArgs e)
        {

        }

        private void OnLaneSize4KButtonClick(object sender, EventArgs e)
        {

        }

        private void OnLaneSize7KButtonClick(object sender, EventArgs e)
        {

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
                    ConfigManager.KeyMania4k1 = ManiaKeys4K[index].CurrentKey;
                    break;
                case 1:
                    ConfigManager.KeyMania4k2 = ManiaKeys4K[index].CurrentKey;
                    break;
                case 2:
                    ConfigManager.KeyMania4k3 = ManiaKeys4K[index].CurrentKey;
                    break;
                case 3:
                    ConfigManager.KeyMania4k4 = ManiaKeys4K[index].CurrentKey;
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
                    ConfigManager.KeyMania7k1 = ManiaKeys7K[index].CurrentKey;
                    break;
                case 1:
                    ConfigManager.KeyMania7k2 = ManiaKeys7K[index].CurrentKey;
                    break;
                case 2:
                    ConfigManager.KeyMania7k3 = ManiaKeys7K[index].CurrentKey;
                    break;
                case 3:
                    ConfigManager.KeyMania7k4 = ManiaKeys7K[index].CurrentKey;
                    break;
                case 4:
                    ConfigManager.KeyMania7k5 = ManiaKeys7K[index].CurrentKey;
                    break;
                case 5:
                    ConfigManager.KeyMania7k6 = ManiaKeys7K[index].CurrentKey;
                    break;
                case 6:
                    ConfigManager.KeyMania7k7 = ManiaKeys7K[index].CurrentKey;
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
            var brightness = ConfigManager.BackgroundBrightness;
            switch (brightness)
            {
                case 0:
                    ConfigManager.BackgroundBrightness = 5;
                    break;
                case 5:
                    ConfigManager.BackgroundBrightness = 10;
                    break;
                case 10:
                    ConfigManager.BackgroundBrightness = 20;
                    break;
                case 20:
                    ConfigManager.BackgroundBrightness = 40;
                    break;
                case 40:
                    ConfigManager.BackgroundBrightness = 60;
                    break;
                case 60:
                    ConfigManager.BackgroundBrightness = 80;
                    break;
                case 80:
                    ConfigManager.BackgroundBrightness = 100;
                    break;
                default:
                    ConfigManager.BackgroundBrightness = 0;
                    break;
            }
            BackgroundBrightnessButton.TextSprite.Text = $@"BG Brightness: {ConfigManager.BackgroundBrightness}%";
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
            var fullscreen = ConfigManager.WindowFullScreen;
            switch (fullscreen)
            {
                case true:
                    ConfigManager.WindowFullScreen = false;
                    break;
                case false:
                    ConfigManager.WindowFullScreen = true;
                    break;
            }
            FullscreenButton.TextSprite.Text = $@"FullScreen: {ConfigManager.WindowFullScreen}";
            GameBase.ChangeWindow(ConfigManager.WindowFullScreen, ConfigManager.WindowLetterboxed);
            BackgroundManager.Readjust();
        }

        /// <summary>
        ///     Gets called everytime letterbox button gets clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLetterboxButtonClicked(object sender, EventArgs e)
        {
            var letterboxed = ConfigManager.WindowLetterboxed;
            switch (letterboxed)
            {
                case true:
                    ConfigManager.WindowLetterboxed = false;
                    break;
                case false:
                    ConfigManager.WindowLetterboxed = true;
                    break;
            }
            LetterBoxingButton.TextSprite.Text = $@"Letterboxing: {ConfigManager.WindowLetterboxed}";
            GameBase.ChangeWindow(ConfigManager.WindowFullScreen, ConfigManager.WindowLetterboxed);
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
            GameBase.ChangeWindow(ConfigManager.WindowFullScreen, ConfigManager.WindowLetterboxed, CommonResolutions[index]);
            BackgroundManager.Readjust();
            Boundary.SizeX = GameBase.WindowRectangle.Width;
            Boundary.SizeY = GameBase.WindowRectangle.Height;
            GameBase.GameOverlay.RecalculateWindow();
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
                ConfigManager.Skin = "";
                ConfigManager.DefaultSkin = DefaultSkins.Arrow;
            }
            else if (skin == "default bars")
            {
                ConfigManager.Skin = "";
                ConfigManager.DefaultSkin = DefaultSkins.Bar;
            }
            else
                ConfigManager.Skin = skin;

            Skin.LoadSkin();
        }
    }
}
