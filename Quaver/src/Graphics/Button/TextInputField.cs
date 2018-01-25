using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Logging;

using Quaver.Utility;
using Quaver.Graphics.Text;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Quaver.Input;

namespace Quaver.Graphics.Button
{
    /// <summary>
    /// This class will be inherited from every button class.
    /// </summary>
    internal class TextInputField : Button
    {
        /// <summary>
        ///     The Text box spprite
        /// </summary>
        internal TextBoxSprite TextSprite { get; set; }

        /// <summary>
        ///     The place holder text for the input field
        /// </summary>
        internal string PlaceHolderText { get; private set; }

        /// <summary>
        ///     The current text in the box
        /// </summary>
        internal StringBuilder CurrentTextField { get; private set; }

        /// <summary>
        ///     If the text input is currently selected
        /// </summary>
        internal bool Selected { get; private set; }

        /// <summary>
        ///     Determines if the field should be cleared when it is deselected
        /// </summary>
        internal bool ClearFieldWhenDeselected { get; set; }

        /// <summary>
        ///     If the text is currently highlighted for a CTRL+A operation
        /// </summary>
        private bool TextHighlighted { get; set; }

        /// <summary>
        ///     This event gets called everytime player finishes inputting into the field.
        /// </summary>
        internal event EventHandler InputFinished;

        /// <summary>
        ///     Ctor - Creates the text box
        /// </summary>
        /// <param name="ButtonSize"></param>
        /// <param name="placeHolderText"></param>
        internal TextInputField(Vector2 ButtonSize, string placeHolderText)
        {
            TextSprite = new TextBoxSprite()
            {
                Text = placeHolderText,
                Size = new UDim2(ButtonSize.X - 8, ButtonSize.Y - 4),
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.WordwrapSingleLine,
                Parent = this
            };
            Size.X.Offset = ButtonSize.X;
            Size.Y.Offset = ButtonSize.Y;
            Image = GameBase.UI.BlankBox;
            TextSprite.TextColor = Color.White;

            PlaceHolderText = placeHolderText;
            CurrentTextField = new StringBuilder();

            GameBase.GameWindow.TextInput += OnTextEntered;
        }

        /// <summary>
        ///     Current tween value of the object. Used for animation.
        /// </summary>
        private float HoverCurrentTween { get; set; }

        /// <summary>
        ///     Target tween value of the object. Used for animation.
        /// </summary>
        private float HoverTargetTween { get; set; }

        /// <summary>
        ///     Current Color/Tint of the object.
        /// </summary>
        private Color CurrentTint = Color.White;

        /// <summary>
        ///     This method is called when the mouse hovers over the button
        /// </summary>
        internal override void MouseOver()
        {
            if (!Selected)
                HoverTargetTween = 1;
        }

        /// <summary>
        ///     This method is called when the Mouse hovers out of the button
        /// </summary>
        internal override void MouseOut()
        {
            if (!Selected)
                HoverTargetTween = 0;
        }

        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        internal override void Update(double dt)
        {
            HoverCurrentTween = Util.Tween(HoverTargetTween, HoverCurrentTween, Math.Min(dt / 40, 1));
            CurrentTint.R = (byte)(((HoverCurrentTween * 0.25) + 0.15f) * 255);
            CurrentTint.G = (byte)(((HoverCurrentTween * 0.25) + 0.15f) * 255);
            CurrentTint.B = (byte)(((HoverCurrentTween * 0.25) + 0.15f) * 255);
            Tint = CurrentTint;

            // Handles CTRL+Key presses
            HandleCtrlKeybinds();

            //TextSprite.Update(dt);
            base.Update(dt);
        }

