using System.Text.RegularExpressions;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Filter.Items
{
    public class DownloadFilterTableItemLongNotePercent : DownloadFilterTableItem
    {
        /// <summary>
        /// </summary>
        private DownloadFilterItemLongNoteTextbox Max { get; }

        /// <summary>
        /// </summary>
        private DownloadFilterItemLongNoteTextbox Min { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public DownloadFilterTableItemLongNotePercent(int width, BindableInt min, BindableInt max) : base(width, "Long Note %")
        {
            const int spacing = 12;

            Max = new DownloadFilterItemLongNoteTextbox("Max LN%", max)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X
            };

            Min = new DownloadFilterItemLongNoteTextbox("Min LN%", min)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = Max.X - Max.Width - spacing,
            };
        }
    }

    public class DownloadFilterItemLongNoteTextbox : DownloadFilterTextbox
    {
        /// <summary>
        /// </summary>
        /// <param name="placeholder"></param>
        /// <param name="bindable"></param>
        public DownloadFilterItemLongNoteTextbox(string placeholder, BindableInt bindable)
            : base(FontManager.GetWobbleFont(Fonts.LatoBlack),22, "", placeholder)
        {
            AllowedCharacters = new Regex(@"^([0-9]|[1-9][0-9]|100)$");

            OnStoppedTyping += s =>
            {
                if (string.IsNullOrEmpty(s) || s == ".")
                    bindable.Value = 0;
                else
                {
                    var value = int.Parse(s);
                    bindable.Value = value;
                }

                bindable.TriggerChangeEvent();
            };
        }
    }
}