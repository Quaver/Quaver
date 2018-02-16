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
using Quaver.Config;

namespace Quaver.Graphics.Button
{
    /// <summary>
    ///     This type of button is used for simple buttons that only require a single image + text, but also includes a tint animation.
    /// </summary>
    internal class MapDifficultySelectButton : Button
    {
        internal static float BUTTON_Y_SIZE = 40.0f;

        internal static float BUTTON_X_SIZE = 460.0f;

        internal bool Selected { get; set; }

        internal int Index { get; set; }

        private TextBoxSprite DifficultyNameText { get; set; }

        private Sprite.Sprite GradeImage { get; set; }

        private Sprite.Sprite DifficultyScaleImage { get; set; }

        /// <summary>
        ///     Current tween value of the object. Used for animation.
        /// </summary>
        private float HoverCurrentTween { get; set; }

        /// <summary>
        ///     Target tween value of the object. Used for animation.
        /// </summary>
        private float HoverTargetTween { get; set; } = 0.6f;

        /// <summary>
        ///     Current Color/Tint of the object.
        /// </summary>
        private Color CurrentTint = Color.White;

        //Constructor
        internal MapDifficultySelectButton(float ButtonScale, int index, Beatmap map)
        {
            Size.Y.Offset = BUTTON_Y_SIZE * ButtonScale;
            Size.X.Offset = BUTTON_X_SIZE * ButtonScale;

            DifficultyNameText = new TextBoxSprite()
            {
                Font = Fonts.Medium48,
                Size = new UDim2(-40 * ButtonScale, -ButtonScale, 1, 0.6f),
                Position = new UDim2(-ButtonScale, ButtonScale),
                Alignment = Alignment.TopRight,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.Black,
                Parent = this
            };

            DifficultyScaleImage = new Sprite.Sprite()
            {
                Size = new UDim2(-40 * ButtonScale, -ButtonScale * 2, 1, 0.4f),
                Position = new UDim2(40 * ButtonScale, -ButtonScale * 2),
                Alignment = Alignment.BotLeft,
                Parent = this
            };

            GradeImage = new Sprite.Sprite()
            {
                Position = new UDim2(ButtonScale, 0),
                Size = new UDim2(38 * ButtonScale, 38 * ButtonScale),
                Alpha = 1f,
                Image = GameBase.LoadedSkin.GradeSmallA,
                Alignment = Alignment.MidLeft,
                Parent = this
            };

            UpdateButtonMapIndex(index, map);
        }

        /// <summary>
        ///     This method is called when the mouse hovers over the button
        /// </summary>
        internal override void MouseOver()
        {
            HoverTargetTween = 0.85f;
        }

        /// <summary>
        ///     This method is called when the Mouse hovers out of the button
        /// </summary>
        internal override void MouseOut()
        {
            HoverTargetTween = 0.6f;
        }

        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        internal override void Update(double dt)
        {
            if (Selected)
                HoverCurrentTween = Util.Tween(1, HoverCurrentTween, Math.Min(dt / BUTTON_Y_SIZE, 1));
            else
                HoverCurrentTween = Util.Tween(HoverTargetTween, HoverCurrentTween, Math.Min(dt / BUTTON_Y_SIZE, 1));

            CurrentTint.R = (byte)(HoverCurrentTween * 155);
            CurrentTint.G = (byte)(HoverCurrentTween * 255);
            CurrentTint.B = (byte)(HoverCurrentTween * 255);

            Tint = CurrentTint;

            // temp
            CurrentTint.R = (byte)(HoverCurrentTween * 255);
            GradeImage.Tint = CurrentTint;
            DifficultyScaleImage.Tint = CurrentTint;

            base.Update(dt);
        }

        internal void UpdateButtonMapIndex(int newIndex, Beatmap newMap)
        {
            Index = newIndex;
            DifficultyNameText.Text = newMap.DifficultyName;
            DifficultyScaleImage.ScaleX = newMap.DifficultyRating / 30f;
            //DiffText.Text = string.Format("{0:f2}", newMap.DifficultyRating);
        }
    }
}
