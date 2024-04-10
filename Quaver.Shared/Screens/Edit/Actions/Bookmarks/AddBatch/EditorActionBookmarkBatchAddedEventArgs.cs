using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Bookmarks.AddBatch
{
    public class EditorActionBookmarkBatchAddedEventArgs : EventArgs
    {
        public List<BookmarkInfo> Bookmarks { get; }

        public EditorActionBookmarkBatchAddedEventArgs(List<BookmarkInfo> bookmarks) => Bookmarks = bookmarks;
    }
}