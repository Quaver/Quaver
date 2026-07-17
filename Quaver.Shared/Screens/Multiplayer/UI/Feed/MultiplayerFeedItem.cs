using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI.Feed
{
    public class MultiplayerFeedItem : SpriteTextPlus
    {
        public MultiplayerFeedItem(Color color, string text) : base(FontManager.GetWobbleFont(Fonts.InterBold), text)
        {
            FontSize = 13;
            Text = $"[{DateTime.Now.ToLocalTime().ToShortTimeString()}]";

            // ReSharper disable once ObjectCreationAsStatement
            new SpriteTextPlus(Font, text)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                FontSize = 14,
                X = Width + 6,
                Tint = color
            };
        }
    }
}
