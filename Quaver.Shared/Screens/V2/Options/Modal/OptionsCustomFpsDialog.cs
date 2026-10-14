using System;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.V2.UI;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.Modal
{
    internal sealed class OptionsCustomFpsDialog : V2ModalDialog
    {
        public OptionsCustomFpsDialog(Action onChanged = null, Action onClosed = null) : base(LocalizationManager.Get("Screen_Options_SetCustomFPS"), LocalizationManager.Get("Screen_Options_CustomFpsDescription"), onClosed)
        {
            void Apply(string text)
            {
                if (!int.TryParse(text, out var fps))
                {
                    NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_InvalidFps"));
                    return;
                }

                var previous = ConfigManager.CustomFpsLimit.Value;
                ConfigManager.CustomFpsLimit.Value = fps;
                if (ConfigManager.CustomFpsLimit.Value != previous)
                    onChanged?.Invoke();

                NotificationManager.Show(NotificationLevel.Success, LocalizationManager.Get("Screen_Options_CustomFpsSet", ConfigManager.CustomFpsLimit.Value));
                Close();
            }

            var textbox = AddTextInput(null, ConfigManager.CustomFpsLimit.Value.ToString(), maxCharacters: 16, initiallyFocused: true, onSubmit: Apply);
            AddButton(LocalizationManager.Get("SkinEditor_Save"), V2ModalButtonStyle.Confirm, modal => Apply(textbox.RawText));
            AddButton(LocalizationManager.Get("SkinEditor_Cancel"), V2ModalButtonStyle.Cancel, modal => modal.Close());
        }
    }
}
