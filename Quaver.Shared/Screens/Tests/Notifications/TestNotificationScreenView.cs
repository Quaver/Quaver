using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Notifications
{
    public class TestNotificationScreenView : ScreenView
    {
        public TestNotificationScreenView(Screen screen) : base(screen)
        {
            var error = new DrawableNotification(null,
                new NotificationInfo(NotificationLevel.Error, "This is an error notification", true), -1)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = 200
            };

            new DrawableNotification(null,
                new NotificationInfo(NotificationLevel.Info,
                    "This is an information notification\nWould you like to receive info?", false), -1)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 100
            };

            new DrawableNotification(null,
                new NotificationInfo(NotificationLevel.Success, "Success notification!", false), -1)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 200
            };

            new DrawableNotification(null,
                new NotificationInfo(NotificationLevel.Warning, "Danger!", false), -1)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 300
            };
        }

        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2F2F2F"));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}