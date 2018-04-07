using System;
using Microsoft.Xna.Framework;
using Quaver.Database.Maps;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.Buttons
{
    /// <inheritdoc />
    /// <summary>
    ///     This type of button is used for simple buttons that only require a single image + text, but also includes a tint animation.
    /// </summary>
    internal class QuaverMapsetSelectButton : QuaverButton
    {
        internal static float BUTTON_Y_SIZE = 56.0f;

        internal static float BUTTON_X_SIZE = 400.0f;

        internal static float BUTTON_OFFSET_PADDING = BUTTON_Y_SIZE + 2.0f;

        internal bool Selected { get; set; }

        internal int Index { get; set; }

        private QuaverTextbox TitleText { get; set; }

        private QuaverTextbox ArtistText { get; set; }

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
        internal QuaverMapsetSelectButton(float ButtonScale, int index, Mapset mapset)
        {
            Size.Y.Offset = BUTTON_Y_SIZE * ButtonScale;
            Size.X.Offset = BUTTON_X_SIZE * ButtonScale;

            //Load and set BG Image
            /*
            Task.Run(() => {
                try
                {
                    Image = ImageLoader.Load(Configuration.SongDirectory + "/" + map.Directory + "/" + map.BackgroundPath);
                }
                catch
                {
                    Exception ex;
                }
            });*/

            TitleText = new QuaverTextbox()
            {
                Text = "Song Title", //map.Title,
                Font = QuaverFonts.Medium48,
                Size = new UDim2D(-2 * ButtonScale, 22 * ButtonScale, 1, 0),
                Position = new UDim2D(ButtonScale, ButtonScale),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.Black,
                Parent = this
            };

            ArtistText = new QuaverTextbox()
            {
                Text = "Song Artist | Charter", //map.Artist + " | "+ map.Creator,
                Font = QuaverFonts.Medium48,
                Size = new UDim2D(-2 * ButtonScale, 14 * ButtonScale, 1, 0),
                Position = new UDim2D(ButtonScale, 22 * ButtonScale, 0, 0),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.TopLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.Black,
                Parent = this
            };

            UpdateButtonMapIndex(index, mapset);
        }

        /// <summary>
        ///     This method is called when the mouse hovers over the button
        /// </summary>
        protected override void MouseOver()
        {
            HoverTargetTween = 0.85f;
        }

        /// <summary>
        ///     This method is called when the Mouse hovers out of the button
        /// </summary>
        protected override void MouseOut()
        {
            HoverTargetTween = 0.6f;
        }

        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        internal override void Update(double dt)
        {
            if (Selected)
                HoverCurrentTween = GraphicsHelper.Tween(1, HoverCurrentTween, Math.Min(dt / 40, 1));
            else
                HoverCurrentTween = GraphicsHelper.Tween(HoverTargetTween, HoverCurrentTween, Math.Min(dt / 40, 1));

            CurrentTint.R = (byte)(HoverCurrentTween * 255);
            CurrentTint.G = (byte)(HoverCurrentTween * 255);
            CurrentTint.B = (byte)(HoverCurrentTween * 255);

            Tint = CurrentTint;
            
            //TextSprite.Update(dt);
            base.Update(dt);
        }

        internal void UpdateButtonMapIndex(int newIndex, Mapset newMapset)
        {
            // Ignore null mapsets
            if (newMapset == null) return;

            Index = newIndex;
            if (newMapset.Maps.Count > 0)
            {
                TitleText.Text = newMapset.Maps[0].Title;
                ArtistText.Text = newMapset.Maps[0].Artist + " | " + newMapset.Maps[0].Creator;
            }
            //TitleText = newMapset.
        }
    }
}
