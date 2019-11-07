using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form;
using Wobble.Bindables;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Graphics.Overlays.Hub.SongRequests.Header
{
    public class DisplayAlertsCheckbox : LabelledCheckbox
    {
        public DisplayAlertsCheckbox() : base("DISPLAY Alerts:".ToUpper(), 20,
            new QuaverCheckbox(ConfigManager.DisplaySongRequestNotifications ?? new Bindable<bool>(false)))
        {
        }
    }
}