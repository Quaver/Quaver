using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Screens.V2.UI;
using Wobble;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting;

public sealed class ChatChannelOverflowMenu : V2Dropdown<ChatChannel>
{
    public ChatChannelOverflowMenu(float width, IReadOnlyList<ChatChannel> channels, SkinV2DropdownConfig config, Container overlayHost) : base(width, CreateEntries(channels), FontManager.GetWobbleFont(Fonts.InterBold), config, overlayHost)
    {
        MaxVisibleItems = 8;
    }

    private static IReadOnlyList<DropdownEntry<ChatChannel>> CreateEntries(IReadOnlyList<ChatChannel> channels)
    {
        var entries = new List<DropdownEntry<ChatChannel>>(channels.Count);
        foreach (var channel in channels)
        {
            entries.Add(new DropdownOption<ChatChannel>(channel, channel.GetDisplayedName(), channel.IsUnread ? ColorHelper.FromHex("#FFE032") : Color.White));
        }

        return entries;
    }
}
