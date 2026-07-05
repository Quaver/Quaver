using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics
{
    public class Tooltip : Sprite
    {
        public SpriteTextPlus Text { get; }

        public Tooltip(string text, Color color, bool cacheText = true)
        {
            Tint = ColorHelper.HexToColor("#161616");
            AddBorder(color, 2);

            DestroyIfParentIsNull = false;
            SetChildrenAlpha = true;
            SetChildrenVisibility = true;

            Text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "", 20, cacheText)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
            };

            ChangeText(text);
            Hide();
        }

        public void ChangeText(string text)
        {
            Text.Text = text;

            const int padding = 16;
            Size = new ScalableVector2(Text.Width + padding, Text.Height + padding);
        }

        public void Show() => Visible = true;

        public void Hide()
        {
            Visible = false;
            Parent = null;
        }
    }
}
