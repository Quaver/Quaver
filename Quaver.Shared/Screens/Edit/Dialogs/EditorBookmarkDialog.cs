using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
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

        protected Textbox Textbox { get; set; }

        public EditorBookmarkDialog(EditScreen screen) : base("ADD BOOKMARK", "Add a custom note for your bookmark...")
        {
            Screen = screen;
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
                20, "", "Add a bookmark note...", OnSubmit)
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
        
        private void OnSubmit(string note) => Screen.ActionManager.AddBookmark((int)Screen.Track.Time, note);
    }
}