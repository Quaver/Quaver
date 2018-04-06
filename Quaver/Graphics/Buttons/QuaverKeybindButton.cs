using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Main;

namespace Quaver.Graphics.Buttons
{
    /// <inheritdoc />
    /// <summary>
    /// This class will be inherited from every button class.
    /// </summary>
    internal class QuaverKeybindButton : QuaverButton
    {
        internal QuaverTextbox QuaverTextSprite { get; set; }

        internal Keys CurrentKey { get; private set; }

        internal bool Selected { get; private set; }

        internal event EventHandler KeyChanged;

        internal QuaverKeybindButton(Vector2 ButtonSize, Keys key)
        {
            QuaverTextSprite = new QuaverTextbox()
            {
                Text = key.ToString(),
                Size = new UDim2D(ButtonSize.X, ButtonSize.Y),
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.MidCenter,
                Parent = this
            };
            Size.X.Offset = ButtonSize.X;
            Size.Y.Offset = ButtonSize.Y;
            Image = GameBase.QuaverUserInterface.BlankBox;
            QuaverTextSprite.TextColor = Color.Black;
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
        protected override void MouseOver()
        {
            HoverTargetTween = 1;
        }

        /// <summary>
        ///     This method is called when the Mouse hovers out of the button
        /// </summary>
        protected override void MouseOut()
        {
            HoverTargetTween = 0;
        }

        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        internal override void Update(double dt)
        {
            HoverCurrentTween = GraphicsHelper.Tween(HoverTargetTween, HoverCurrentTween, Math.Min(dt / 40, 1));
            //CurrentTint.R = (byte)((HoverCurrentTween * Tint.R / 255f * 0.65f + 0.35f) * 255);
            //CurrentTint.G = (byte)((HoverCurrentTween * Tint.G / 255f * 0.65f + 0.35f) * 255);
            //CurrentTint.B = (byte)((HoverCurrentTween * Tint.B / 255f * 0.65f + 0.35f) * 255);
            //Tint = CurrentTint;

            //QuaverTextSprite.Update(dt);
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
            QuaverTextSprite.Text = CurrentKey.ToString();
        }

        protected override void OnClicked()
        {
            Selected = !Selected;
            if (Selected)
            {
                Tint = Color.LightYellow;
                QuaverTextSprite.Text = "Press Key";
                HoverTargetTween = 1;
            }
        }

        protected override void OnClickedOutside()
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
