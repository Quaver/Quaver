using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs
{
    public class JoinPasswordGameDialog : YesNoDialog
    {
        /// <summary>
        /// </summary>
        private MultiplayerGame Game { get; }

        /// <summary>
        /// </summary>
        protected Textbox Textbox { get; set; }

        /// <summary>
        /// </summary>
        private bool IsSpectating { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public JoinPasswordGameDialog(MultiplayerGame game, bool spectating = false, string initialText = "") : base("ENTER GAME PASSWORD",
            "Enter the password to join the multiplayer game...")
        {
            Game = game;
            IsSpectating = spectating;

            YesButton.Visible = false;
            YesButton.IsClickable = false;
            NoButton.Visible = false;
            NoButton.IsClickable = false;

            CreateTextbox(initialText);
        }

        /// <summary>
        /// </summary>
        private void CreateTextbox(string initialText)
        {
            Textbox = new Textbox(new ScalableVector2(Panel.Width * 0.90f, 50), FontManager.GetWobbleFont(Fonts.LatoBlack),
                20, initialText, "Enter password...", s =>
                {
                    DialogManager.Show(new JoinGameDialog(Game, s, false, IsSpectating));
                })
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Y = -44,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                AlwaysFocused = true,
                MaxCharacters = 15
            };

            Textbox.AddBorder(ColorHelper.HexToColor("#363636"), 2);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Close()
        {
            Textbox.Visible = false;
            base.Close();
        }
    }
}