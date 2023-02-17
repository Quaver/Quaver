using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Bookmarks.Offset
{
    public class EditorActionChangeBookmarkOffsetBatchEventArgs : EventArgs
    {
        public List<BookmarkInfo> Bookmarks { get; }

        public int Offset { get; }

        public EditorActionChangeBookmarkOffsetBatchEventArgs(List<BookmarkInfo> bookmarks, int offset)
        {
            Bookmarks = bookmarks;
            Offset = offset;
        }
    }
}