using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    public class DrawableEditorLineBookmark : DrawableEditorLine
    {
        public BookmarkInfo Bookmark { get;  }

        private Tooltip Tooltip { get; }

        public DrawableEditorLineBookmark(EditorPlayfield playfield, BookmarkInfo bookmark) : base(playfield)
        {
            Bookmark = bookmark;

            Tooltip = new Tooltip(Bookmark.Note, Color.Yellow, false)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Width + 4
            };

            Image = UserInterface.BlankBox;
        }

        public override void Draw(GameTime gameTime)
        {
            DrawToSpriteBatch();

            if (string.IsNullOrEmpty(Bookmark.Note)) 
                return;
            
            Tooltip.Draw(gameTime);
            Tooltip.Border.Draw(gameTime);
            Tooltip.ChangeText(Bookmark.Note);
            Tooltip.Text.Draw(gameTime);
        }

        public override Color GetColor() => Color.Yellow;

        public override string GetValue() => "";

        public override int GetTime() => Bookmark.StartTime;
        
        public override void SetSize()
        {
            const int height = 4;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Height != height)
                Height = height;
        }
    }
}