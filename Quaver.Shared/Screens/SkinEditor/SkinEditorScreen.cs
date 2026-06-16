using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Main;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.SkinEditor
{
    public sealed class SkinEditorScreen : QuaverScreen
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.SkinEditor;

        public SkinEditorScreen() => View = new SkinEditorScreenView(this);

        public override void Update(GameTime gameTime)
        {
            HandleInput();
            base.Update(gameTime);
        }

        private void HandleInput()
        {
            if (Exiting || DialogManager.Dialogs.Count != 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                ExitToMenu();
        }

        public void ExitToMenu() => Exit(() => new MainMenuScreen());

        public override UserClientStatus GetClientStatus()
            => new(ClientStatus.InMenus, -1, "", 0, "Editing a skin", (long)ModManager.Mods);
    }
}
