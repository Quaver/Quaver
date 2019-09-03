using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs.Create;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.CreatePlaylists
{
    public class TestScreenCreatePlaylistView : ScreenView
    {
        public TestScreenCreatePlaylistView(Screen screen) : base(screen)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new CreatePlaylistContainer()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
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