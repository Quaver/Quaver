using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics.Form.Checkboxes;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.CheckboxContainers
{
    public class TestCheckboxContainerScreenView : ScreenView
    {
        public TestCheckboxContainerScreenView(Screen screen) : base(screen)
        {
            var channels = new List<ICheckboxContainerItem>();

            for (var i = 0; i < 15; i++)
            {
                channels.Add(new TestCheckboxChatChannel()
                {
                    Name = $"Channel {i}"
                });
            }

            new CheckboxContainer(channels, new ScalableVector2(250, 400), 250)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };

            new CheckboxContainer(channels, new ScalableVector2(250, 400), 425)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                X = 300
            };
        }

        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2f2f2f"));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}