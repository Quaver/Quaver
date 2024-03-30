using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.AddBatch;

namespace Quaver.Shared.Screens.Edit.Actions.Bookmarks.RemoveBatch
{
    [MoonSharpUserData]
    public class EditorActionRemoveBookmarkBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RemoveBookmarkBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<BookmarkInfo> Bookmarks { get; }

        [MoonSharpVisible(false)]
        public EditorActionRemoveBookmarkBatch(EditorActionManager manager, Qua workingMap, List<BookmarkInfo> bookmarks)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Bookmarks = bookmarks;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            foreach (var sv in Bookmarks)
                WorkingMap.Bookmarks.Remove(sv);

            ActionManager.TriggerEvent(Type, new EditorActionBookmarkBatchRemovedEventArgs(Bookmarks));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionAddBookmarkBatch(ActionManager, WorkingMap, Bookmarks).Perform();
    }
}
