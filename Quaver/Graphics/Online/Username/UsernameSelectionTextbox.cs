using Microsoft.Xna.Framework;
using Quaver.Resources;
using Quaver.Online;
using Quaver.Scheduling;
using Quaver.Screens.Menu.UI.Navigation.User;
using Steamworks;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;

namespace Quaver.Graphics.Online.Username
{
    public class UsernameSelectionTextbox : Textbox
    {
        /// <summary>
        ///     The overlay for the textbox.
        /// </summary>
        private BorderedTextButton SubmitButton { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public UsernameSelectionTextbox()
            : base(new ScalableVector2(360, 39), BitmapFonts.Exo2Regular, 14, "", "Enter Username")
        {
            Image = UserInterface.BlankBox;
            Tint = Color.Transparent;
            AddBorder(Color.White, 2);
            InputText.Alignment = Alignment.MidLeft;
            Cursor.Y = 10;
            AlwaysFocused = true;
            MaxCharacters = 15;

            SubmitButton = new BorderedTextButton("Submit", Colors.MainAccent, (o, e) => OnBoxSubmitted(RawText))
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(100, Height),
                X = 361,
                Text =
                {
                    FontSize = 13
                }
            };

            X -= SubmitButton.Width / 2f;

            OnSubmit += OnBoxSubmitted;
            OnStoppedTyping += OnUserStoppedTyping;
        }

        /// <summary>
        ///     Handles the username selection process
        /// </summary>
        /// <param name="text"></param>
        private void OnBoxSubmitted(string text)
        {
            DialogManager.Dismiss();

            ThreadScheduler.Run(() =>
            {
                OnlineManager.Client.ChooseUsername(text, SteamUser.GetSteamID().m_SteamID, SteamFriends.GetPersonaName(),
                    SteamManager.PTicket, SteamManager.PcbTicket);
            });
        }

        /// <summary>
        ///     Handles the username availability check.
        /// </summary>
        /// <param name="text"></param>
        private void OnUserStoppedTyping(string text)
        {
        }
    }
}