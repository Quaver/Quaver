using FontStashSharp;
using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.Modifiers;
using Wobble;
using Wobble.Screens;
using Alignment = Wobble.Graphics.Alignment;

namespace Quaver.Shared.Screens.Tests.ModifierSelectors
{
    public class TestModifierSelectorScreenView : ScreenView
    {
        public TestModifierSelectorScreenView(Screen screen) : base(screen)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new ModifierSelectorContainer
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
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