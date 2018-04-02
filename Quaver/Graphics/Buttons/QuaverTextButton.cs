using System;
using Microsoft.Xna.Framework;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;

namespace Quaver.Graphics.Buttons
{
    /// <inheritdoc />
    /// <summary>
    ///     This type of button is used for simple buttons that only require a single image + text, but also includes a tint animation.
    /// </summary>
    internal class QuaverTextButton : QuaverButton
    {
        internal QuaverTextbox QuaverTextSprite { get; set; }

        //Constructor
        internal QuaverTextButton(Vector2 ButtonSize, string ButtonText)
        {
            QuaverTextSprite = new QuaverTextbox()
            {
                Text = ButtonText,
                Size = new UDim2D(ButtonSize.X, ButtonSize.Y),
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.MidCenter,
                Parent = this
            };
            Size.X.Offset = ButtonSize.X;
            Size.Y.Offset = ButtonSize.Y;
            Image = GameBase.QuaverUserInterface.BlankBox;
            QuaverTextSprite.TextColor = Color.Black;
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
            HoverCurrentTween = GraphicsHelper.Tween(HoverTargetTween, HoverCurrentTween, Math.Min(dt / 40, 1));
            CurrentTint.R = (byte)(((HoverCurrentTween * 0.25) + 0.75f) * 255);
            CurrentTint.G = (byte)(((HoverCurrentTween * 0.25) + 0.75f) * 255);
            CurrentTint.B = (byte)(((HoverCurrentTween * 0.25) + 0.75f) * 255);
            Tint = CurrentTint;
            
            //QuaverTextSprite.Update(dt);
            base.Update(dt);
        }
    }
}
