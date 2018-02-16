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
    internal class MapsetSelectButton : Button
    {
        internal static float BUTTON_Y_SIZE = 56.0f;

        internal static float BUTTON_X_SIZE = 400.0f;

        internal bool Selected { get; set; }

        internal int Index { get; set; }

        private TextBoxSprite TitleText { get; set; }

        private TextBoxSprite ArtistText { get; set; }

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
        internal MapsetSelectButton(float ButtonScale, int index, Mapset mapset)
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

            TitleText = new TextBoxSprite()
            {
                Text = "Song Title", //map.Title,
                Font = Fonts.Medium48,
                Size = new UDim2(-2 * ButtonScale, 22 * ButtonScale, 1, 0),
                Position = new UDim2(ButtonScale, ButtonScale),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.Black,
                Parent = this
            };

            ArtistText = new TextBoxSprite()
            {
                Text = "Song Artist | Charter", //map.Artist + " | "+ map.Creator,
                Font = Fonts.Medium48,
                Size = new UDim2(-2 * ButtonScale, 14 * ButtonScale, 1, 0),
                Position = new UDim2(ButtonScale, 22 * ButtonScale, 0, 0),
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
                HoverCurrentTween = Util.Tween(1, HoverCurrentTween, Math.Min(dt / 40, 1));
            else
                HoverCurrentTween = Util.Tween(HoverTargetTween, HoverCurrentTween, Math.Min(dt / 40, 1));

            CurrentTint.R = (byte)(HoverCurrentTween * 255);
            CurrentTint.G = (byte)(HoverCurrentTween * 155);
            CurrentTint.B = (byte)(HoverCurrentTween * 155);

            Tint = CurrentTint;
            
            //TextSprite.Update(dt);
            base.Update(dt);
        }

        internal void UpdateButtonMapIndex(int newIndex, Mapset newMapset)
        {
            Index = newIndex;
            if (newMapset.Beatmaps.Count > 0)
            {
                TitleText.Text = newMapset.Beatmaps[0].Title;
                ArtistText.Text = newMapset.Beatmaps[0].Artist + " | " + newMapset.Beatmaps[0].Creator;
            }
            //TitleText = newMapset.
        }
    }
}
