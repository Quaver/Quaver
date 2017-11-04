using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Main;
using Quaver.Utility;

namespace Quaver.Graphics
{
    class Button : Sprite
    {
        private Texture2D Mask { get; set; }
        private String Text { get; set; }
        private ButtonType ButtonType { get; set; }

        private float HoverCurrentTween { get; set; }
        private float HoverTargetTween { get; set; }

        private Color CurrentTint = Color.White;

        //Constructor
        public Button(ButtonType type)
        {
            ButtonType = type;
        }

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
            
        }

        /// <summary>
        ///     This method is called when the mouse hovers over the button
        /// </summary>
        public void MouseOver()
        {
            
        }

        /// <summary>
        ///     This method is called when the Mouse hovers out of the button
        /// </summary>
        public void MouseOut()
        {
            
        }

        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        public override void Update(double dt)
        {
            if (this.GlobalRect.Contains(GameBase.MouseState.Position)) HoverTargetTween = 1;
            else HoverTargetTween = 0;

            HoverCurrentTween = Util.Tween(HoverTargetTween, HoverCurrentTween, Math.Min(dt/30,1));
            CurrentTint.R = (byte)(HoverCurrentTween * 255);
            CurrentTint.G = (byte)(HoverCurrentTween * 255);
            CurrentTint.B = (byte)(HoverCurrentTween * 255);

            Tint = CurrentTint;

            //Do button logic
        }
    }
}
