using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multi.UI.Status.Sharing
{
    public class ShareMultiplayerMapsetConfirmDialog : YesNoDialog
    {
        public ShareMultiplayerMapsetConfirmDialog() : base(MultiLocalization.Get("UploadUnsubmittedMapsetTitle"),
            MultiLocalization.Get("UploadUnsubmittedMapsetConfirmMessage"),
            () => DialogManager.Show(new UploadMultiplayerMapsetLoadingDialog()))
        {
            YesButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "UPLOAD MAPSET", 20, Color.White);
        }
    }
}
