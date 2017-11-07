using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Main;
using Quaver.Utility;

namespace Quaver.Graphics.Button
{
    internal class BasicButton : Button
    {
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
        ///     This method is called when the button gets clicked
        /// </summary>
        public override void OnClicked()
        {
            base.OnClicked();
        }

        /// <summary>
        ///     This method is called when the mouse hovers over the button
        /// </summary>
        public override void MouseOver()
        {
            HoverTargetTween = 1;
        }

        /// <summary>
        ///     This method is called when the Mouse hovers out of the button
        /// </summary>
        public override void MouseOut()
        {
            HoverTargetTween = 0;
        }

        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        public override void Update(double dt)
        {
            HoverCurrentTween = Util.Tween(HoverTargetTween, HoverCurrentTween, Math.Min(dt / 40, 1));
            CurrentTint.R = (byte)((HoverCurrentTween * 0.25 + 0.75f) * 255);
            CurrentTint.G = (byte)((HoverCurrentTween * 0.25 + 0.75f) * 255);
            CurrentTint.B = (byte)((HoverCurrentTween * 0.25 + 0.75f) * 255);
            Tint = CurrentTint;
            base.Update(dt);
        }
    }
}
