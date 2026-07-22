using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Edit.UI.Footer.Bookmarks
{
    /// <summary>
    ///     An individual bookmark display object
    /// </summary>
    public class EditorFooterBookmark : ImageButton
    {
        private const int MaximumNoteWidth = 360;

        private EditScreen Screen { get; }
        
        private BookmarkInfo Bookmark { get; }

        private Tooltip Tooltip { get; }

        public EditorFooterBookmark(EditScreen screen, BookmarkInfo bookmark) : base(UserInterface.BlankBox)
        {
            Screen = screen;
            Bookmark = bookmark;
            Tint = GetColor();
            Tooltip = new Tooltip(Bookmark.Note, Tint, maxTextWidth: MaximumNoteWidth)
            {
                DestroyIfParentIsNull = false
            };

            Hovered += (sender, args) =>
            {
                if (string.IsNullOrEmpty(Bookmark.Note))
                    return;
                
                screen.ActivateTooltip(Tooltip);
            };
            
            LeftHover += (sender, args) => screen.DeactivateTooltip();
            RightClicked += (sender, args) => screen.ActionManager.RemoveBookmark(Bookmark);
        }

        public override void Draw(GameTime gameTime)
        {
            Tint = GetColor();
            Tooltip.Border.Tint = Tint;
            base.Draw(gameTime);
        }

        private Color GetColor() => ColorHelper.ToXnaColor(Bookmark.GetColor());

        public override void Destroy()
        {
            Tooltip.Destroy();
            base.Destroy();
        }
    }
}
