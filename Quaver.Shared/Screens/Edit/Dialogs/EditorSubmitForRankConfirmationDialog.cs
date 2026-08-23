using System;
using Quaver.Shared.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorSubmitForRankConfirmationDialog : YesNoDialog
    {
        public EditorSubmitForRankConfirmationDialog(EditScreen screen) : base(
            LocalizationManager.Get("Screen_Editor_SubmitForRank"),
            LocalizationManager.Get("Screen_Editor_SubmitForRankConfirmation"),
            () => DialogManager.Show(new EditorSubmitForRankDialog(screen)))
        {
        }
    }
}
