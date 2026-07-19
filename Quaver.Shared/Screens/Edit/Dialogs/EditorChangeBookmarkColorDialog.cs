using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;

namespace Quaver.Shared.Screens.Edit.Dialogs;

public class EditorChangeBookmarkColorDialog : ColorDialog
{
    private EditorActionManager ActionManager { get; }

    private List<BookmarkInfo> Bookmarks { get; }

    public EditorChangeBookmarkColorDialog(List<BookmarkInfo> bookmarks, EditorActionManager actionManager)
        : base("CHANGE BOOKMARK COLOR", "Enter a new RGB color for the selected bookmarks...")
    {
        Bookmarks = bookmarks;
        ActionManager = actionManager;

        if (Bookmarks.Count != 0)
            UpdateColor(ColorHelper.ToXnaColor(Bookmarks[0].GetColor()));
    }

    protected override void OnColorChange(Color newColor) =>
        ActionManager.ChangeBookmarkColorBatch(Bookmarks, newColor);
}
