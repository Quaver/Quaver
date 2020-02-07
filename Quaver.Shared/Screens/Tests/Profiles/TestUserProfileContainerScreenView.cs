using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.Profile;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Profiles
{
    public class TestUserProfileContainerScreenView : ScreenView
    {
        private Bindable<UserProfile> Profile { get; }

        public TestUserProfileContainerScreenView(Screen screen) : base(screen)
        {
            Profile = new Bindable<UserProfile>(null)
            {
                Value = new UserProfile()
                {
                    Username = "Testuser28",
                }
            };

            Profile.Value.PopulateStats();

            new LocalProfileContainer(Profile)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2F2F2F"));
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}