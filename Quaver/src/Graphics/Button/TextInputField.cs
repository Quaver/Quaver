using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Logging;

using Quaver.Utility;
using Quaver.Graphics.Text;

namespace Quaver.Graphics.Button
{
    /// <summary>
    /// This class will be inherited from every button class.
    /// </summary>
    internal class TextInputField : Button
    {
        internal TextBoxSprite TextSprite { get; set; }

        internal string PlaceHolderText { get; private set; }

        internal StringBuilder CurrentTextField { get; private set; }

        internal TextInputField(Vector2 ButtonSize, string placeHolderText)
        {
            TextSprite = new TextBoxSprite()
            {
                Text = placeHolderText,
                Size = new UDim2(ButtonSize.X - 4, ButtonSize.Y - 4),
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
            HoverTargetTween = 1;
        }

        /// <summary>
        ///     This method is called when the Mouse hovers out of the button
        /// </summary>
        internal override void MouseOut()
        {
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

            //TextSprite.Update(dt);
            base.Update(dt);
        }

        private void OnTextEntered(object sender, TextInputEventArgs e)
        {
            try
            {
                CurrentTextField.Append(e.Character.ToString());
                TextSprite.Text = CurrentTextField.ToString();
            }
            catch
            {
                Logger.Log("could not write character: " + e.Character, LogColors.GameWarning);
            }
        }

        internal override void Destroy()
        {
            GameBase.GameWindow.TextInput -= OnTextEntered;
            base.Destroy();
        }
    }
}
