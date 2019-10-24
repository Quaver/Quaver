using System.Collections.Generic;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Filter
{
    public class OnlineHubOnlineUsersFilterDropdown : LabelledDropdown
    {
        public OnlineHubOnlineUsersFilterDropdown() : base("Filter: ".ToUpper(), 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(124, 30), 22, ColorHelper.HexToColor($"#10C8F6"), 0))
        {
        }

        private static List<string> GetDropdownItems()
        {
            return new List<string>
            {
                "All",
                "Friends",
                "Country",
            };
        }
    }
}