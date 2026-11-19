using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Overlays.V2Hub.Users;
using Quaver.Shared.Screens.V2.UI;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting;

internal sealed class ChatPreviewUserList : PoolableScrollContainer<User>
{
    private Color RowBackgroundColor { get; }

    private float PreviousViewportWidth { get; set; }

    private float PreviousViewportHeight { get; set; }

    public ChatPreviewUserList(List<User> users, ScalableVector2 size, Color rowBackgroundColor) : base(users, users.Count, 0, size, size)
    {
        RowBackgroundColor = rowBackgroundColor;
        CreatePool();
    }

    public void RefreshViewport()
    {
        var widthChanged = System.Math.Abs(PreviousViewportWidth - Width) > 0.001f;
        var heightChanged = System.Math.Abs(PreviousViewportHeight - Height) > 0.001f;
        if (!widthChanged && !heightChanged)
            return;

        PreviousViewportWidth = Width;
        PreviousViewportHeight = Height;

        if (widthChanged)
        {
            DestroyPool();
            CreatePool();
        }
        else
            RecalculateContainerHeight();
    }

    protected override PoolableSprite<User> CreateObject(User item, int index) =>
        new UserRow(this, item, index, RowBackgroundColor, true, V2Scrollbar.GetContentWidth(this))
        {
            Alpha = 0
        };
}
