using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Dialogs;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Screens;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Shared.Screens.Menu
{
    public class MenuScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Menu;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     Dictates if this is the first ever menu screen load.
        ///     Used to determine if we should auto-connect to the server
        /// </summary>
        public static bool FirstMenuLoad { get; set; }

        /// <summary>
        /// </summary>
        public MenuScreen()
        {
            try
            {
                ModManager.RemoveSpeedMods();
            }
            catch (Exception e)
            {
                // ignored
            }

            View = new MenuScreenView(this);

            if (!FirstMenuLoad && ConfigManager.AutoLoginToServer.Value)
            {
                OnlineManager.Login();
                FirstMenuLoad = true;
            }
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            AudioEngine.Track?.Fade(ConfigManager.VolumeMusic.Value, 500);
            base.OnFirstUpdate();
        }

        /// <summary>
        ///     Handles all the input for this screen.
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleInput(GameTime gameTime)
        {
            if (DialogManager.Dialogs.Count > 0 || Exiting)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Show(new QuitDialog());
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InMenus, -1, "",
            (byte) ConfigManager.SelectedGameMode.Value, "", (long) ModManager.Mods);
    }
}