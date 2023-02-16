using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Bookmarks.Offset
{
    public class EditorActionChangeBookmarkOffsetBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangeBookmarkOffsetBatch;
        
        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<BookmarkInfo> Bookmarks { get; }

        private int Offset { get; }

        public EditorActionChangeBookmarkOffsetBatch(EditorActionManager manager, Qua map, List<BookmarkInfo> bookmarks, int offset)
        {
            ActionManager = manager;
            WorkingMap = map;
            Bookmarks = bookmarks;
            Offset = offset;
        }
        
        public void Perform()
        {
            foreach (var bookmark in Bookmarks)
                bookmark.StartTime += Offset;
            
            WorkingMap.Sort();
            ActionManager.TriggerEvent(Type, new EditorActionChangeBookmarkOffsetBatchEventArgs(Bookmarks, Offset));
        }

        public void Undo() => new EditorActionChangeBookmarkOffsetBatch(ActionManager, WorkingMap, Bookmarks, -Offset).Perform();
    }
}