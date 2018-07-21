using System;
using Quaver.API.Enums;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Graphics.Buttons
{
    /// <inheritdoc />
    /// <summary>
    ///     This type of button is used for simple buttons that only require a single image + text, but also includes a tint animation.
    /// </summary>
    internal class MapDifficultySelectButton : Button
    {
        internal static float BUTTON_Y_SIZE = 40.0f;

        internal static float BUTTON_X_SIZE = 400.0f;

        internal static float BUTTON_OFFSET_PADDING = BUTTON_Y_SIZE + 2.0f;

        internal bool Selected { get; set; }

        internal int Index { get; set; }

        private SpriteText DifficultyNameText { get; set; }

        private Sprite GradeImage { get; set; }

        private Sprite DifficultyScaleImage { get; set; }

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
        internal MapDifficultySelectButton(float ButtonScale, int index, Map map)
        {
            Size.Y.Offset = BUTTON_Y_SIZE * ButtonScale;
            Size.X.Offset = BUTTON_X_SIZE * ButtonScale;

            DifficultyNameText = new SpriteText()
            {
                Font = Fonts.Medium48,
                Size = new UDim2D(-40 * ButtonScale, -ButtonScale, 1, 0.6f),
                Position = new UDim2D(-ButtonScale, ButtonScale),
                Alignment = Alignment.TopRight,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.Black,
                Parent = this
            };

            DifficultyScaleImage = new Sprite()
            {
                Size = new UDim2D(-40 * ButtonScale, -ButtonScale * 2, 1, 0.4f),
                Position = new UDim2D(40 * ButtonScale, -ButtonScale * 2),
                Alignment = Alignment.BotLeft,
                Parent = this
            };

            GradeImage = new Sprite()
            {
                Position = new UDim2D(ButtonScale, 0),
                Size = new UDim2D(38 * ButtonScale, 38 * ButtonScale),
                Alpha = 1f,
                Image = GameBase.Skin.Grades[Grade.A],
                Alignment = Alignment.MidLeft,
                Parent = this
            };

            UpdateButtonMapIndex(index, map);
        }

        /// <inheritdoc />
        /// <summary>
        ///     This method is called when the mouse hovers over the button
        /// </summary>
        protected override void MouseOver() => HoverTargetTween = 0.85f;

        /// <summary>
        ///     This method is called when the Mouse hovers out of the button
        /// </summary>
        protected override void MouseOut() => HoverTargetTween = 0.6f;

        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        internal override void Update(double dt)
        {
            if (Selected)
                HoverCurrentTween = GraphicsHelper.Tween(1, HoverCurrentTween, Math.Min(dt / BUTTON_Y_SIZE, 1));
            else
                HoverCurrentTween = GraphicsHelper.Tween(HoverTargetTween, HoverCurrentTween, Math.Min(dt / BUTTON_Y_SIZE, 1));

            CurrentTint.R = (byte)(HoverCurrentTween * 205);
            CurrentTint.G = (byte)(HoverCurrentTween * 255);
            CurrentTint.B = (byte)(HoverCurrentTween * 255);

            Tint = CurrentTint;

            // temp
            CurrentTint.R = (byte)(HoverCurrentTween * 255);
            GradeImage.Tint = CurrentTint;
            DifficultyScaleImage.Tint = CurrentTint;

            base.Update(dt);
        }

        internal void UpdateButtonMapIndex(int newIndex, Map newMap)
        {
            Index = newIndex;
            DifficultyNameText.Text = newMap.DifficultyName;
            DifficultyScaleImage.ScaleX = newMap.DifficultyRating / 30f;
            //DiffText.Text = string.Format("{0:f2}", newMap.DifficultyRating);
        }
    }
}
