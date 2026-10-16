using System;
using System.Linq;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.V2.Options.Model;
using Quaver.Shared.Screens.V2.UI;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.Modal
{
    internal sealed class OptionsPresetCreateDialog : ModalDialog
    {
        public OptionsPresetCreateDialog(Action<OptionsPreset> onCreated) : base(LocalizationManager.Get("Screen_Options_CreatePreset"), LocalizationManager.Get("Screen_Options_PresetDescription"))
        {
            if (onCreated == null)
                throw new ArgumentNullException(nameof(onCreated));

            var nameTextbox = AddTextInput(LocalizationManager.Get("Screen_Options_PresetName"), maxCharacters: 16, initiallyFocused: true);

            AddButton(LocalizationManager.Get("Screen_Options_CreatePreset"), V2ModalButtonStyle.Confirm, modal =>
                {
                    var presetName = nameTextbox.RawText?.Trim();
                    if (string.IsNullOrWhiteSpace(presetName))
                    {
                        NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Editor_InvalidName"));
                        return;
                    }

                    if (OptionsPressetStore.LoadAll().Any(preset => string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase)))
                    {
                        NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_PresetNameTaken"));
                        return;
                    }

                    OptionsPreset preset;
                    try
                    {
                        preset = new OptionsPreset
                        {
                            Id = Guid.NewGuid(),
                            Name = presetName,
                            Values = OptionsList.CapturePresetValues()
                        };
                        OptionsList.CapturePresetInputs(preset);
                        OptionsPressetStore.Save(preset);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Could not create options preset: {e}", LogType.Runtime);
                        NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_PresetSaveFailed"));
                        return;
                    }

                    onCreated(preset);
                    modal.Close();
                });

            AddButton(LocalizationManager.Get("SkinEditor_Cancel"), V2ModalButtonStyle.Cancel, modal => modal.Close());
        }
    }
}
