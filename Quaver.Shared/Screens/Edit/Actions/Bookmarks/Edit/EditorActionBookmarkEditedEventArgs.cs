using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Bookmarks.Edit
{
    public class EditorActionBookmarkEditedEventArgs : EventArgs
    {
        public BookmarkInfo Bookmark { get; }

        public EditorActionBookmarkEditedEventArgs(BookmarkInfo bm) => Bookmark = bm;
    }
}