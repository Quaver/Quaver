using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines;

public class DrawableEditorLineBookmarkButton : ImageButton
{
    private EditorPlayfield Playfield { get; }

    public BookmarkInfo Bookmark { get; }

    public DrawableEditorLineBookmarkButton(EditorPlayfield playfield, BookmarkInfo bookmark) : base(UserInterface
        .BlankBox)
    {
        Playfield = playfield;
        Bookmark = bookmark;
        Alpha = 0;
        RightClicked += OnRightClicked;
    }

    protected override bool IsMouseInClickArea() => ScreenRectangle.Contains(Playfield.GetRelativeMousePosition());

    private void OnRightClicked(object sender, EventArgs e) => Playfield.ActionManager.RemoveBookmark(Bookmark);
}
