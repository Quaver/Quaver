using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
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

        public EditorFooterBookmarks(EditScreen screen)
        {
            Screen = screen;
            Size = new ScalableVector2(WindowManager.Width, 14);
            CreateBookmarks();
        }

        /// <summary>
        ///     Initializes each bookmark tick
        /// </summary>
        private void CreateBookmarks()
        {
            for (var i = 0; i < 80; i++)
            {
                var bookmark = new BookmarkInfo { StartTime = i * 1000 };

                if (i % 2 == 0)
                    bookmark.Note = "This section is for when the music gets more intense, so I put jumptrills here";
                
                new EditorFooterBookmark(bookmark)
                {
                    Parent = this,
                    X = (float) (bookmark.StartTime / Screen.Track.Length) * Width,
                    Size = new ScalableVector2(4, Height)
                };
            }
        }
    }

    /// <summary>
    ///     An individual bookmark display object
    /// </summary>
    public class EditorFooterBookmark : ImageButton
    {
        private BookmarkInfo Bookmark { get; }

        private Tooltip Tooltip { get; }

        public EditorFooterBookmark(BookmarkInfo bookmark) : base(UserInterface.BlankBox)
        {
            Bookmark = bookmark;
            SetColor();
            Tooltip = new Tooltip(Bookmark.Note, Tint) { DestroyIfParentIsNull = false };

            Hovered += (sender, args) =>
            {
                if (string.IsNullOrEmpty(Bookmark.Note))
                    return;
                
                var game = (QuaverGame) GameBase.Game;
                game.CurrentScreen?.ActivateTooltip(Tooltip);
            };

            LeftHover += (sender, args) =>
            {
                var game = (QuaverGame)GameBase.Game;
                game.CurrentScreen?.DeactivateTooltip();
            };
        }

        public override void Destroy()
        {
            Tooltip.Destroy();
            base.Destroy();
        }

        private void SetColor() => Tint = string.IsNullOrEmpty(Bookmark.Note) ? Colors.MainBlue : Color.Yellow;
    }
}