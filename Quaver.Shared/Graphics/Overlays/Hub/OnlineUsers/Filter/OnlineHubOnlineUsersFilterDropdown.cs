using System.Collections.Generic;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Filter
{
    public class OnlineHubOnlineUsersFilterDropdown : LabelledDropdown
    {
        public OnlineHubOnlineUsersFilterDropdown() : base("Filter: ".ToUpper(), 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(124, 30), 22, ColorHelper.HexToColor($"#10C8F6"), GetSelectedIndex()))
        {
            Dropdown.ItemSelected += (sender, args) =>
            {
                if (ConfigManager.OnlineUserListFilterType == null)
                    return;

                ConfigManager.OnlineUserListFilterType.Value = (OnlineUserListFilter) args.Index;
            };
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>
        {
            "All",
            "Friends",
            "Country",
        };

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            if (ConfigManager.OnlineUserListFilterType == null)
                return 0;

            return (int) ConfigManager.OnlineUserListFilterType.Value;
        }
    }
}