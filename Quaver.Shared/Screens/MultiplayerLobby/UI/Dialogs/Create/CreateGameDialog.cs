using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs.Create
{
    public sealed class CreateGameDialog : YesNoDialog
    {
        /// <summary>
        /// </summary>
        private LabelledTextbox Name { get; set; }

        /// <summary>
        /// </summary>
        private LabelledTextbox Password { get; set; }

        /// <summary>
        /// </summary>
        private TextboxTabControl TabControl { get; }

        /// <summary>
        /// </summary>
        private MaxPlayersDropdown MaxPlayers { get; set; }

        /// <summary>
        /// </summary>
        private CreateGameRulesetDropdown Ruleset { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public CreateGameDialog() : base(MultiplayerLobbyLocalization.Get("CreateMultiplayerGameTitle"),
            MultiplayerLobbyLocalization.Get("CreateMultiplayerGameMessage"))
        {
            Panel.Height = 510;
            Panel.Image = UserInterface.BlankBox;
            Panel.Tint = ColorHelper.HexToColor("#242424");
            Panel.AddBorder(ColorHelper.HexToColor("#0FBAE5"), 2);

            CreateNameTextbox();
            CreatePasswordTextbox();

            TabControl = new TextboxTabControl(new List<Textbox>
            {
                Name.Textbox,
                Password.Textbox
            })
            {
                Parent = this
            };

            CreateMaxPlayersDropdown();
            CreateRulesetDropdown();

            Name.Height = 0;
            Password.Height = 0;

            YesButton.Y = -30;
            YesButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "CREATE", 20, Color.White);
            NoButton.Y = YesButton.Y;

            YesAction += OnCreateClicked;
            ValidateBeforeClosing = ValidateFields;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Ruleset.Dropdown.Opened || MaxPlayers.Dropdown.Opened)
            {
                YesButton.Depth = 1;
                NoButton.Depth = 1;
            }
            else
            {
                YesButton.Depth = 0;
                NoButton.Depth = 0;
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (DialogManager.Dialogs.First() == this)
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                {
                    NoAction?.Invoke();
                    Close();
                    return;
                }
            }
        }

        /// <summary>
        /// </summary>
        private void CreateNameTextbox()
        {
            Name = new LabelledTextbox(Panel.Width * 0.90f, MultiplayerLobbyLocalization.Get("GameName"), 22, 44,
                20, 14, MultiplayerLobbyLocalization.Get("ChooseGameNamePlaceholder"))
            {
                Parent = Panel,
                Y = Banner.Height + 18,
                Alignment = Alignment.TopCenter,
                Textbox =
                {
                    AllowSubmission = false,
                    MaxCharacters = 50
                }
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePasswordTextbox()
        {
            Password = new LabelledTextbox(Name.Width, MultiplayerLobbyLocalization.Get("Password"), Name.Label.FontSize, (int) Name.Textbox.Height,
                Name.Textbox.InputText.FontSize, 14, MultiplayerLobbyLocalization.Get("EnterPasswordPlaceholder"))
            {
                Parent = Panel,
                Y = Name.Y + Name.Height + 18,
                Alignment = Alignment.TopCenter,
                Textbox =
                {
                    AllowSubmission = false,
                    MaxCharacters = 50
                }
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMaxPlayersDropdown()
        {
            MaxPlayers = new MaxPlayersDropdown()
            {
                Parent = Panel,
                Alignment = Alignment.TopRight,
                Y = Password.Y + Password.Height + 22,
                X = -100
            };
        }

        /// <summary>
        /// </summary>
        private void CreateRulesetDropdown()
        {
            Ruleset = new CreateGameRulesetDropdown()
            {
                Parent = Panel,
                Alignment = Alignment.TopLeft,
                Y = MaxPlayers.Y,
                X = -MaxPlayers.X
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Close()
        {
            Name.Visible = false;
            Password.Visible = false;
            Ruleset.Visible = false;
            MaxPlayers.Visible = false;

            base.Close();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private bool ValidateFields()
        {
            if (string.IsNullOrEmpty(Name.Textbox.RawText) || string.IsNullOrWhiteSpace(Name.Textbox.RawText))
                return false;

            if (MapManager.Selected.Value == null)
            {
                NotificationManager.Show(NotificationLevel.Warning, MultiplayerLobbyLocalization.Get("CannotCreateGameWithoutMap"));
                return false;
            }

            return true;
        }

        /// <summary>
        /// </summary>
        private void OnCreateClicked()
        {
            if (!ValidateFields())
            {
                Name.Textbox.Border.ClearAnimations();
                Name.Textbox.Border.Tint = Color.Crimson;
                Name.Textbox.Border.FadeToColor(ColorHelper.HexToColor("#2F2F2F"), Easing.Linear, 250);
                return;
            }

            CreateGame();
        }

        /// <summary>
        /// </summary>
        private void CreateGame()
        {
            DialogManager.Show(new JoinGameDialog(null, null, true));

            ThreadScheduler.Run(() =>
            {
                var name = Name.Textbox.RawText;
                var password = Password.Textbox.RawText;
                var maxPlayers = int.Parse(MaxPlayers.Dropdown.SelectedText.Text);
                var map = MapManager.Selected.Value.ToString();
                var mapId = MapManager.Selected.Value.MapId;
                var mapsetId = MapManager.Selected.Value.MapSetId;
                var ruleset = (MultiplayerGameRuleset) Ruleset.Dropdown.SelectedIndex;
                var mode = (byte) MapManager.Selected.Value.Mode;
                var difficultyRating = MapManager.Selected.Value.DifficultyFromMods(ModManager.Mods);
                var md5 = MapManager.Selected.Value.Md5Checksum;
                var difficultyRatings = MapManager.Selected.Value.GetDifficultyRatings();
                var judgementCount = MapManager.Selected.Value.GetJudgementCount();
                var alternativeMd5 = MapManager.Selected.Value.GetAlternativeMd5();

                var game = MultiplayerGame.CreateCustom(MultiplayerGameType.Friendly, name, password, maxPlayers, map,
                    mapId, mapsetId, ruleset, false, true, mode, difficultyRating, md5, difficultyRatings,
                    judgementCount, alternativeMd5);

                OnlineManager.Client?.CreateMultiplayerGame(game);
            });
        }
    }
}
