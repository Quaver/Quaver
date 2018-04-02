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
    internal class KeyBindButton : Button
    {
        internal TextBoxSprite TextSprite { get; set; }

        internal Keys CurrentKey { get; private set; }

        internal bool Selected { get; private set; }

        internal event EventHandler KeyChanged;

        internal KeyBindButton(Vector2 ButtonSize, Keys key)
        {
            TextSprite = new TextBoxSprite()
            {
                Text = key.ToString(),
                Size = new UDim2(ButtonSize.X, ButtonSize.Y),
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.MidCenter,
                Parent = this
            };
            Size.X.Offset = ButtonSize.X;
            Size.Y.Offset = ButtonSize.Y;
            Image = GameBase.UI.BlankBox;
            TextSprite.TextColor = Color.Black;
            Tint = Color.LightPink;
            CurrentKey = key;

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
            //CurrentTint.R = (byte)((HoverCurrentTween * Tint.R / 255f * 0.65f + 0.35f) * 255);
            //CurrentTint.G = (byte)((HoverCurrentTween * Tint.G / 255f * 0.65f + 0.35f) * 255);
            //CurrentTint.B = (byte)((HoverCurrentTween * Tint.B / 255f * 0.65f + 0.35f) * 255);
            //Tint = CurrentTint;

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
                    // Handle normal key inputs
                    switch (e.Key)
                    {
                        case Keys.Back:
                            break;
                        case Keys.Tab:
                            break;
                        case Keys.Delete:
                            break;
                        case Keys.Enter:
                            break;
                        default:
                            CurrentKey = e.Key;
                            KeyChanged?.Invoke(this, null);
                            break;
                    }
                    UnSelect();
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("Could not select key: " + e.Character, LogType.Runtime);
                    Logger.LogError(ex, LogType.Runtime);
                }
            }
        }

        internal void UnSelect()
        {
            Tint = Color.LightPink;
            Selected = false;
            HoverTargetTween = 0;
            TextSprite.Text = CurrentKey.ToString();
        }

        internal override void OnClicked()
        {
            Selected = !Selected;
            if (Selected)
            {
                Tint = Color.LightYellow;
                TextSprite.Text = "Press Key";
                HoverTargetTween = 1;
            }
            base.OnClicked();
        }

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
    }
}
