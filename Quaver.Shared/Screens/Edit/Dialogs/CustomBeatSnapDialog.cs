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
            "Enter a value or range of values for the custom beat snap divisor (1/?)...")
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
                20, "", "Enter a beat snap value or range (max 48, e.g. \"7\" or \"5-9\")", s => OnSubmit(s))
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Y = -100,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                AlwaysFocused = true,
                AllowedCharacters = new Regex(@"^(?:[1-9]|[1-3][0-9]|4[0-8])(?:-(?:[1-9]|[1-3][0-9]|4[0-8])?)?$")
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
            int from, to;

            if (s.IndexOf('-') is var range && range is -1)
                from = to = int.Parse(s);
            else if (range == s.Length - 1)
                from = to = int.Parse(s.AsSpan()[..^1]);
            else
            {
                from = int.Parse(s.AsSpan()[..range]);
                to = int.Parse(s.AsSpan()[(range + 1)..]);
            }

            // "15-5" should behave the same as "5-15".
            if (from > to)
                (from, to) = (to, from);

            BeatSnap.Value = from;

            for (var i = from; i <= to; i++)
                if (AvailableBeatSnaps.BinarySearch(i) is var index && index < 0)
                    AvailableBeatSnaps.Insert(~index, i);
        }
    }
}
