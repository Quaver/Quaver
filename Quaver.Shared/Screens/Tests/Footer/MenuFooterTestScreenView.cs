using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Graphics.Notifications;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Footer
{
    public class MenuFooterTestScreenView : ScreenView
    {
        public MenuFooterTestScreenView(Screen screen) : base(screen)
        {
            var footer = new MenuFooter(new List<ButtonText>
            {
                new ButtonText(FontsBitmap.GothamRegular, "Back", 16, null),
            }, new List<ButtonText>()
            {
                new ButtonText(FontsBitmap.GothamRegular, "Test Button 6", 10, null),
                new ButtonText(FontsBitmap.GothamRegular, "Test Button 7", 10, null),
                new ButtonText(FontsBitmap.GothamRegular, "Test Button 8", 10, null),
            }, Colors.MainAccent)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };
        }

        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        public override void Destroy()
        {
            Container?.Destroy();
        }
    }
}
