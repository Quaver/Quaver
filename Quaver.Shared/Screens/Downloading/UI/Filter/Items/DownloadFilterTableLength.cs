using System.Text.RegularExpressions;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Filter.Items
{
    public class DownloadFilterTableLength : DownloadFilterTableItem
    {
        /// <summary>
        /// </summary>
        private DownloadFilterItemLengthTextbox Max { get; }

        /// <summary>
        /// </summary>
        private DownloadFilterItemLengthTextbox Min { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public DownloadFilterTableLength(int width, BindableInt min, BindableInt max) : base(width, "Length (Seconds)")
        {
            const int spacing = 12;

            Max = new DownloadFilterItemLengthTextbox("Max Length", max)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X
            };

            Min = new DownloadFilterItemLengthTextbox("Min Length", min)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = Max.X - Max.Width - spacing
            };
        }
    }

    public class DownloadFilterItemLengthTextbox : DownloadFilterTextbox
    {
        /// <summary>
        /// </summary>
        /// <param name="placeholder"></param>
        /// <param name="bindable"></param>
        public DownloadFilterItemLengthTextbox(string placeholder, BindableInt bindable) : base(FontManager.GetWobbleFont(Fonts.LatoBlack),
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
                    bindable.Value = (int) value;
                }
            };
        }
    }
}