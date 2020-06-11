using System;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online.API;
using Quaver.Shared.Online.API.Legal;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;

namespace Quaver.Shared.Graphics.Dialogs.Online
{
    public class TermsOfServiceDialog : LegalAcceptanceDialog
    {
        public TermsOfServiceDialog() : base("ACCEPT TERMS OF SERVICE", "Terms of Service", new APIRequestTOS())
        {
            YesButton.Clicked += (sender, args) =>
            {
                DialogManager.Show(new LoadingDialog("PLEASE WAIT", "Fetching privacy policy. Please wait...", () =>
                {
                    try
                    {
                        var dialog = new LegalAcceptanceDialog("PRIVACY POLICY", "Privacy Policy", new APIRequestPrivacyPolicy());
                        
                        // Send acceptance to server & login
                        dialog.YesButton.Clicked += (o, eventArgs) =>
                        {
                            Console.WriteLine("ACCEPTED TOS & PRIVACY POLICY");
                        };
                        
                        DialogManager.Show(dialog);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, LogType.Runtime);
                        NotificationManager.Show(NotificationLevel.Error, "Unable fetch privacy policy document.");
                    }
                }));
            };
        }
    }
}