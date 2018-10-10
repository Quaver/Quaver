using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.Menu.UI.Panels
{
    public class Panel : Button
    {
        /// <summary>
        ///     The thumbnail image for the panel.
        /// </summary>
        public Sprite Thumbnail { get; set; }

        /// <summary>
        ///     Contains the heading for the panel.
        /// </summary>
        public Sprite HeadingContainer { get; set; }

        /// <summary>
        ///     The title of the panel.
        /// </summary>
        public SpriteTextBitmap Title { get; set; }

        /// <summary>
        ///     The description of the panel.
        /// </summary>
        public SpriteTextBitmap Description { get; set; }

        /// <summary>
        ///     The original size of the panel.
        /// </summary>
        public ScalableVector2 OriginalSize { get; } = new ScalableVector2(302, 302);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="image"></param>
        public Panel(string title, string description, Texture2D image)
        {
            Size = new ScalableVector2(OriginalSize.X.Value, OriginalSize.Y.Value);

            Thumbnail = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(Width, Height - 100),
                Image = image,
                SpriteBatchOptions = new SpriteBatchOptions()
                {
                    BlendState = BlendState.NonPremultiplied
                }
            };

            HeadingContainer = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(Width, 100),
                Y = Thumbnail.Height,
                Tint = ColorHelper.HexToColor("#EEEEEE")
            };

            Title = new SpriteTextBitmap(BitmapFonts.Exo2BoldItalic, title.ToUpper(), 24,
                ColorHelper.HexToColor("#383939"), Alignment.MidCenter, (int) Width)
            {
                Parent = HeadingContainer,
                Alignment = Alignment.TopLeft,
                SpriteBatchOptions = new SpriteBatchOptions()
                {
                    BlendState = BlendState.NonPremultiplied
                },
                X = 10,
                Y = 6
            };

            Title.Size = new ScalableVector2(Title.Width * 0.95f, Title.Height * 0.95f);

            Description = new SpriteTextBitmap(BitmapFonts.Exo2BoldItalic, description, 20,
                ColorHelper.HexToColor("#383939"),
                Alignment.MidLeft, (int) (Width * 1.75f))
            {
                Parent = HeadingContainer,
                Alignment = Alignment.TopLeft,
                SpriteBatchOptions = new SpriteBatchOptions()
                {
                    BlendState = BlendState.NonPremultiplied
                },
                X = 15,
                Y = Title.Y + Title.Height + 0
            };

            Description.Size = new ScalableVector2(Description.Width * 0.50f, Description.Height * 0.50f);

            AddBorder(Color.White, 0);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (IsHovered)
            {
                Width = MathHelper.Lerp(Width, OriginalSize.X.Value * 1.08f + 2, (float) Math.Min(dt / 30, 1));
                Height = MathHelper.Lerp(Height, OriginalSize.Y.Value * 1.08f + 2, (float) Math.Min(dt / 30, 1));

                Border.Thickness = MathHelper.Lerp(Border.Thickness, 5, (float) Math.Min(dt / 30, 1));
                Border.FadeToColor(Color.Yellow, dt, 30);

                // Resetting the parent allows the panel to go on top of the other ones (changes draw order)
                Parent = Parent;
            }
            else
            {
                Width = MathHelper.Lerp(Width, OriginalSize.X.Value, (float) Math.Min(dt / 30, 1));
                Height = MathHelper.Lerp(Height, OriginalSize.Y.Value, (float) Math.Min(dt / 30, 1));

                Border.Thickness = MathHelper.Lerp(Border.Thickness, 0, (float) Math.Min(dt / 30, 1));
                Border.FadeToColor(Colors.MainAccent, dt, 30);
            }

            // Always make sure thumbnail is at the correct size
            Thumbnail.Width = Width;
            Thumbnail.Height = Height - 100;

            // Always make sure heading container is at the correct size.
            HeadingContainer.Width = Width;
            HeadingContainer.Height = 100;
            HeadingContainer.Y = Thumbnail.Height;

            base.Update(gameTime);
        }
    }
}