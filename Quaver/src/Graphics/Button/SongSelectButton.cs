using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Graphics.Text;

using Quaver.Utility;
using Quaver.Database.Beatmaps;

namespace Quaver.Graphics.Button
{
    /// <summary>
    ///     This type of button is used for simple buttons that only require a single image + text, but also includes a tint animation.
    /// </summary>
    internal class SongSelectButton : Button
    {
        public TextBoxSprite TitleText { get; set; }

        public TextBoxSprite ArtistText { get; set; }

        //Constructor
        public SongSelectButton(Beatmap map, float ButtonScale) //Vector2 ButtonSize, string ButtonText)
        {
            var ButtonSizeY = 40 * ButtonScale;
            var mapText = map.Artist + " - " + map.Title + " [" + map.DifficultyName + "]";

            SizeY = ButtonSizeY;
            SizeX = ButtonSizeY * 8;

            TitleText = new TextBoxSprite()
            {
                Text = map.Title,
                Font = Fonts.Medium12,
                ScaleY = 0.5f,
                ScaleX = 1,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotCenter,
                TextColor = Color.Black,
                Multiline = false,
                Wordwrap = false,
                Parent = this
            };

            ArtistText = new TextBoxSprite()
            {
                Text = map.Artist + " | "+ map.Creator,
                Font = Fonts.Medium12,
                ScaleY = 0.5f,
                ScaleX = 1,
                Alignment = Alignment.BotLeft,
                TextAlignment = Alignment.TopCenter,
                TextColor = Color.Black,
                Multiline = false,
                Wordwrap = false,
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
            CurrentTint.R = (byte)(((HoverCurrentTween * 0.25) + 0.75f) * 255);
            CurrentTint.G = (byte)(((HoverCurrentTween * 0.25) + 0.75f) * 255);
            CurrentTint.B = (byte)(((HoverCurrentTween * 0.25) + 0.75f) * 255);
            Tint = CurrentTint;
            
            //TextSprite.Update(dt);
            base.Update(dt);
        }
    }
}
