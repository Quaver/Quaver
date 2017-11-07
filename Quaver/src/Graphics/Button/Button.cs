using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Utility;

namespace Quaver.Graphics.Button
{
    internal class Button : Sprite
    {
        //Mouse Over
        private bool MouseHovered { get; set; }
        private float HoverCurrentTween { get; set; }
        private float HoverTargetTween { get; set; }

        //Mouse Click
        private bool MouseClicked { get; set; }
        private double MouseDownDuration { get; set; }
        public event EventHandler Clicked;

        private Color CurrentTint = Color.White;

        /// <summary>
        ///     This method draws the button.
        /// </summary>
        public override void Draw()
        {
            base.Draw();
        }

        /// <summary>
        ///     This method is called when the button gets clicked
        /// </summary>
        public void OnClicked()
        {
            MouseClicked = true;
            Clicked(this, null);
        }

        /// <summary>
        ///     This method is called when the mouse hovers over the button
        /// </summary>
        public void MouseOver()
        {
            MouseHovered = true;
            HoverTargetTween = 1;
        }

        /// <summary>
        ///     This method is called when the Mouse hovers out of the button
        /// </summary>
        public void MouseOut()
        {
            MouseHovered = false;
            HoverTargetTween = 0;
        }

        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        public override void Update(double dt)
        {
            if (this.GlobalRect.Contains(GameBase.MouseState.Position))
            {
                //Animation
                if (!MouseHovered) MouseOver();

                //Click logic
                if (GameBase.MouseState.LeftButton == ButtonState.Pressed)
                {
                    if (!MouseClicked) OnClicked();
                }
                else
                {
                    if (MouseClicked) MouseClicked = false;
                }
            }
            else
            {
                //Animation
                if (MouseHovered) MouseOut();

                //Click logic
                if (MouseClicked) MouseClicked = false;
            }

            HoverCurrentTween = Util.Tween(HoverTargetTween, HoverCurrentTween, Math.Min(dt/40,1));
            CurrentTint.R = (byte)((HoverCurrentTween * 0.25 + 0.75f) * 255);
            CurrentTint.G = (byte)((HoverCurrentTween * 0.25 + 0.75f) * 255);
            CurrentTint.B = (byte)((HoverCurrentTween * 0.25 + 0.75f) * 255);

            Tint = CurrentTint;

            //Do button logic
        }
    }
}
