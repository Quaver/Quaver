using System;
using Quaver.Shared.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorSubmitForRankConfirmationDialog : YesNoDialog
    {
        public EditorSubmitForRankConfirmationDialog(EditScreen screen) : base("SUBMIT FOR RANK",
            "Are you sure you would like to submit your mapset for rank?\nNote: It must follow the ranking criteria in order to be accepted.",
            () => DialogManager.Show(new EditorSubmitForRankDialog(screen)))
        {
        }
    }
}