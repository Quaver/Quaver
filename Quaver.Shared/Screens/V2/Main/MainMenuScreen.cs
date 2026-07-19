using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Config;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Main.UI;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.V2.Main
{
    /// <summary>
    ///     Rewritten main menu screen.
    /// </summary>
    public sealed class MainMenuScreen : QuaverScreen
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Menu;

        public MainMenuScreen() => View = new MainMenuScreenView(this);

        public override void OnFirstUpdate()
        {
            GameBase.Game.GlobalUserInterface.Cursor.Show(1);
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 1;
            base.OnFirstUpdate();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Exiting && DialogManager.Dialogs.Count == 0 && KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Show(new QuitDialog());

            base.Update(gameTime);
        }

        public override UserClientStatus GetClientStatus() =>
            new(ClientStatus.InMenus, -1, "", (byte)ConfigManager.SelectedGameMode.Value, "", (long)ModManager.Mods);
    }
}
