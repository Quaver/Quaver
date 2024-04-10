using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Bookmarks.Offset
{
    [MoonSharpUserData]
    public class EditorActionChangeBookmarkOffsetBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangeBookmarkOffsetBatch;
        
        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<BookmarkInfo> Bookmarks { get; }

        private int Offset { get; }

        [MoonSharpVisible(false)]
        public EditorActionChangeBookmarkOffsetBatch(EditorActionManager manager, Qua map, List<BookmarkInfo> bookmarks, int offset)
        {
            ActionManager = manager;
            WorkingMap = map;
            Bookmarks = bookmarks;
            Offset = offset;
        }
        
        [MoonSharpVisible(false)]
        public void Perform()
        {
            foreach (var bookmark in Bookmarks)
                bookmark.StartTime += Offset;
            
            WorkingMap.Sort();
            ActionManager.TriggerEvent(Type, new EditorActionChangeBookmarkOffsetBatchEventArgs(Bookmarks, Offset));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionChangeBookmarkOffsetBatch(ActionManager, WorkingMap, Bookmarks, -Offset).Perform();
    }
}