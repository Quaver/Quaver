using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Edit.Dialogs;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    public class DrawableEditorLineBookmark : DrawableEditorLine
    {
        private EditorPlayfield Playfield { get; }
        
        public BookmarkInfo Bookmark { get;  }

        private Tooltip Tooltip { get; }

        public DrawableEditorLineBookmark(EditorPlayfield playfield, BookmarkInfo bookmark) : base(playfield)
        {
            Playfield = playfield;
            Bookmark = bookmark;

            Tooltip = new Tooltip(Bookmark.Note, Color.Yellow, false)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Width + 4
            };

            Image = UserInterface.BlankBox;
            Clicked += OnClicked;
            RightClicked += OnRightClicked;
        }
        
        public override void Draw(GameTime gameTime)
        {
            Update(gameTime);
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

        protected override bool IsMouseInClickArea() => ScreenRectangle.Contains(Playfield.GetRelativeMousePosition());
        
        private void OnClicked(object sender, EventArgs e) => DialogManager.Show(new EditorBookmarkDialog(Playfield.ActionManager, Playfield.Track, Bookmark));

        private void OnRightClicked(object sender, EventArgs e) => Playfield.ActionManager.RemoveBookmark(Bookmark);
    }
}