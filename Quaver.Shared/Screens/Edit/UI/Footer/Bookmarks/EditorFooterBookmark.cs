using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Edit.Dialogs;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Edit.UI.Footer.Bookmarks
{
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
            Tint = Color.Yellow;
            Tooltip = new Tooltip(Bookmark.Note, Tint) { DestroyIfParentIsNull = false };

            Hovered += (sender, args) =>
            {
                if (string.IsNullOrEmpty(Bookmark.Note))
                    return;
                
                screen.ActivateTooltip(Tooltip);
            };
            
            LeftHover += (sender, args) => screen.DeactivateTooltip();
            Clicked += (sender, args) => DialogManager.Show(new EditorBookmarkDialog(Screen.ActionManager, Screen.Track, Bookmark)); 
            RightClicked += (sender, args) => screen.ActionManager.RemoveBookmark(Bookmark);
        }

        public override void Destroy()
        {
            Tooltip.Destroy();
            base.Destroy();
        }
    }
}