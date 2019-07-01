using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI.Feed
{
    public class MultiplayerFeedItem : SpriteTextBitmap
    {
        public MultiplayerFeedItem(Color color, string text) : base(FontsBitmap.GothamRegular, text)
        {
            FontSize = 15;
            Text = $"[{DateTime.Now.ToLocalTime().ToShortTimeString()}]";

            // ReSharper disable once ObjectCreationAsStatement
            new SpriteTextBitmap(Font, text)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                FontSize = 16,
                X = Width + 6,
                Tint = color
            };
        }
    }
}