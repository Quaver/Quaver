using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Edit.Dialogs;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines;

public class DrawableEditorLineBookmarkButton : ImageButton
{
    private EditorPlayfield Playfield { get; }

    public BookmarkInfo Bookmark { get; }

    private Drawable BookmarkText { get; }

    public DrawableEditorLineBookmarkButton(EditorPlayfield playfield, BookmarkInfo bookmark, Drawable bookmarkText)
        : base(UserInterface.BlankBox)
    {
        Playfield = playfield;
        Bookmark = bookmark;
        BookmarkText = bookmarkText;
        Alpha = 0;
        Clicked += OnClicked;
        RightClicked += OnRightClicked;
    }

    protected override bool IsMouseInClickArea()
    {
        var mousePosition = Playfield.GetRelativeMousePosition();
        return ScreenRectangle.Contains(mousePosition) ||
               BookmarkText.Visible && BookmarkText.ScreenRectangle.Contains(mousePosition);
    }

    private void OnClicked(object sender, EventArgs e) =>
        DialogManager.Show(new EditorBookmarkDialog(Playfield.ActionManager, Playfield.Track, Bookmark));

    private void OnRightClicked(object sender, EventArgs e)
    {
        if (ScreenRectangle.Contains(Playfield.GetRelativeMousePosition()))
            Playfield.ActionManager.RemoveBookmark(Bookmark);
    }
}
