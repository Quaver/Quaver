using System;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class ApplyModifiersToMapDialog : YesNoDialog
    {
        public ApplyModifiersToMapDialog(EditScreen screen, ModIdentifier mod) : base(
            LocalizationManager.Get("Screen_Editor_ApplyModifier"),
            LocalizationManager.Get("Screen_Editor_ApplyModifierConfirmation",
                LocalizationManager.Get("Screen_Editor_Modifier_" + mod)), () =>
            {
                screen.WorkingMap.ApplyMods(mod);

                screen.Exit(() =>
                {
                    screen.Save(true, true);
                    NotificationManager.Show(NotificationLevel.Success,
                        LocalizationManager.Get("Screen_Editor_MapSavedSuccessfully"));

                    return new EditScreen(screen.Map, AudioEngine.LoadMapAudioTrack(screen.Map));
                });
            })
        {
        }
    }
}
