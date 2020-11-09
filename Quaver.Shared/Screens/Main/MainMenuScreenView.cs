using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Main.UI;
using Quaver.Shared.Screens.Main.UI.Nagivation;
using Quaver.Shared.Screens.Main.UI.News;
using Quaver.Shared.Screens.Main.UI.Tips;
using Quaver.Shared.Screens.Main.UI.Visualizer;
using Quaver.Shared.Screens.Menu.UI.Visualizer;
using Quaver.Shared.Screens.Music;
using Quaver.Shared.Screens.Options;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Main
{
    public class MainMenuScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private Sprite MenuLogoBackground { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Logo { get; set; }

        /// <summary>
        /// </summary>
        private MenuTipContainer TipsContainer { get; set; }

        /// <summary>
        /// </summary>
        private NavigationButtonContainer NavigationButtonContainer { get; set; }

        /// <summary>
        /// </summary>
        private NewsPost News { get; set; }

        /// <summary>
        ///     The amount of padding from the top of the screen where top components are positioned
        /// </summary>
        private const int PADDING_TOP_Y = 184;

        /// <summary>
        ///     The amount of padding from the left of the screen
        /// </summary>
        private const int PADDING_X = 70;

        /// <summary>
        ///     The amount of padding from the bottom of the screen
        /// </summary>
        private const int PADDING_BOTTOM_Y = 90;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MainMenuScreenView(QuaverScreen screen) : base(screen)
        {
            CreateBackground();
            CreateMenuLogoBackground();
            CreateLogo();
            CreateNoteVisualizer();
            CreateAudioVisualizer();
            CreateMenuTip();
            CreateNavigationButtons();
            CreateNewsPost();
            CreateHeader();
            CreateFooter();

            screen.ScreenExiting += OnScreenExiting;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2F2F2F"));
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///     Creates <see cref="Background"/>
        /// </summary>
        private void CreateBackground()
        {
            Background = new BackgroundImage(SkinManager.Skin?.MainMenu?.Background ?? UserInterface.TrianglesWallpaper, 0,
                false) {Parent = Container};
        }

        /// <summary>
        ///     Creates <see cref="MenuLogoBackground"/>
        /// </summary>
        private void CreateMenuLogoBackground()
        {
            var tex = UserInterface.MenuLogoBackground;

            MenuLogoBackground = new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(tex.Width * 0.80f, tex.Height * 0.80f),
                Y = PADDING_TOP_Y,
                Image = tex
            };

            MenuLogoBackground.X = -MenuLogoBackground.Width - 50;
            MenuLogoBackground.MoveToX(0, Easing.OutQuint, 450);
        }

        /// <summary>
        ///    Creates <see cref="Logo"/>
        /// </summary>
        private void CreateLogo()
        {
            var tex = UserInterface.MenuLogo;

            Logo = new Sprite
            {
                Parent = MenuLogoBackground,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(tex.Width * 0.80f, tex.Height * 0.80f),
                X = PADDING_X,
                Image = tex
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMenuTip()
        {
            TipsContainer = new MenuTipContainer()
            {
                Parent = Container,
                Alignment = Alignment.BotRight,
                Position = new ScalableVector2(-PADDING_X, -PADDING_BOTTOM_Y)
            };

            TipsContainer.X = TipsContainer.Width + 50;
            TipsContainer.MoveToX(-PADDING_X, Easing.OutQuint, 450);
        }

        /// <summary>
        /// </summary>
        private void CreateNavigationButtons()
        {
            var quitColor = ColorHelper.HexToColor("#F9645D");
            var screen = Screen as MainMenuScreen;

            NavigationButtonContainer = new NavigationButtonContainer(new List<NavigationButton>()
            {
                new NavigationButton(FontAwesome.Get(FontAwesomeIcon.fa_gamepad_console), "Single Player",
                    (o, e) => screen?.ExitToSinglePlayer()),
                new NavigationButton(FontAwesome.Get(FontAwesomeIcon.fa_group_profile_users), "Multiplayer",
                    (o, e) => screen?.ExitToMultiplayer()),
                new NavigationButton(FontAwesome.Get(FontAwesomeIcon.fa_pencil), "Editor",
                    (o, e) => screen?.ExitToEditor()),
                new NavigationButton(FontAwesome.Get(FontAwesomeIcon.fa_download_to_storage_drive), "Download Songs",
                    (o, e) => screen?.ExitToDownload()),
                new NavigationButton(FontAwesome.Get(FontAwesomeIcon.fa_open_wrench_tool_silhouette), "Steam Workshop",
                    (sender, args) => BrowserHelper.OpenURL($"https://steamcommunity.com/app/{SteamManager.ApplicationId}/workshop/")),
                new NavigationButton(FontAwesome.Get(FontAwesomeIcon.fa_settings), "Options",
                    (o, e) => DialogManager.Show(new OptionsDialog())),
                new NavigationButton(FontAwesome.Get(FontAwesomeIcon.fa_power_button_off), "Quit Game",
                    (o, e) => DialogManager.Show(new QuitDialog()))
                {
                    Icon = { Tint = SkinManager.Skin?.MainMenu?.NavigationQuitButtonTextColor ?? quitColor },
                    Name = { Tint = SkinManager.Skin?.MainMenu?.NavigationQuitButtonTextColor ?? quitColor }
                }
            })
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                X = PADDING_X,
                Y = 144
            };
        }

        /// <summary>
        /// </summary>
        private void CreateNewsPost()
        {
            News = new NewsPost
            {
                Parent = Container,
                Alignment = Alignment.BotRight,
                X = -PADDING_X,
                Y = TipsContainer.Y - TipsContainer.Height - 26
            };

            News.X = News.Width + 50;
            News.MoveToX(-PADDING_X, Easing.OutQuint, 450);
        }

        /// <summary>
        /// </summary>
        private void CreateHeader() => new MenuHeaderMain { Parent = Container };

        /// <summary>
        /// </summary>
        private void CreateFooter() => new MainMenuFooter
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        /// </summary>
        private void CreateNoteVisualizer()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new NoteVisualizer
            {
                Parent = Container,
                Alignment = Alignment.TopRight
            };

            // ReSharper disable once ObjectCreationAsStatement
            /*new NoteVisualizer
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                X = -WindowManager.Width / 2f - 120
            };*/
        }

        /// <summary>
        /// </summary>
        private void CreateAudioVisualizer()
        {
            var visBottom = new MenuAudioVisualizer((int) WindowManager.Width, 750, 220, 3, 8)
            {
                Parent = Container,
                Y = -MenuBorder.HEIGHT,
                Alignment = Alignment.BotRight,
            };

            visBottom.Bars.ForEach(bar =>
            {
                bar.Alignment = Alignment.BotRight;
                bar.X = -bar.X;
                bar.Alpha =  SkinManager.Skin?.MainMenu?.AudioVisualizerOpacity ?? 0.85f;
                bar.Tint = SkinManager.Skin?.MainMenu?.AudioVisualizerColor ?? Colors.MainBlue;
            });
        }

        /// <summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// </summary>
        private void OnScreenExiting(object sender, ScreenExitingEventArgs e)
        {
            NavigationButtonContainer.Exit();

            const int animTime = 450;

            MenuLogoBackground.MoveToX(-MenuLogoBackground.Width - 50, Easing.OutQuint, animTime);

            TipsContainer.MoveToX(TipsContainer.Width + 50, Easing.OutQuint, animTime);
            News.MoveToX(News.Width + 50, Easing.OutQuint, animTime);
        }
    }
}