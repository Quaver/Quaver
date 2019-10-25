using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Filter
{
    public class OnlineHubOnlineUsersFilterPanel : Sprite
    {
        /// <summary>
        /// </summary>
        private OnlineHubOnlineUserSearchBox SearchBox { get; set; }

        /// <summary>
        /// </summary>
        public OnlineHubOnlineUsersFilterDropdown FilterDropdown { get; private set; }

        /// <summary>
        /// </summary>
        public Bindable<string> CurrentSearchQuery { get; }

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public OnlineHubOnlineUsersFilterPanel(ScalableVector2 size)
        {
            Size = size;
            CurrentSearchQuery = new Bindable<string>("") { Value = ""};

            CreateSearchBox();
            CreateFilterDropdown();
        }

        /// <summary>
        /// </summary>
        private void CreateSearchBox()
        {
            SearchBox = new OnlineHubOnlineUserSearchBox(CurrentSearchQuery, new ScalableVector2(200, 30))
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 12
            };
        }

        /// <summary>
        /// </summary>
        private void CreateFilterDropdown()
        {
            FilterDropdown = new OnlineHubOnlineUsersFilterDropdown()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -SearchBox.X
            };
        }
    }
}