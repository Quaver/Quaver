using System;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class ApplyModifiersToMapDialog : YesNoDialog
    {
        public ApplyModifiersToMapDialog(EditScreen screen, ModIdentifier mod) : base("APPLY TEST PLAY MODIFIER",
            $"Would like to apply the modifier: \"{mod}\" to your map?\n\n" +
            "This action is NOT reversible! Choose wisely.", () =>
            {
                screen.WorkingMap.ApplyMods(mod);

                screen.Exit(() =>
                {
                    screen.Save(true, true);
                    NotificationManager.Show(NotificationLevel.Success, "Your map has been successfully saved!");

                    return new EditScreen(screen.Map, AudioEngine.LoadMapAudioTrack(screen.Map));
                });
            })
        {
        }
    }
}