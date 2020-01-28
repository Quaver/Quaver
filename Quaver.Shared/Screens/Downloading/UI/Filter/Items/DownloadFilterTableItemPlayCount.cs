using System.Text.RegularExpressions;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Filter.Items
{
    public class DownloadFilterTableItemPlayCount : DownloadFilterTableItem
    {
        /// <summary>
        /// </summary>
        private DownloadFilterItemPlayCountTextbox Max { get; }

        /// <summary>
        /// </summary>
        private DownloadFilterItemPlayCountTextbox Min { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public DownloadFilterTableItemPlayCount(int width, BindableInt min, BindableInt max) : base(width, "Ranked Play Count")
        {
            const int spacing = 12;

            Max = new DownloadFilterItemPlayCountTextbox("Max Plays", max)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X
            };

            Min = new DownloadFilterItemPlayCountTextbox("Min Plays", min)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = Max.X - Max.Width - spacing
            };
        }
    }

    public class DownloadFilterItemPlayCountTextbox : DownloadFilterTextbox
    {
        /// <summary>
        /// </summary>
        /// <param name="placeholder"></param>
        /// <param name="bindable"></param>
        public DownloadFilterItemPlayCountTextbox(string placeholder, BindableInt bindable)
            : base(FontManager.GetWobbleFont(Fonts.LatoBlack), 22, "", placeholder)
        {
            AllowedCharacters = new Regex(@"^(?!.*\..*\.)[.\d]+$");

            OnStoppedTyping += s =>
            {
                if (string.IsNullOrEmpty(s) || s == ".")
                    bindable.Value = 0;
                else
                {
                    int.TryParse(s, out var value);
                    bindable.Value = value;
                }
            };
        }
    }
}