using System;
using System.Collections.Generic;
using Quaver.Shared.Graphics.Containers;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Edit.UI.AutoMods
{
    public class EditorAutoModScrollPanel : PoolableScrollContainer<Drawable>
    {
        public EditorAutoModScrollPanel() : base(new List<Drawable>(), int.MaxValue,
            0, new ScalableVector2(0, 0), new ScalableVector2(0, 0))
        {
            Image = AssetLoader.LoadTexture2DFromFile(@"C:\users\swan\desktop\automod-scroll-panel.png");
            Size = new ScalableVector2(720, 600);

            Scrollbar.Visible = false;
        }

        protected override PoolableSprite<Drawable> CreateObject(Drawable item, int index) => null;
    }
}