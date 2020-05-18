using System;
using System.Threading;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Online
{
    public class UploadWorkshopSkinDialog : LoadingDialog
    {
        public UploadWorkshopSkinDialog(SteamWorkshopSkin skin) : base("UPLOADING SKIN", "Please wait while your skin is being uploaded...",
            () =>
            {
                skin.Upload();

                while (!skin.HasUploaded)
                    Thread.Sleep(50);
            })
        {
        }
    }
}