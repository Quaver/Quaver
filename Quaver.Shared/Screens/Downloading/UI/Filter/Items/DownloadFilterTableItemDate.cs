using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Filter.Items
{
    public class DownloadFilterTableItemDate : DownloadFilterTableItem
    {
        /// <summary>
        /// </summary>
        private DownloadFilterItemDateTextbox Max { get; }

        /// <summary>
        /// </summary>
        private DownloadFilterItemDateTextbox Min { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public DownloadFilterTableItemDate(int width, string name, Bindable<string> min, Bindable<string> max)
            : base(width, name)
        {
            const int spacing = 12;

            Max = new DownloadFilterItemDateTextbox("End Date", max)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X
            };

            Min = new DownloadFilterItemDateTextbox("Start Date", min)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = Max.X - Max.Width - spacing
            };
        }
    }

    /// <summary>
    /// </summary>
    public class DownloadFilterItemDateTextbox : DownloadFilterTextbox
    {
        /// <summary>
        /// </summary>
        private Regex Regex { get; } = new Regex(@"^[0-9m]{1,2}\-[0-9d]{1,2}\-[0-9y]{4}$");

        /// <summary>
        /// </summary>
        private string PreviousRawText { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="placeholder"></param>
        /// <param name="bindable"></param>
        public DownloadFilterItemDateTextbox(string placeholder, Bindable<string> bindable)
            : base(FontManager.GetWobbleFont(Fonts.LatoBlack), 22, "", placeholder)
        {
            OnStoppedTyping += s =>
            {
                if (string.IsNullOrEmpty(s) || s == ".")
                    bindable.Value = "";
                else
                {
                    if (Regex.IsMatch(s))
                        bindable.Value = s;
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
                Border.Tint = Regex.IsMatch(RawText) || string.IsNullOrEmpty(RawText) ? ColorHelper.HexToColor("#5B5B5B") : Color.Crimson;

            PreviousRawText = RawText;

            base.Update(gameTime);
        }
    }
}