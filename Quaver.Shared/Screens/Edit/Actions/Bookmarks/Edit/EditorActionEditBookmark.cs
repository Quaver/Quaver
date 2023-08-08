using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Bookmarks.Edit
{
    public class EditorActionEditBookmark : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.EditBookmark;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private BookmarkInfo Bookmark { get; }

        private string NewNote { get; }

        private string OldNote { get; }

        public EditorActionEditBookmark(EditorActionManager manager, Qua workingMap, BookmarkInfo bookmark, string newNote)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Bookmark = bookmark;
            NewNote = newNote;
            OldNote = Bookmark.Note;
        }
        
        public void Perform()
        {
            Bookmark.Note = NewNote;
            ActionManager.TriggerEvent(Type, new EditorActionBookmarkEditedEventArgs(Bookmark));
        }

        public void Undo() => new EditorActionEditBookmark(ActionManager, WorkingMap, Bookmark, OldNote).Perform();
    }
}