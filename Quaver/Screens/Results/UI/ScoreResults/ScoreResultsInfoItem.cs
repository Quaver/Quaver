using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Resources;
using Quaver.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Results.UI.ScoreResults
{
    public class ScoreResultsInfoItem
    {
        /// <summary>
        ///     The "title" of the result.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     The value of the result
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///    The text that displays the title.
        /// </summary>
        public SpriteText TitleText { get; set; }

        /// <summary>
        ///     The text that displays the value.
        /// </summary>
        public SpriteText ValueText { get; set; }

        /// <summary>
        ///     If there is no set value, then we will display this sprite, to show
        ///     that it is loading.
        /// </summary>
        private Sprite LoadingSprite { get; set; }

        /// <summary>
        ///     If the item has already been initialized. It'll throw an exception
        ///     if it has been initialized once already.
        /// </summary>
        private bool IsInitialized { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="title"></param>
        /// <param name="defaultValue"></param>
        public ScoreResultsInfoItem(string title, string defaultValue = null)
        {
            Title = title;
            Value = defaultValue;
        }

        /// <summary>
        ///    Initializes the item. It can only be initialized one time.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="posX"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Initialize(Drawable parent, float posX)
        {
            if (IsInitialized)
                throw new InvalidOperationException($"ScoreResultsInfoItem has already been initialized.");

            TitleText = new SpriteText(BitmapFonts.Exo2Regular, Title.ToUpper(), 16)
            {
                Parent = parent,
                TextAlignment = Alignment.MidCenter,
                Text = Title.ToUpper(),
                X = posX,
                Y = 17,
                Tint = Colors.SecondaryAccent
            };

            if (Value != null)
            {
                ValueText = new SpriteText(BitmapFonts.Exo2Regular, Value, 16)
                {
                    Parent = parent,
                    TextAlignment = Alignment.MidCenter,
                    Text = Value,
                    X = posX,
                    Y = TitleText.Y + 25,
                    Tint = Color.White
                };
            }
            else
            {
                LoadingSprite = new Sprite()
                {
                    Parent = parent,
                    Size = new ScalableVector2(20, 20),
                    Position = new ScalableVector2(posX - 13, TitleText.Y + TitleText.Height + 17),
                    Image = FontAwesome.Get(FontAwesomeIcon.fa_spinner_of_dots)
                };
            }

            IsInitialized = true;
        }

        /// <summary>
        ///     Updates the item. Mainly used for loading animations
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            if (LoadingSprite == null)
                return;

            LoadingSprite.Rotation = (float)(MathHelper.ToDegrees(LoadingSprite.Rotation) + 7 * dt / 30f);
        }
    }
}
