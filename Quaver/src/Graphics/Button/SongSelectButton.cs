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
using Quaver.Graphics.Sprite;

namespace Quaver.Graphics.Button
{
    /// <summary>
    ///     This type of button is used for simple buttons that only require a single image + text, but also includes a tint animation.
    /// </summary>
    internal class SongSelectButton : Button
    {
        private TextBoxSprite TitleText { get; set; }

        private TextBoxSprite ArtistText { get; set; }

        private TextBoxSprite DiffText { get; set; }

        private Boundary ModeAndGradeBoundaryOutter { get; set; }

        private Boundary ModeAndGradeBoundaryInner { get; set; }

        private Sprite.Sprite GameModeImage { get; set; }

        private Sprite.Sprite GradeImage { get; set; }

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
                ScaleX = 0.8f,
                Alignment = Alignment.TopRight,
                TextAlignment = Alignment.BotLeft,
                TextColor = Color.Black,
                Multiline = false,
                Wordwrap = false,
                TextScale = ButtonScale,
                Parent = this
            };

            ArtistText = new TextBoxSprite()
            {
                Text = map.Artist + " | "+ map.Creator,
                Font = Fonts.Medium12,
                ScaleY = 0.5f,
                ScaleX = 0.8f,
                Alignment = Alignment.BotRight,
                TextAlignment = Alignment.TopLeft,
                TextColor = Color.Black,
                Multiline = false,
                Wordwrap = false,
                TextScale = 0.9f * ButtonScale,
                Parent = this
            };

            DiffText = new TextBoxSprite()
            {
                Text = "00.00",
                Font = Fonts.Bold12,
                ScaleY = 0.5f,
                ScaleX = 0.2f,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotCenter,
                TextColor = Color.Red,
                Multiline = false,
                Wordwrap = false,
                TextScale = ButtonScale,
                Parent = this
            };

            /*
            ModeAndGradeBoundaryOutter = new Boundary()
            {
                ScaleX = 0.2f,
                ScaleY = 0.5f,
                Alignment = Alignment.BotLeft,
                Parent = this
            };

            ModeAndGradeBoundaryInner = new Boundary()
            {
                SizeX = 35 * ButtonScale,
                ScaleY = 1,
                Alignment = Alignment.MidCenter,
                Parent = ModeAndGradeBoundaryOutter
            };*/

            GameModeImage = new Sprite.Sprite()
            {
                ScaleX = 0.2f,
                ScaleY = 0.5f,
                Alignment = Alignment.BotLeft,
                Parent = this
            };

            /*
            GradeImage = new Sprite.Sprite()
            {
                Size = Vector2.One * 15 * ButtonScale,
                Alignment = Alignment.MidLeft,
                Parent = ModeAndGradeBoundaryOutter
            };*/
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
