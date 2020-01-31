using System.Text.RegularExpressions;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Filter.Items
{
    public class DownloadFilterTableItemMaxCombo : DownloadFilterTableItem
    {
        /// <summary>
        /// </summary>
        private DownloadFilterItemMaxComboTextbox Max { get; }

        /// <summary>
        /// </summary>
        private DownloadFilterItemMaxComboTextbox Min { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public DownloadFilterTableItemMaxCombo(int width, BindableInt min, BindableInt max) : base(width, "Max Combo")
        {
            const int spacing = 12;

            Max = new DownloadFilterItemMaxComboTextbox("Max Combo", max)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X
            };

            Min = new DownloadFilterItemMaxComboTextbox("Min Combo", min)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = Max.X - Max.Width - spacing
            };
        }
    }

    public class DownloadFilterItemMaxComboTextbox : DownloadFilterTextbox
    {
        /// <summary>
        /// </summary>
        /// <param name="placeholder"></param>
        /// <param name="bindable"></param>
        public DownloadFilterItemMaxComboTextbox(string placeholder, BindableInt bindable)
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