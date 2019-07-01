﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata;
using Quaver.Shared.Screens.Lobby.UI.Dialogs.Joining;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using TagLib.Id3v2;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Lobby.UI.Dialogs.Create
{
    public class CreateGameInterface : Sprite
    {
        /// <summary>
        /// </summary>
        private CreateGameDialog Dialog { get; }

        /// <summary>
        /// </summary>
        public Sprite HeaderBackground { get; private set; }

        /// <summary>
        /// </summary>
        public Sprite FooterBackground { get; private set; }

        /// <summary>
        /// </summary>
        private Button OkButton { get; set; }

        /// <summary>
        /// </summary>
        private Button CancelButton { get; set; }

        /// <summary>
        /// </summary>
        private LabelledTextbox GameName { get; set; }

        /// <summary>
        /// </summary>
        private LabelledTextbox Password { get; set; }

        /// <summary>
        /// </summary>
        private LabelledHorizontalSelector GameType { get; set; }

        /// <summary>
        /// </summary>
        private LabelledHorizontalSelector Ruleset { get; set; }

        /// <summary>
        /// </summary>
        private LabelledHorizontalSelector MaxPlayers { get; set; }

        /// <summary>
        /// </summary>
        private LabelledHorizontalSelector AutoHostRotation { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        public CreateGameInterface(CreateGameDialog dialog)
        {
            Dialog = dialog;
            Size = new ScalableVector2(400, 502);
            Alpha = 1f;
            Image = UserInterface.LobbyCreateGame;

            CreateHeader();
            CreateFooter();
            CreateOkButton();
            CreateCancelButton();
            CreateContainer();
        }

        /// <summary>
        /// </summary>
        private void CreateHeader() => HeaderBackground = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(Width, 38),
            Alpha = 0
        };

        /// <summary>
        /// </summary>
        private void CreateFooter() => FooterBackground = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(Width, 38),
            Tint = ColorHelper.HexToColor("#212121"),
            Alignment = Alignment.BotLeft,
            Y = 1,
            Alpha = 0
        };

        /// <summary>
        /// </summary>
        private void CreateOkButton()
        {
            OkButton = new BorderedTextButton("OK", Color.LimeGreen, (sender, args) =>
            {
                CreateGame();
            })
            {
                Parent = FooterBackground,
                Alignment = Alignment.MidRight,
                X = -20,
                Text =
                {
                    Font = Fonts.Exo2SemiBold,
                    FontSize = 13
                }
            };

            OkButton.Height -= 8;
        }

        /// <summary>
        /// </summary>
        private void CreateCancelButton()
        {
            CancelButton = new BorderedTextButton("Cancel", Color.Crimson, (sender, args) => Dialog.Close())
            {
                Parent = FooterBackground,
                Alignment = Alignment.MidRight,
                X = OkButton.X - OkButton.Width - 20,
                Text =
                {
                    Font = Fonts.Exo2SemiBold,
                    FontSize = 13
                }
            };

            CancelButton.Height -= 8;
        }

        /// <summary>
        /// </summary>
        private void CreateContainer()
        {
            var width = Width - 30;

            GameName = new LabelledTextbox(width, "Game Name")
            {
                Parent = this,
                Y = HeaderBackground.Y + HeaderBackground.Height + 18,
                Alignment = Alignment.TopCenter,
                Textbox =
                {
                    RawText = $"{ConfigManager.Username.Value}'s Game",
                    InputText =
                    {
                        Text = $"{ConfigManager.Username.Value}'s Game"
                    },
                    MaxCharacters = 100
                }
            };

            const int spacing = 30;

            Password = new LabelledTextbox(width, "Password")
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = GameName.Y + GameName.Height + spacing,
                Textbox =
                {
                    MaxCharacters = 100,
                    AllowedCharacters = new Regex("^.{1,100}$")
                }
            };

            Ruleset = new LabelledHorizontalSelector(width - 50, "Game Mode", new List<string>
            {
                "Free-For-All",
                "Team",
                "Battle-Royale"
            })
            {
                Parent = this,
                Y = Password.Y + Password.Height + spacing,
                X = 16
            };

            MaxPlayers = new LabelledHorizontalSelector(width - 50, "Max Players", new List<string>
            {
                "2",
                "4",
                "6",
                "8",
                "10",
                "12",
                "14",
                "16"
            }, 7)
            {
                Parent = this,
                Y = Ruleset.Y + Ruleset.Height + spacing,
                X = 16
            };

            AutoHostRotation = new LabelledHorizontalSelector(width - 50, "Host Rotation", new List<string>
            {
                "No",
                "Yes"
            })
            {
                Parent = this,
                Y = MaxPlayers.Y + MaxPlayers.Height + spacing,
                X = 16
            };
        }

        /// <summary>
        /// </summary>
        public void CreateGame()
        {
            if (MapManager.Selected.Value == null)
            {
                NotificationManager.Show(NotificationLevel.Error, "You cannot create a game without having a map selected!");
                return;
            }

            if (string.IsNullOrEmpty(GameName.Textbox.RawText) || string.IsNullOrWhiteSpace(GameName.Textbox.RawText))
            {
                NotificationManager.Show(NotificationLevel.Error, "You must provide a valid game name.");
                return;
            }

            DialogManager.Show(new JoiningGameDialog(JoiningGameDialogType.Creating));

            ThreadScheduler.Run(() =>
            {
                var game = MultiplayerGame.CreateCustom(MultiplayerGameType.Friendly,
                    GameName.Textbox.RawText, Password.Textbox.RawText,
                    int.Parse(MaxPlayers.Selector.SelectedItemText.Text), MapManager.Selected.Value.ToString(),
                    MapManager.Selected.Value.MapId,
                    MapManager.Selected.Value.MapSetId, Enum.Parse<MultiplayerGameRuleset>(Ruleset.Selector.SelectedItemText.Text.Replace("-", "_")),
                    AutoHostRotation.Selector.SelectedItemText.Text == "Yes", (byte) MapManager.Selected.Value.Mode,
                    MapManager.Selected.Value.DifficultyFromMods(ModManager.Mods), MapManager.Selected.Value.Md5Checksum,
                    MapManager.Selected.Value.GetDifficultyRatings(), MapManager.Selected.Value.GetJudgementCount(), MapManager.Selected.Value.GetAlternativeMd5());

                OnlineManager.Client.CreateMultiplayerGame(game);
            });

            Dialog.Close();
        }
    }
}