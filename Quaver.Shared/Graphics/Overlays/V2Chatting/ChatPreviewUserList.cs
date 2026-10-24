using System.Collections.Generic;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Overlays.V2Hub.Users;
using Quaver.Shared.Screens.V2.UI;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting;

internal sealed class ChatPreviewUserList : PoolableScrollContainer<User>
{
    public ChatPreviewUserList(List<User> users, ScalableVector2 size) : base(users, users.Count, 0, size, size)
    {
        CreatePool();
    }

    protected override PoolableSprite<User> CreateObject(User item, int index) =>
        new UserRow(this, item, index, true, V2Scrollbar.GetContentWidth(this))
        {
            Alpha = 0
        };
}
