using System;
using System.Threading;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Online
{
    public class UploadWorkshopSkinDialog : LoadingDialog
    {
        public UploadWorkshopSkinDialog(SteamWorkshopItem item) : base("UPLOADING SKIN", "Please wait while your skin is being uploaded...",
            () =>
            {
                item.Upload();

                while (!item.HasUploaded)
                    Thread.Sleep(50);
            })
        {
        }
    }
}