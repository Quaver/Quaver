using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Dialogs
{
    public class YesNoTextDialog : YesNoDialog
    {
        /// <summary>
        /// </summary>
        protected Textbox Textbox { get; set; }

        /// <summary>
        ///     The action to be performed when the user submits the text.
        /// </summary>
        public Action<string> SubmitAction { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public YesNoTextDialog(string header, string confirmationText, string initialText, string placeHolderText,
            Action<string> submitAction = null, Action cancelAction = null) : base(header, confirmationText, null, cancelAction)
        {
            SubmitAction = submitAction;
            YesAction += () =>
            {
                SubmitAction?.Invoke(Textbox.RawText);
            };
            Panel.Height += 50;
            YesButton.Image = UserInterface.SaveButton;
            YesButton.Y += 10;
            NoButton.Y += 10;

            CreateTextbox(initialText, placeHolderText);
        }

        /// <summary>
        /// </summary>
        private void CreateTextbox(string initialText, string placeHolderText)
        {
            Textbox = new Textbox(new ScalableVector2(Panel.Width * 0.90f, 50), FontManager.GetWobbleFont(Fonts.LatoBlack),
                20, initialText, placeHolderText, s =>
                {
                    SubmitAction?.Invoke(s);
                    Close();
                })
            {
                Parent = Panel,
                Alignment = Alignment.TopCenter,
                Y = 180,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                AlwaysFocused = true
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