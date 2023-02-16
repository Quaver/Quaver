using System.Collections.Generic;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Add;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Edit;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Offset;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Remove;
using Wobble.Graphics;
using Wobble.Window;

namespace Quaver.Shared.Screens.Edit.UI.Footer.Bookmarks
{
    /// <summary>
    ///     Container for displaying bookmarks across the width of the screen
    /// </summary>
    public class EditorFooterBookmarkContainer : Container
    {
        private EditScreen Screen { get; }

        private Dictionary<BookmarkInfo, EditorFooterBookmark> Bookmarks { get; } = new Dictionary<BookmarkInfo, EditorFooterBookmark>();

        public EditorFooterBookmarkContainer(EditScreen screen)
        {
            Screen = screen;
            Size = new ScalableVector2(WindowManager.Width, 14);
            CreateBookmarks();

            Screen.ActionManager.BookmarkAdded += OnBookmarkAdded;
            Screen.ActionManager.BookmarkRemoved += OnBookmarkRemoved;
            Screen.ActionManager.BookmarkEdited += OnBookmarkEdited;
            Screen.ActionManager.BookmarkBatchOffsetChanged += OnBookmarkBatchOffsetChanged;
        }
        
        public override void Destroy()
        {
            Screen.ActionManager.BookmarkAdded -= OnBookmarkAdded;
            Screen.ActionManager.BookmarkRemoved -= OnBookmarkRemoved;
            Screen.ActionManager.BookmarkEdited -= OnBookmarkEdited;
            Screen.ActionManager.BookmarkBatchOffsetChanged -= OnBookmarkBatchOffsetChanged;
            base.Destroy();
        }

        /// <summary>
        ///     Initializes each bookmark tick
        /// </summary>
        private void CreateBookmarks()
        {
            foreach (var bookmark in Screen.WorkingMap.Bookmarks)
                AddBookmark(bookmark);
        }

        private void AddBookmark(BookmarkInfo bookmark)
        {
            Bookmarks.Add(bookmark, new EditorFooterBookmark(Screen, bookmark)
            {
                Parent = this,
                X = (float) (bookmark.StartTime / Screen.Track.Length) * Width,
                Size = new ScalableVector2(4, Height)
            });
        }

        private void RemoveBookmark(BookmarkInfo bookmark)
        {
            if (!Bookmarks.ContainsKey(bookmark))
                return;

            var bm = Bookmarks[bookmark];
            Bookmarks.Remove(bookmark);
            bm.Destroy();
        }
        
        private void OnBookmarkAdded(object sender, EditorActionBookmarkAddedEventArgs e) => AddBookmark(e.Bookmark);
        private void OnBookmarkRemoved(object sender, EditorActionBookmarkRemovedEventArgs e) => RemoveBookmark(e.Bookmark);
        
        private void OnBookmarkEdited(object sender, EditorActionBookmarkEditedEventArgs e)
        {
            RemoveBookmark(e.Bookmark);
            AddBookmark(e.Bookmark);
        }
        
        private void OnBookmarkBatchOffsetChanged(object sender, EditorActionChangeBookmarkOffsetBatchEventArgs e)
        {
            foreach (var bookmark in e.Bookmarks)
            {
                RemoveBookmark(bookmark);
                AddBookmark(bookmark);
            }
        }
    }
}