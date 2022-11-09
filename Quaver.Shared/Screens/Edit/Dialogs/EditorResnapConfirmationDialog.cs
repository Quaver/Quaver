using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorResnapConfirmationDialog : YesNoDialog
    {
        public EditorResnapConfirmationDialog(EditScreen screen, List<int> snaps, List<HitObjectInfo> notes) : base("RESNAP MAP",
            $"Are you sure you would like to perform resnap on {notes.Count} notes?")
        {
            YesAction += () =>
            {
                screen.ActionManager.ResnapNotes(snaps, notes);
            };
        }
    }
}