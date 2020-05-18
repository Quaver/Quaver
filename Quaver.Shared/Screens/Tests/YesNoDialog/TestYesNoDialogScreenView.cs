using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.YesNoDialog
{
    public class TestYesNoDialogScreenView : ScreenView
    {
        private bool IsDarkGrayBackground { get; set; } = true;

        public TestYesNoDialogScreenView(Screen screen) : base(screen)
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.A))
                DialogManager.Show(new Graphics.YesNoDialog("Change Background Color",
                    "Would you like to change the background color?",
                    () => IsDarkGrayBackground = !IsDarkGrayBackground));

            Container?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {

            GameBase.Game.GraphicsDevice.Clear(IsDarkGrayBackground ? ColorHelper.HexToColor("#2f2f2f") : Color.LightGray);
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}