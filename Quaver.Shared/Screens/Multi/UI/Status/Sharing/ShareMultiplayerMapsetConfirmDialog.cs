using System;
using System.Threading;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Multi.UI.Status
{
    public class ShareMultiplayerMapsetConfirmDialog : YesNoDialog
    {
        public ShareMultiplayerMapsetConfirmDialog() : base("UPLOAD UNSUBMITTED MAPSET",
            "Would you like to temporarily upload the current mapset\nfor other players to download?",
            () => DialogManager.Show(new UploadMultiplayerMapsetLoadingDialog()))
        {
            YesButton.Image = UserInterface.MultiplayerUploadMapset;
        }
    }
}