using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Multi.UI.Status.Sharing
{
    public class ShareMultiplayerMapsetConfirmDialog : YesNoDialog
    {
        public ShareMultiplayerMapsetConfirmDialog() : base(MultiLocalization.Get("UploadUnsubmittedMapsetTitle"),
            MultiLocalization.Get("UploadUnsubmittedMapsetConfirmMessage"),
            () => DialogManager.Show(new UploadMultiplayerMapsetLoadingDialog()))
        {
            YesButton.Image = UserInterface.MultiplayerUploadMapset;
        }
    }
}
