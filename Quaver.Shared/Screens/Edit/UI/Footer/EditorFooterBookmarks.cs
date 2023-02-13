using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Window;

namespace Quaver.Shared.Screens.Edit.UI.Footer
{
    /// <summary>
    ///     Container for displaying bookmarks across the width of the screen
    /// </summary>
    public class EditorFooterBookmarks : Container
    {
        private EditScreen Screen { get; }

        private Dictionary<BookmarkInfo, EditorFooterBookmark> Bookmarks { get; } = new Dictionary<BookmarkInfo, EditorFooterBookmark>();

        public EditorFooterBookmarks(EditScreen screen)
        {
            Screen = screen;
            Size = new ScalableVector2(WindowManager.Width, 14);
            CreateBookmarks();

            Screen.ActionManager.BookmarkAdded += OnBookmarkAdded;
            Screen.ActionManager.BookmarkRemoved += OnBookmarkRemoved;
        }
        
        public override void Destroy()
        {
            Screen.ActionManager.BookmarkAdded -= OnBookmarkAdded;
            Screen.ActionManager.BookmarkRemoved -= OnBookmarkRemoved;
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
    }

    /// <summary>
    ///     An individual bookmark display object
    /// </summary>
    public class EditorFooterBookmark : ImageButton
    {
        private EditScreen Screen { get; }
        private BookmarkInfo Bookmark { get; }

        private Tooltip Tooltip { get; }

        public EditorFooterBookmark(EditScreen screen, BookmarkInfo bookmark) : base(UserInterface.BlankBox)
        {
            Screen = screen;
            Bookmark = bookmark;
            SetColor();
            Tooltip = new Tooltip(Bookmark.Note, Tint) { DestroyIfParentIsNull = false };

            Hovered += (sender, args) =>
            {
                if (string.IsNullOrEmpty(Bookmark.Note))
                    return;
                
                screen.ActivateTooltip(Tooltip);
            };
            
            LeftHover += (sender, args) => screen.DeactivateTooltip();
            RightClicked += (sender, args) => screen.ActionManager.RemoveBookmark(Bookmark);
        }

        public override void Destroy()
        {
            Tooltip.Destroy();
            base.Destroy();
        }

        private void SetColor() => Tint = string.IsNullOrEmpty(Bookmark.Note) ? Colors.MainBlue : Color.Yellow;
    }
}