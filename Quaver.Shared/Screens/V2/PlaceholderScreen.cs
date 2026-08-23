using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Screens.V2.SkinEditor;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.V2
{
    /// <summary>
    ///     Temporary behavior shared by empty V2 replacement screens.
    /// </summary>
    internal abstract class PlaceholderScreen : SkinV2Screen
    {
        protected override ISkinV2EditorHost SkinEditorHost => (ISkinV2EditorHost) View;

        protected virtual bool CanExitToMainMenu => true;

        public override void Update(GameTime gameTime)
        {
            if (CanExitToMainMenu && !Exiting && !IsSkinEditorActive &&
                DialogManager.Dialogs.Count == 0 && KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Exit(() => QuaverScreenFactory.CreateMainMenu());

            base.Update(gameTime);
        }
    }
}
