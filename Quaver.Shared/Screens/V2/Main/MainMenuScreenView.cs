using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Managers;
using Wobble.Screens;

namespace Quaver.Shared.Screens.V2.Main
{
    /// <summary>
    ///     View for the rewritten main menu.
    /// </summary>
    public class MainMenuScreenView : ScreenView
    {
        private SpriteTextPlus Title { get; }

        public MainMenuScreenView(MainMenuScreen screen) : base(screen)
        {
            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "Main Menu v2", 48)
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
