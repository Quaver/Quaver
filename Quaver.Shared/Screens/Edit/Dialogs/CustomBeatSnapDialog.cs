using System;
using System.Text.RegularExpressions;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class CustomBeatSnapDialog : YesNoDialog
    {
        /// <summary>
        /// </summary>
        protected Textbox Textbox { get; set; }

        /// <summary>
        /// </summary>
        private BindableInt BeatSnap { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public CustomBeatSnapDialog(BindableInt beatSnap) : base("CUSTOM BEAT SNAP",
            "Enter a value for the custom beat snap divisor (1/?)...")
        {
            BeatSnap = beatSnap;

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
                20, "", "Enter a beat snap value (max 48)", s =>
                {
                    BeatSnap.Value = int.Parse(s);
                })
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Y = -44,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                AlwaysFocused = true,
                AllowedCharacters = new Regex(@"^([1-9]|[1-3][0-9]|4[0-8])$")
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