using System;
using System.Text.RegularExpressions;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions.Offset;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorApplyOffsetDialog : YesNoDialog
    {
        private EditScreen Screen { get; }

        /// <summary>
        /// </summary>
        protected Textbox Textbox { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorApplyOffsetDialog(EditScreen screen) : base("APPLY OFFSET TO MAP",
            "Enter a value to apply an offset to all of your map's objects, timing points,\n and scroll velocities...")
        {
            Screen = screen;
            CreateTextbox();

            Panel.Height += 50;
            YesButton.Y = -30;
            NoButton.Y = YesButton.Y;

            YesButton.Clicked += (sender, args) => OnSubmit(Textbox.RawText);
        }

        /// <summary>
        /// </summary>
        private void CreateTextbox()
        {
            Textbox = new Textbox(new ScalableVector2(Panel.Width * 0.90f, 50), FontManager.GetWobbleFont(Fonts.LatoBlack),
                20, "", "Enter an offset to apply to your map...", OnSubmit)
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Y = -100,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                AlwaysFocused = true,
                AllowedCharacters = new Regex(@"^[0-9-]*$")
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

        private void OnSubmit(string s)
        {
            try
            {
                var val = int.Parse(s);
                Screen.ActionManager.Perform(new EditorActionApplyOffset(Screen.ActionManager, Screen.WorkingMap, val));
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}