using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Remove;

namespace Quaver.Shared.Screens.Edit.Actions.Bookmarks.Add
{
    [MoonSharpUserData]
    public class EditorActionAddBookmark : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddBookmark;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public BookmarkInfo Bookmark { get; }

        [MoonSharpVisible(false)]
        public EditorActionAddBookmark(EditorActionManager manager, Qua map, BookmarkInfo bookmark)
        {
            ActionManager = manager;
            WorkingMap = map;
            Bookmark = bookmark;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            WorkingMap.Bookmarks.InsertSorted(Bookmark);
            ActionManager.TriggerEvent(Type, new EditorActionBookmarkAddedEventArgs(Bookmark));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveBookmark(ActionManager, WorkingMap, Bookmark).Perform();
    }
}
