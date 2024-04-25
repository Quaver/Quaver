using System.Collections.Generic;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.AddBatch;

namespace Quaver.Shared.Screens.Edit.Actions.Bookmarks.RemoveBatch
{
    public class EditorActionBookmarkBatchRemovedEventArgs : EditorActionBookmarkBatchAddedEventArgs
    {
        public EditorActionBookmarkBatchRemovedEventArgs(List<BookmarkInfo> bookmarks) : base(bookmarks)
        {
        }
    }
}