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

namespace Quaver.Graphics.Button
{
    /// <summary>
    ///     This type of button is used for simple buttons that only require a single image + text, but also includes a tint animation.
    /// </summary>
    internal class TextButton : Button
    {

        public string Text { get; set; }
        public SpriteFont Font { get; } = GameBase.Content.Load<SpriteFont>("testFont");
        public Color TextColor = Color.White;

        public TextSprite TextSprite { get; set; }

        //Constructor
        public TextButton()
        {
            TextSprite = new TextSprite()
            {
                Text = this.Text,
                Size = this.Size,
                Parent = this
            };
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

        public override void Draw()
        {
            base.Draw();
            TextSprite.Draw();
            //TODO: Use a text sprite after
            //GameBase.SpriteBatch.DrawString(Font, Text, new Vector2(GlobalRect.X + 40, GlobalRect.Y + 5), TextColor);
        }
    }
}
