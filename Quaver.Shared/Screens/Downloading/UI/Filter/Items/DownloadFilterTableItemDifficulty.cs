using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Filter.Items
{
    public class DownloadFilterTableItemDifficulty : DownloadFilterTableItem
    {
        /// <summary>
        /// </summary>
        private DownloadFilterItemDifficultyTextbox Max { get; }

        /// <summary>
        /// </summary>
        private DownloadFilterItemDifficultyTextbox Min { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="minDiff"></param>
        /// <param name="maxDiff"></param>
        public DownloadFilterTableItemDifficulty(int width, BindableFloat minDiff, BindableFloat maxDiff)
            : base(width, "Difficulty Rating")
        {
            const int spacing = 12;

            Max = new DownloadFilterItemDifficultyTextbox("Max Rating", maxDiff)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X
            };

            Min = new DownloadFilterItemDifficultyTextbox("Min Rating", minDiff)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = Max.X - Max.Width - spacing
            };
        }
    }

    public class DownloadFilterItemDifficultyTextbox : DownloadFilterTextbox
    {
        /// <summary>
        /// </summary>
        private string PreviousRawText { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="placeholder"></param>
        /// <param name="bindable"></param>
        public DownloadFilterItemDifficultyTextbox(string placeholder, BindableFloat bindable) : base(FontManager.GetWobbleFont(Fonts.LatoBlack),
            22, "", placeholder)
        {
            AllowedCharacters = new Regex(@"^(?!.*\..*\.)[.\d]+$");

            OnStoppedTyping += s =>
            {
                if (string.IsNullOrEmpty(s) || s == ".")
                    bindable.Value = 0;
                else
                {
                    var value = float.Parse(s);
                    bindable.Value = value;
                }
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (RawText != PreviousRawText)
            {
                if (string.IsNullOrEmpty(RawText) || RawText == ".")
                    InputText.Tint = Color.White;
                else
                    InputText.Tint = ColorHelper.DifficultyToColor(float.Parse(RawText));
            }

            PreviousRawText = RawText;
            base.Update(gameTime);
        }
    }
}