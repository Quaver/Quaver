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

        public BookmarkInfo Bookmark { get; }

        public string NewNote { get; }

        public string OldNote { get; }

        public string NewColorRgb { get; }

        public string OldColorRgb { get; }

        [MoonSharpVisible(false)]
        public EditorActionEditBookmark(EditorActionManager manager, Qua workingMap, BookmarkInfo bookmark,
            string newNote, string newColorRgb = null)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Bookmark = bookmark;
            NewNote = newNote;
            OldNote = Bookmark.Note;
            NewColorRgb = newColorRgb ?? Bookmark.ColorRgb;
            OldColorRgb = Bookmark.ColorRgb;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            Bookmark.Note = NewNote;
            Bookmark.ColorRgb = NewColorRgb;
            ActionManager.TriggerEvent(Type, new EditorActionBookmarkEditedEventArgs(Bookmark));
        }

        [MoonSharpVisible(false)]
        public void Undo() =>
            new EditorActionEditBookmark(ActionManager, WorkingMap, Bookmark, OldNote, OldColorRgb).Perform();
    }
}
