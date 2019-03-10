using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.Select;
using Quaver.Shared.Screens.Settings;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Gameplay.UI.Offset
{
    public class OffsetConfirmDialog : ConfirmCancelDialog
    {
        private QuaverScreen Screen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="offset"></param>
        public OffsetConfirmDialog(QuaverScreen screen, int offset)
            : base($"Your suggested offset is: {offset} ms. Would you like to use this?", (o, e) => OnOffsetConfirm(screen, offset),
                (o, e) => OnCancel(screen)) => Screen = screen;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                OnCancel(Screen);

            base.HandleInput(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="offset"></param>
        private static void OnOffsetConfirm(QuaverScreen screen, int offset)
        {
            ConfigManager.GlobalAudioOffset.Value = offset;
            DialogManager.Dismiss(DialogManager.Dialogs.First());
            Exit(screen);
        }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        private static void OnCancel(QuaverScreen screen)
        {
            DialogManager.Dismiss(DialogManager.Dialogs.First());
            Exit(screen);
        }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public static void Exit(QuaverScreen screen) => screen.Exit(() =>
        {
            var options = new SettingsDialog();
            options.SwitchSelected(options.Sections[1]);
            DialogManager.Show(options);

            ModManager.RemoveAllMods();

            switch (QuaverScreenManager.LastScreen)
            {
                case QuaverScreenType.Select:
                    return new SelectScreen();
                default:
                    return new MenuScreen();
            }
        });
    }
}
