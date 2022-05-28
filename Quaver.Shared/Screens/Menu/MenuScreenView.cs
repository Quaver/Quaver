/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Online;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Menu.UI.Buttons;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Menu.UI.Navigation;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Quaver.Shared.Screens.Menu.UI.Panels;
using Quaver.Shared.Screens.Menu.UI.Tips;
using Quaver.Shared.Screens.Menu.UI.Visualizer;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.Options;
using Quaver.Shared.Screens.Selection;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Menu
{
    public class MenuScreenView : ScreenView
    {
        /// <summary>
        ///     The main menu background.
        /// </summary>
        public BackgroundImage Background { get; set; }

        /// <summary>
        ///    The navbar at the top of the screen.
        /// </summary>
        public Navbar Navbar { get; set; }

        /// <summary>
        ///     The line on the bottom.
        /// </summary>
        public Line BottomLine { get; set; }

        /// <summary>
        ///     The container for the screen content.
        /// </summary>
        public ScrollContainer MiddleContainer { get; set; }

        /// <summary>
        ///     The menu tip displayed at the bottom.
        /// </summary>
        public MenuTip Tip { get; set; }

        /// <summary>
        ///    The button to exit the game.
        /// </summary>
        public ToolButton PowerButton { get; set; }

        /// <summary>
        ///     The button to access the game settings.
        /// </summary>
        public ToolButton SettingsButton { get; set; }

        /// <summary>
        ///     Contains all the panels for the screen.
        /// </summary>
        public PanelContainer PanelContainer { get; set; }

        /// <summary>
        ///     The text that says "Main Menu"
        /// </summary>
        public SpriteText MainMenuText { get; set; }

        /// <summary>
        ///     The jukebox to play music.
        /// </summary>
        public Jukebox Jukebox { get; set; }

        /// <summary>
        ///     The audio visualizer for the music
        /// </summary>
        public MenuAudioVisualizer Visualizer { get; set; }

        /// <summary>
        ///     The user's profile when the click on their name in the navbar.
        /// </summary>
        public UserProfileContainer UserProfile { get; set; }

        /// <summary>
        ///     The currently logged in user's playercard.
        /// </summary>
        public OnlinePlayercard Playercard { get; set; }

        /// <summary>
        /// </summary>
        public MenuFooter Footer { get; private set; }

        /// <summary>
        /// </summary>
        public MenuHeader Header { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MenuScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateAudioVisualizer();
            CreateMenuFooter();
            CreateMenuHeader();
            CreateMiddleContainer();
            CreatePanelContainer();
            CreateMenuTip();
            CreateJukebox();
            CreatePlayercard();
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
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///     Create
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.MenuBackgroundNormal, 45)
        {
            Parent = Container,
        };

        /// <summary>
        ///     Creates a ScrollContainer for all of the content int he middle of the screen.
        /// </summary>
        private void CreateMiddleContainer() => MiddleContainer = new ScrollContainer(
                new ScalableVector2(WindowManager.Width - 50, 44),
                new ScalableVector2(WindowManager.Width - 50, 44))
        {
            Parent = Container,
            Alignment = Alignment.MidCenter,
            Alpha = 0,
        };

        /// <summary>
        ///     Creates the menu tip
        /// </summary>
        private void CreateMenuTip()
        {
            Tip = new MenuTip
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
                Y = -Footer.Height - 20,
                X = 25
            };
        }

        /// <summary>
        ///     Creates the panels for the screen.
        /// </summary>
        private void CreatePanelContainer() => PanelContainer = new PanelContainer(new List<Panel>()
        {
            // Single Player
            new Panel("Single Player", "Play offline and compete for scoreboard ranks",
                UserInterface.ThumbnailSinglePlayer, OnSinglePlayerPanelClicked),

            // Compettive
            new Panel("Competitive", "Compete against players all over the world",
                UserInterface.ThumbnailCompetitive, OnCompetitivePanelClicked),

            // Custom Games
            new Panel("Multiplayer", "Play custom online matches with other players",
                UserInterface.ThumbnailCustomGames, OnCustomGamesPanelClicked),

            // Editor
            new Panel("Editor", "Create or edit a map to any song you would like",
                UserInterface.ThumbnailEditor, OnEditorPanelClicked),
        })
        {
            Parent = Container
        };

        /// <summary>
        ///     Creates the text that says "Main Menu"
        /// </summary>
        private void CreateTextMainMenu()
        {
            var mainMenuBackground = new Sprite()
            {
                Parent = Container,
                X = 62,
                Y = Navbar.Line.Y + 20,
                Tint = Color.Black,
                Alpha = 0.0f
            };

            MainMenuText = new SpriteText(Fonts.Exo2BoldItalic, "Main Menu", 32)
            {
                Parent = mainMenuBackground,
                Alignment = Alignment.MidCenter,
            };
        }

        /// <summary>
        ///     Creates and aligns the jukebox sprite.
        /// </summary>
        private void CreateJukebox() => Jukebox = new Jukebox
        {
            Parent = Container,
            Alignment = Alignment.TopLeft,
            Y = Header.Y + Header.Height + 20 + 16,
            X = 25
        };

        /// <summary>
        ///     Creates the audio visaulizer container for the screen
        /// </summary>12
        private void CreateAudioVisualizer() => Visualizer = new MenuAudioVisualizer((int) WindowManager.Width, 400, 150, 5)
        {
            Parent = Container,
            Alignment = Alignment.BotLeft,
            Y = -44
        };

        /// <summary>
        ///     Creates the container for user profiles.
        /// </summary>
        private void CreateUserProfile() => UserProfile = new UserProfileContainer(this)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            Y = Navbar.Line.Y + Navbar.Line.Thickness,
            X = -28
        };

        private void CreateMenuFooter()
        {
            Footer = new MenuFooter(new List<ButtonText>()
            {
                new ButtonText(FontsBitmap.GothamRegular, "Quit Game", 14, (sender, args) =>
                {
                    DialogManager.Show(new ConfirmCancelDialog("Are you sure you want to exit Quaver?", (o, ex) =>
                    {
                        var game = GameBase.Game as QuaverGame;
                        game?.Exit();
                    }));
                }),
                new ButtonText(FontsBitmap.GothamRegular, "Options", 14, (sender, args) => DialogManager.Show(new OptionsDialog())),
                new ButtonText(FontsBitmap.GothamRegular, "Chat", 14, (sender, args) =>
                {
                    if (OnlineManager.Status.Value != ConnectionStatus.Connected)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "You must be logged in to use the chat!");
                        return;
                    }
                }),
                new ButtonText(FontsBitmap.GothamRegular, "Download Maps", 14, (sender, args) =>
                {
                    if (OnlineManager.Status.Value != ConnectionStatus.Connected)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "You must be logged in to download maps!");
                        return;
                    }

                    var screen = (QuaverScreen) Screen;
                    screen.Exit(() => new DownloadScreen());
                }),
            }, new List<ButtonText>()
            {
                new ButtonText(FontsBitmap.GothamRegular, "Report Bugs", 14, (sender, args) => BrowserHelper.OpenURL("https://github.com/Quaver/Quaver/issues")),
                new ButtonText(FontsBitmap.GothamRegular, "Discord", 14, (sender, args) => BrowserHelper.OpenURL("https://discord.gg/nJa8VFr")),
                new ButtonText(FontsBitmap.GothamRegular, "Twitter", 14, (sender, args) => BrowserHelper.OpenURL("https://twitter.com/QuaverGame")),
                new ButtonText(FontsBitmap.GothamRegular, "Website", 14, (sender, args) => BrowserHelper.OpenURL("https://quavergame.com")),
            }, Colors.MainAccent)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };
        }

        private void CreateMenuHeader()
        {
            Header = new MenuHeader(UserInterface.QuaverLogoStylish, "Main", "Menu", "Find something to play, or use the editor",
                Colors.MainAccent)
            {
                Parent = Container,
                Alignment = Alignment.TopLeft
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = Header,
                Alignment = Alignment.MidLeft,
                X = Header.Icon.X,
                Image = UserInterface.QuaverLogoFull,
                Size = new ScalableVector2(138, 30)
            };

            Header.Icon.Visible = false;
            Header.Title.Visible = false;
            Header.Title2.Visible = false;
        }

        /// <summary>
        ///     Creates the playercard container
        /// </summary>
        private void CreatePlayercard() => Playercard = new OnlinePlayercard()
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            Position = new ScalableVector2(-Jukebox.X, Jukebox.Y - 16)
        };

        /// <summary>
        ///     Called when the single player panel is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSinglePlayerPanelClicked(object sender, EventArgs e)
        {
            var screen = Screen as MainMenuScreen;

            // We have maps in the queue, so we need to go to the import screen first
            if (MapsetImporter.Queue.Count != 0)
            {
                screen?.Exit(() =>
                {
                    AudioEngine.Track?.Fade(10, 300);
                    return new ImportingScreen();
                });

                return;
            }

            if (MapManager.Mapsets.Count == 0 || MapManager.Selected == null || MapManager.Selected.Value == null)
            {
                if (OnlineManager.Status.Value == ConnectionStatus.Connected)
                {
                    screen?.Exit(() => new DownloadScreen());
                    return;
                }

                NotificationManager.Show(NotificationLevel.Error, "You have no maps loaded. Try importing some!");
                return;
            }

            var button = (Button) sender;
            button.IsClickable = false;

            screen?.Exit(() =>
            {
                AudioEngine.Track?.Fade(10, 300);
                return new SelectionScreen();
            });
        }

        /// <summary>
        ///     Called when the competitive panel is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnCompetitivePanelClicked(object sender, EventArgs e)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet. Check back later.");
        }

        /// <summary>
        ///     Called when the custom games panel is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCustomGamesPanelClicked(object sender, EventArgs e)
        {
            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Error, "You must be logged in to play multiplayer.");
                return;
            }

            var screen = (QuaverScreen) Screen;

            if (MapManager.Mapsets.Count == 0 || MapManager.Selected == null || MapManager.Selected.Value == null)
            {
                if (OnlineManager.Status.Value == ConnectionStatus.Connected)
                {
                    screen?.Exit(() => new DownloadScreen());
                    return;
                }

                NotificationManager.Show(NotificationLevel.Error, "You have no maps loaded. Try importing some!");
                return;
            }

            screen.Exit(() => new MultiplayerLobbyScreen());
        }

        /// <summary>
        ///     Called when the editor panel is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEditorPanelClicked(object sender, EventArgs e)
        {
            if (MapManager.Selected == null || MapManager.Selected.Value == null)
            {
                NotificationManager.Show(NotificationLevel.Error, "You cannot edit without a map selected.");
                return;
            }

            var screen = Screen as MainMenuScreen;

            screen?.Exit(() =>
            {
                if (AudioEngine.Track.IsPlaying)
                    AudioEngine.Track?.Pause();

                try
                {
                    return new EditorScreen(MapManager.Selected.Value.LoadQua(false));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "Unable to read map file!");
                    return new MainMenuScreen();
                }
            });
        }
    }
}