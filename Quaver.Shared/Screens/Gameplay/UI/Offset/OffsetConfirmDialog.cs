using System.Linq;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Options;
using Quaver.Shared.Screens.Selection;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Gameplay.UI.Offset
{
    public class OffsetConfirmDialog : YesNoDialog
    {
        private QuaverScreen Screen { get; }

        public OffsetConfirmDialog(QuaverScreen screen, int offset) : base("CHANGE GLOBAL OFFSET",
            $"Your suggested offset is: {offset} ms. Would you like to use this?", () => OnOffsetConfirm(screen, offset),
            () => OnCancel(screen))
        {
            Screen = screen;
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
            var options = new OptionsDialog();
            DialogManager.Show(options);

            ModManager.RemoveAllMods();

            switch (QuaverScreenManager.LastScreen)
            {
                case QuaverScreenType.Select:
                    return new SelectionScreen();
                default:
                    return new MainMenuScreen();
            }
        });
    }
}