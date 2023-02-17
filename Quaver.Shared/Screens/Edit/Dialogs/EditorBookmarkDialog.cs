using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;
using Wobble.Audio.Tracks;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorBookmarkDialog : YesNoDialog
    {
        private EditorActionManager ActionManager { get; }

        private IAudioTrack Track { get; }
        
        /// <summary>
        ///     The bookmark that's currently being edited. If none is provided in the constructor,
        ///     then the purpose of the dialog will be to add a new one.
        /// </summary>
        private BookmarkInfo EditingBookmark { get; }
        protected Textbox Textbox { get; set; }

        public EditorBookmarkDialog(EditorActionManager manager, IAudioTrack track, BookmarkInfo editingBookmark) 
            : base(editingBookmark == null ? "ADD BOOKMARK" : "EDIT BOOKMARK", "Add a custom note for your bookmark...")
        {
            ActionManager = manager;
            Track = track;
            EditingBookmark = editingBookmark;
            CreateTextbox();

            Panel.Height += 50;
            YesButton.Y = -30;
            NoButton.Y = YesButton.Y;

            YesAction += () => OnSubmit(Textbox.RawText);
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Close()
        {
            Textbox.Visible = false;
            base.Close();
        }
        
        private void CreateTextbox()
        {
            Textbox = new Textbox(new ScalableVector2(Panel.Width * 0.90f, 50), FontManager.GetWobbleFont(Fonts.LatoBlack),
                20, EditingBookmark?.Note ?? "", "Add a bookmark note...", OnSubmit)
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Y = -100,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                AlwaysFocused = true,
                AllowSubmission = false
            };

            Textbox.AddBorder(ColorHelper.HexToColor("#363636"), 2);    
        }
        
        private void OnSubmit(string note)
        {
            if (EditingBookmark != null)
            {
                ActionManager.EditBookmark(EditingBookmark, note);
                return;
            }
            
            ActionManager.AddBookmark((int)Track.Time, note);
        }
    }
}