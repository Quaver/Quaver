using System;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;

namespace Quaver.Screens.Connecting.UI
{
    public class UsernameSelectionTextbox : Textbox
    {
        /// <summary>
        ///     The overlay for the textbox.
        /// </summary>
        private TextButton SubmitButton { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public UsernameSelectionTextbox()
            : base(TextboxStyle.SingleLine, new ScalableVector2(360, 40), Fonts.Exo2Regular24, "", "Enter Username", 0.60f)
        {
            Image = UserInterface.UsernameSelectionTextbox;
            InputText.Y = 5;
            Cursor.Y = 6;
            AlwaysFocused = true;

            SubmitButton = new TextButton(UserInterface.BlankBox, Fonts.Exo2Regular24, "Submit", 0.55f, (o, e) => OnBoxSubmitted(RawText))
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(100, Height),
                X = 360,
                Tint = Colors.MainAccent,
                Alpha = 0.65f
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
            Console.WriteLine("Text submitted with: " + text);
        }

        /// <summary>
        ///     Handles the username availability check.
        /// </summary>
        /// <param name="text"></param>
        private void OnUserStoppedTyping(string text)
        {
            Console.WriteLine("Stopped typing with: " + text);
        }
    }
}