using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Lobby.UI;
using Quaver.Shared.Screens.Lobby.UI.Dialogs;
using Quaver.Shared.Screens.Lobby.UI.Dialogs.Create;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Menu.UI.Visualizer;
using Quaver.Shared.Screens.Settings;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Lobby
{
    public class LobbyScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private LobbyScreen LobbyScreen { get; }

        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private MenuHeader Header { get; set; }

        /// <summary>
        /// </summary>
        private MenuFooter Footer { get; set; }

        /// <summary>
        /// </summary>
        public LobbySearchBox Searchbox { get; private set; }

        /// <summary>
        /// </summary>
        public LobbyMatchScrollContainer MatchContainer { get; private set; }

        /// <summary>
        /// </summary>
        private LobbyMatchInfo MatchInfo { get; set; }

        /// <summary>
        /// </summary>
        private Jukebox Jukebox { get; }

        /// <summary>
        /// </summary>
        private MenuAudioVisualizer Visualizer { get; }

        /// <summary>
        /// </summary>
        private Sprite FilterBackground { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public LobbyScreenView(LobbyScreen screen) : base(screen)
        {
            LobbyScreen = screen;

            CreateBackground();
            CreateHeader();
            CreateFooter();

            Visualizer = new MenuAudioVisualizer((int) WindowManager.Width, 400, 150, 5)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
                Y = -Footer.Height
            };

            Visualizer.Bars.ForEach(x =>
            {
                x.Alpha = 0.30f;
            });

            CreateSearchBox();
            CreateMatchContainer();

            Jukebox = new Jukebox(true);

            FilterBackground = new Sprite()
            {
                Parent = Container,
                Position = new ScalableVector2(-Searchbox.X, Searchbox.Y),
                Size = new ScalableVector2(762, Searchbox.Height),
                Tint = Color.Black,
                Alpha = 0.75f
            };

            FilterBackground.AddBorder(Color.White, 2);

            const float spacing = 60f;

            var locked = new LabelledCheckbox("Display Locked", ConfigManager.LobbyFilterHasPassword)
            {
                Parent = FilterBackground,
                Alignment = Alignment.MidLeft,
                X = 14
            };

            var full = new LabelledCheckbox("Display Full", ConfigManager.LobbyFilterFullGame)
            {
                Parent = FilterBackground,
                Alignment = Alignment.MidLeft,
                X = locked.X + locked.Width + spacing
            };

            var owned = new LabelledCheckbox("Map Downloaded", ConfigManager.LobbyFilterOwnsMap)
            {
                Parent = FilterBackground,
                Alignment = Alignment.MidLeft,
                X = full.X + full.Width + spacing
            };

            var friends = new LabelledCheckbox("Friends In Game", ConfigManager.LobbyFilterHasFriends)
            {
                Parent = FilterBackground,
                Alignment = Alignment.MidLeft,
                X = owned.X + owned.Width + spacing
            };

            ConfigManager.LobbyFilterHasPassword.ValueChanged += RefilterGames;
            ConfigManager.LobbyFilterFullGame.ValueChanged += RefilterGames;
            ConfigManager.LobbyFilterOwnsMap.ValueChanged += RefilterGames;
            ConfigManager.LobbyFilterHasFriends.ValueChanged += RefilterGames;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefilterGames(object sender, BindableValueChangedEventArgs<bool> e) => MatchContainer.FilterGames(Searchbox.RawText, true);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);
            Jukebox.Update(gameTime);

            if (DialogManager.Dialogs.Count != 0)
                Searchbox.Focused = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            BackgroundHelper.Draw(gameTime);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable DelegateSubtraction
            ConfigManager.LobbyFilterHasPassword.ValueChanged -= RefilterGames;
            ConfigManager.LobbyFilterFullGame.ValueChanged -= RefilterGames;
            ConfigManager.LobbyFilterOwnsMap.ValueChanged -= RefilterGames;
            ConfigManager.LobbyFilterHasFriends.ValueChanged -= RefilterGames;

            Container?.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.MenuBackgroundRaw, 0, true)
        {
            Parent = Container
        };

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            Header = new MenuHeader(FontAwesome.Get(FontAwesomeIcon.fa_group_profile_users), "CUSTOM", "GAMES",
                "find or create a multiplayer match", Colors.MainAccent) { Parent = Container };

            Header.Y = -Header.Height;
            Header.MoveToY(0, Easing.OutQuint, 600);
        }

        /// <summary>
        /// </summary>
        private void CreateFooter()
        {
            Footer = new MenuFooter(new List<ButtonText>
            {
                new ButtonText(FontsBitmap.GothamRegular, "back to menu", 14, (o, e) => LobbyScreen.ExitToMenu()),
                new ButtonText(FontsBitmap.GothamRegular, "options menu", 14, (o, e) => DialogManager.Show(new SettingsDialog()))
            }, new List<ButtonText>
            {
                new ButtonText(FontsBitmap.GothamRegular, "quick match", 14, (o, e) => { }),
                new ButtonText(FontsBitmap.GothamRegular, "create match", 14, (o, e) => DialogManager.Show(new CreateGameDialog())),
            }, Colors.MainAccent)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };

            Footer.Y = Footer.Height;
            Footer.MoveToY(0, Easing.OutQuint, 600);
        }

        /// <summary>
        /// </summary>
        private void CreateSearchBox() => Searchbox = new LobbySearchBox(this, new ScalableVector2(450, 36))
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            Y = Header.Height + 20,
            X = -22
        };

        /// <summary>
        /// </summary>
        private void CreateMatchContainer() => MatchContainer = new LobbyMatchScrollContainer((LobbyScreen) Screen, new List<MultiplayerGame>(), int.MaxValue,
            0,
            new ScalableVector2(1320, 584),
            new ScalableVector2(1320, 584))
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = Searchbox.Y + Searchbox.Height + 20
        };

        /// <summary>
        /// </summary>
        private void CreateMatchInfo() => MatchInfo = new LobbyMatchInfo(Screen as LobbyScreen)
        {
            Parent = Container,
            Alignment = Alignment.TopLeft,
            X = 30,
            Y = Searchbox.Y
        };
    }
}