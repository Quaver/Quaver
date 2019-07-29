using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Menu.UI
{
    public class ButtonSupportQuaver : ButtonText
    {
        private Sprite Heart { get; }

        public ButtonSupportQuaver() : base(FontsBitmap.GothamRegular, "Support Quaver", 14)
        {
            Heart = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(14, 14),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_heart_shape_silhouette),
                Tint = Color.Crimson
            };

            Text.X = Heart.X + Heart.Width + 1;

            Size = new ScalableVector2(Heart.Width + 1 + Text.Width, Text.Height);

            Clicked += (sender, args) => BrowserHelper.OpenURL($"https://quavergame.com/donate", true);
        }

        public override void Update(GameTime gameTime)
        {
            Heart.FadeToColor(IsHovered ? Colors.MainAccent : Color.Crimson, gameTime.ElapsedGameTime.TotalMilliseconds, 60);
            base.Update(gameTime);
        }
    }
}