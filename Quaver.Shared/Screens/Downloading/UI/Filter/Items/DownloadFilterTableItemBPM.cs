using System.Text.RegularExpressions;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Filter.Items
{
    public class DownloadFilterTableItemBpm : DownloadFilterTableItem
    {
        /// <summary>
        /// </summary>
        private DownloadFilterItemBpmTextbox Max { get; }

        /// <summary>
        /// </summary>
        private DownloadFilterItemBpmTextbox Min { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public DownloadFilterTableItemBpm(int width, BindableFloat min, BindableFloat max) : base(width, "BPM")
        {
            const int spacing = 12;

            Max = new DownloadFilterItemBpmTextbox("Max BPM", max)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X
            };

            Min = new DownloadFilterItemBpmTextbox("Min BPM", min)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = Max.X - Max.Width - spacing
            };
        }
    }

    public class DownloadFilterItemBpmTextbox : DownloadFilterTextbox
    {
        /// <summary>
        /// </summary>
        /// <param name="placeholder"></param>
        /// <param name="bindable"></param>
        public DownloadFilterItemBpmTextbox(string placeholder, BindableFloat bindable) : base(FontManager.GetWobbleFont(Fonts.LatoBlack),
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
    }
}