        /// <summary>
        ///     Checks for any key strokes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextEntered(object sender, TextInputEventArgs e)
        {
            if (Selected)
            {
                try
                {
                    // If the text is highlighted for a CTRL + A operation, then we need to handle that separately
                    if (TextHighlighted)
                    {
                        // Reset the text
                        CurrentTextField.Length = 0;

                        switch (e.Key)
                        {
                            // If it's one of the keys that crash you and dont have an input, just clear
                            case Keys.Back:
                            case Keys.Tab:
                            case Keys.Delete:
                                break;
                            // For all other key presses, we reset the string and append the new character
                            default:
                                CurrentTextField.Append(e.Character.ToString());
                                break;
                        }

                        TextSprite.Text = CurrentTextField.ToString();
                        TextHighlighted = false;
                        return;
                    }

                    // Handle normal key inputs
                    switch (e.Key)
                    {
                        case Keys.Back:
                            CurrentTextField.Length--;
                            TextSprite.Text = CurrentTextField.ToString();
                            break;
                        case Keys.Tab:
                            break;
                        case Keys.Delete:
                            break;
                        case Keys.Enter:
                            TextSprite.Text = CurrentTextField.ToString();

                            InputFinished?.Invoke(this, new StringInputEventArgs(CurrentTextField.ToString()));
                            CurrentTextField.Clear();
                            TextSprite.Text = PlaceHolderText;
                            UnSelect();
                            break;
                        default:
                            CurrentTextField.Append(e.Character.ToString());
                            TextSprite.Text = CurrentTextField.ToString();
                            break;
                    }
                }
                catch
                {
                    Logger.Log("could not write character: " + e.Character, LogColors.GameWarning);
                }
            }
        }

        /// <summary>
        ///  Unselects the text box
        /// </summary>
        internal void UnSelect()
        {
            Selected = false;
            HoverTargetTween = 0;

            // Clears text field to placeholder ClearFieldWhenDeselected is true
            if (ClearFieldWhenDeselected)
            {
                CurrentTextField.Clear();
                TextSprite.Text = PlaceHolderText;
            }
        }

        /// <summary>
        ///     When yoou click into the text box
        /// </summary>
        internal override void OnClicked()
        {
            Selected = !Selected;

            if (Selected)
            {
                // Clears text if ClearFieldWhenDeselected is true
                if (ClearFieldWhenDeselected)
                    TextSprite.Text = CurrentTextField.ToString();

                HoverTargetTween = 1;
            }
            base.OnClicked();
        }

        /// <summary>
        ///     When you click outside of the text box
        /// </summary>
        internal override void OnClickedOutside()
        {
            if (Selected)
                UnSelect();
        }

        internal override void Destroy()
        {
            GameBase.GameWindow.TextInput -= OnTextEntered;
            base.Destroy();
        }

        /// <summary>
        ///     Handles CTRL+Key presses
        /// </summary>
        private void HandleCtrlKeybinds()
        {
            if ((!GameBase.KeyboardState.IsKeyDown(Keys.LeftControl) && !GameBase.KeyboardState.IsKeyDown(Keys.RightControl)) || !Selected)
                return;

            // CTRL + A (Select all text)
            if (GameBase.KeyboardState.IsKeyDown(Keys.A))
            {
                // Set the text highligting to true, signifying that we are ready for to clear the input
                TextHighlighted = true;
            }

            // CTRL + BackSpace (Clear Input)
            else if (GameBase.KeyboardState.IsKeyDown(Keys.Back))
            {
                // Clear the entire input
                CurrentTextField.Length = 0;
                TextSprite.Text = CurrentTextField.ToString();
            }

            // CTRL + C (Copy)
            else if (GameBase.KeyboardState.IsKeyDown(Keys.C))
            {
                if (TextHighlighted)
                    Clipboard.SetText(TextSprite.Text);
            }

            // CTRL + V (Paste)
            else if (GameBase.KeyboardState.IsKeyDown(Keys.V))
            {
                // If the text is highlighted, then we need to replace it 
                if (TextHighlighted)
                {
                    // Don't do anything if the clip board is empty
                    if (Clipboard.GetText() == "")
                        return;

                    CurrentTextField.Length = 0;
                    // Append clipboard text
                    CurrentTextField.Append(Clipboard.GetText());
                    TextSprite.Text = CurrentTextField.ToString();
                    return;
                }

                // Normal pasting
                var oldText = CurrentTextField.ToString();

                // Append old text + new text
                CurrentTextField.Length = 0;               
                CurrentTextField.Append(oldText + Clipboard.GetText());
                TextSprite.Text = CurrentTextField.ToString();
            }
        }
    }
}
