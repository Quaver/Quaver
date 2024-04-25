using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Bookmarks.Edit
{
    [MoonSharpUserData]
    public class EditorActionEditBookmark : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.EditBookmark;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private BookmarkInfo Bookmark { get; }

        private string NewNote { get; }

        private string OldNote { get; }

        [MoonSharpVisible(false)]
        public EditorActionEditBookmark(EditorActionManager manager, Qua workingMap, BookmarkInfo bookmark, string newNote)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Bookmark = bookmark;
            NewNote = newNote;
            OldNote = Bookmark.Note;
        }
        
        [MoonSharpVisible(false)]
        public void Perform()
        {
            Bookmark.Note = NewNote;
            ActionManager.TriggerEvent(Type, new EditorActionBookmarkEditedEventArgs(Bookmark));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionEditBookmark(ActionManager, WorkingMap, Bookmark, OldNote).Perform();
    }
}