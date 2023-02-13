using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Bookmarks
{
    public class EditorActionBookmarkRemovedEventArgs : EventArgs
    {
        public BookmarkInfo Bookmark { get;  }

        public EditorActionBookmarkRemovedEventArgs(BookmarkInfo bookmark) => Bookmark = bookmark;
    }
}