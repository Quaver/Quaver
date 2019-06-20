using System.Collections.Generic;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Download.UI.Filter;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Lobby.UI.Dialogs.Create
{
    public class LabelledHorizontalSelector : Sprite
    {
        /// <summary>
        /// </summary>
        private SpriteTextBitmap Label { get; }

        /// <summary>
        /// </summary>
        public HorizontalSelector Selector { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="label"></param>
        /// <param name="options"></param>
        public LabelledHorizontalSelector(float width, string label, List<string> options, int selectedIndex = 0)
        {
            Size = new ScalableVector2(width, 62);
            Alpha = 0;

            Label = new SpriteTextBitmap(FontsBitmap.GothamRegular, label)
            {
                Parent = this,
                FontSize = 17
            };

            Selector = new QuaverHorizontalSelector(options, new ScalableVector2(186, 26), Fonts.SourceSansProSemiBold, 13,
                new ScalableVector2(20, 15), (s, i) => { }, selectedIndex)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                X = 26,
                Y = -2
            };

            Height = Selector.Height;
        }
    }
}