using System;
using System.Linq;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.V2.Options.Model;
using Quaver.Shared.Screens.V2.UI;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.Modal
{
    internal sealed class OptionsPresetManageDialog : V2ModalDialog
    {
        public OptionsPresetManageDialog(OptionsPreset preset, Action<OptionsPreset> onSaved, Action<Guid> onDeleted) : base(LocalizationManager.Get("Screen_Options_ManagePreset"), LocalizationManager.Get("Screen_Options_PresetManageDescription"))
        {
            if (preset == null)
                throw new ArgumentNullException(nameof(preset));
            if (onSaved == null)
                throw new ArgumentNullException(nameof(onSaved));
            if (onDeleted == null)
                throw new ArgumentNullException(nameof(onDeleted));

            var nameTextbox = AddTextInput(LocalizationManager.Get("Screen_Options_PresetName"), preset.Name, LocalizationManager.Get("Screen_Options_PresetName"), 64);

            AddButton(LocalizationManager.Get("Screen_Options_SavePreset"), V2ModalButtonStyle.Confirm, modal =>
                {
                    var name = nameTextbox.RawText?.Trim();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Editor_InvalidName"));
                        return;
                    }

                    if (OptionsPressetStore.LoadAll().Any(other =>
                            other.Id != preset.Id && string.Equals(other.Name, name, StringComparison.OrdinalIgnoreCase)))
                    {
                        NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_PresetNameTaken"));
                        return;
                    }

                    OptionsPreset saved;
                    try
                    {
                        saved = new OptionsPreset
                        {
                            Id = preset.Id,
                            Name = name,
                            Values = OptionsList.CapturePresetValues()
                        };
                        OptionsList.CapturePresetInputs(saved);
                        OptionsPressetStore.Save(saved);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Could not save options preset {name} ({preset.Id}): {e}", LogType.Runtime);
                        NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_PresetSaveFailed"));
                        return;
                    }

                    onSaved(saved);
                    modal.Close();
                });

            AddButton(LocalizationManager.Get("Screen_Options_DeletePreset"), V2ModalButtonStyle.Cancel, modal =>
                {
                    try
                    {
                        OptionsPressetStore.Delete(preset.Id);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Could not delete options preset {preset.Name} ({preset.Id}): {e}", LogType.Runtime);
                        NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_PresetDeleteFailed"));
                        return;
                    }

                    onDeleted(preset.Id);
                    modal.Close();
                });
        }
    }
}
