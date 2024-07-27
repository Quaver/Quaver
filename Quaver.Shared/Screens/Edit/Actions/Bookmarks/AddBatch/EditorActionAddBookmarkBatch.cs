using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Helpers;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.RemoveBatch;

namespace Quaver.Shared.Screens.Edit.Actions.Bookmarks.AddBatch
{
    [MoonSharpUserData]
    public class EditorActionAddBookmarkBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddBookmarkBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public List<BookmarkInfo> Bookmarks { get; }

        [MoonSharpVisible(false)]
        public EditorActionAddBookmarkBatch(EditorActionManager manager, Qua workingMap, List<BookmarkInfo> bookmarks)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Bookmarks = bookmarks;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            WorkingMap.Bookmarks.InsertSorted(Bookmarks);
            ActionManager.TriggerEvent(Type, new EditorActionBookmarkBatchAddedEventArgs(Bookmarks));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveBookmarkBatch(ActionManager, WorkingMap, Bookmarks).Perform();
    }
}
