using System.Text.RegularExpressions;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Steamworks;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Dialogs.Online
{
    public class CreateUsernameDialog : YesNoDialog
    {
        /// <summary>
        /// </summary>
        protected Textbox Textbox { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public CreateUsernameDialog() : base("CREATE ONLINE ACCOUNT",
            "Please choose a username to use for your account.\n\nUsernames must be between 3 to 15 characters and may only contain\nnumbers, letters, and spaces.")
        {
            CreateTextbox();

            Panel.Height += 50;
            YesButton.Y = -30;
            NoButton.Y = YesButton.Y;

            YesButton.Clicked += (sender, args) => OnSubmit(Textbox.RawText);
        }

        /// <summary>
        /// </summary>
        private void CreateTextbox()
        {
            Textbox = new Textbox(new ScalableVector2(Panel.Width * 0.90f, 50), FontManager.GetWobbleFont(Fonts.LatoBlack),
                20, "", "Enter a username...", s => OnSubmit(s))
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Y = -100,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                AlwaysFocused = true,
                MaxCharacters = 15,
                AllowedCharacters = new Regex(@"^[a-zA-Z\s]*$")
            };

            Textbox.AddBorder(ColorHelper.HexToColor("#363636"), 2);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Close()
        {
            Textbox.Visible = false;
            base.Close();
        }

        private void OnSubmit(string s) => DialogManager.Show(new CreatingAccountDialog(s));
    }
}