using System;
using System.Text.RegularExpressions;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorBookmarkDialog : YesNoDialog
    {
        private EditScreen Screen { get; }

        private BookmarkInfo Bookmark { get; }
        
        protected Textbox Textbox { get; set; }

        public EditorBookmarkDialog(EditScreen screen, BookmarkInfo bookmark) : base(bookmark != null ? "EDIT BOOKMARK" : "ADD BOOKMARK", 
            "Add a custom note for your bookmark...")
        {
            Screen = screen;
            Bookmark = bookmark;
            CreateTextbox();

            Panel.Height += 50;
            YesButton.Y = -30;
            NoButton.Y = YesButton.Y;
            
            YesButton.Clicked += (o, e) => OnSubmit(Textbox.RawText);
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
                20, Bookmark?.Note ?? "", "Add a bookmark note...", OnSubmit)
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Y = -100,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                AlwaysFocused = true,
            };

            Textbox.AddBorder(ColorHelper.HexToColor("#363636"), 2);    
        }
        
        private void OnSubmit(string note)
        {
            if (Bookmark == null)
                Screen.ActionManager.AddBookmark((int) Screen.Track.Time, note);
            else
            {
                
            }
        }
    }
}