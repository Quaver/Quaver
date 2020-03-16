using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// </summary>
        private List<int> AvailableBeatSnaps { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public CustomBeatSnapDialog(BindableInt beatSnap, List<int> availableBeatSnaps) : base("CUSTOM BEAT SNAP",
            "Enter a value for the custom beat snap divisor (1/?)...")
        {
            BeatSnap = beatSnap;
            AvailableBeatSnaps = availableBeatSnaps;

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
                20, "", "Enter a beat snap value (max 48)", s => OnSubmit(s))
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Y = -100,
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

        private void OnSubmit(string s)
        {
            BeatSnap.Value = int.Parse(s);

            if (!AvailableBeatSnaps.Contains(BeatSnap.Value))
            {
                AvailableBeatSnaps.Add(BeatSnap.Value);
                AvailableBeatSnaps.Sort();
            }
        }
    }
}