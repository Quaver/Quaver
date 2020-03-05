using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Layers.Rename;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Panels.Layers.Dialogs
{
    public class DialogRenameLayer : YesNoDialog
    {
        private EditorActionManager ActionManager { get; }

        private EditorLayerInfo Layer { get; }

        private Qua WorkingMap { get; }

        protected Textbox Textbox { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DialogRenameLayer(EditorLayerInfo layer, EditorActionManager manager, Qua workingMap) : base("RENAME LAYER",
            "Enter a new name for the your layer...")
        {
            ActionManager = manager;
            Layer = layer;
            WorkingMap = workingMap;

            YesButton.Visible = false;
            YesButton.IsClickable = false;
            NoButton.Visible = false;
            NoButton.IsClickable = false;

            CreateTextbox();
        }

        /// <summary>
        /// </summary>
        private void CreateTextbox()
        {
            Textbox = new Textbox(new ScalableVector2(Panel.Width * 0.90f, 50), FontManager.GetWobbleFont(Fonts.LatoBlack),
                20, Layer.Name, "Enter name...", s =>
                {
                    if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
                        return;

                    ActionManager.Perform(new EditorActionRenameLayer(ActionManager, WorkingMap, Layer, s));
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