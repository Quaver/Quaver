/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Settings;
using Wobble;
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
            GameBase.Game.GlobalUserInterface.Cursor.Show(1);
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 1;

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
                DialogManager.Show(new ConfirmCancelDialog("Are you sure you want to exit Quaver?",(sender, args) =>
                {
                    var game = GameBase.Game as QuaverGame;
                    game?.Exit();
                }));
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InMenus, -1, "",
            (byte) ConfigManager.SelectedGameMode.Value, "", (long) ModManager.Mods);
    }
}
