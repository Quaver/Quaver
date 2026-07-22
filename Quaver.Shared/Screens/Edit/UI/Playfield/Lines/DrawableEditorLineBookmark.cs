using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Dialogs;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    public class DrawableEditorLineBookmark : DrawableEditorLine
    {
        private const int MaximumNoteWidth = 360;

        private EditorPlayfield Playfield { get; }
        
        public BookmarkInfo Bookmark { get;  }

        private Tooltip Tooltip { get; }

        private ImageButton ImageButton { get; }

        public DrawableEditorLineBookmark(EditorPlayfield playfield, BookmarkInfo bookmark) : base(playfield)
        {
            Playfield = playfield;
            Bookmark = bookmark;
            DrawIfOffScreen = true;

            Tooltip = new Tooltip(Bookmark.Note, GetColor(), maxTextWidth: MaximumNoteWidth)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Width + 4,
                DrawIfOffScreen = true
            };

            Tooltip.Text.DrawIfOffScreen = true;
            Tooltip.Border.DrawIfOffScreen = true;
            Tooltip.Visible = !string.IsNullOrEmpty(Bookmark.Note);

            ImageButton = new DrawableEditorLineBookmarkButton(Playfield, Bookmark)
            {
                Parent = this,
                Size = new ScalableVector2(0, 0, 1, 1),
                X = Width / 2
            };
            Image = UserInterface.BlankBox;
        }

        public override void Draw(GameTime gameTime)
        {
            Tint = GetColor();
            Tooltip.Border.Tint = Tint;

            if (Tooltip.Text.Text != Bookmark.Note)
            {
                Tooltip.ChangeText(Bookmark.Note);
                Tooltip.Visible = !string.IsNullOrEmpty(Bookmark.Note);
            }

            base.Draw(gameTime);
        }

        public override Color GetColor() => Bookmark == null
            ? Color.Yellow
            : ColorHelper.ToXnaColor(Bookmark.GetColor());

        public override string GetValue() => "";

        public override float StartTime => Bookmark.StartTime;
        
        public override void SetSize()
        {
            const int height = 4;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Height != height)
                Height = height;
        }

    }
}
