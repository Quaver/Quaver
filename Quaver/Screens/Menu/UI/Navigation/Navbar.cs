using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Screens.Menu.UI.Navigation
{
    public class Navbar : Sprite
    {
        /// <summary>
        ///     The line at the bottom of the navbar.
        /// </summary>
        public Line Line { get; set; }

        /// <summary>
        ///     The Quaver logo displayed
        /// </summary>
        public Sprite QuaverLogo { get; set; }

        /// <summary>
        ///     The items that are aligned to the left.
        /// </summary>
        public List<NavbarItem> LeftAlignedItems { get; }

        /// <summary>
        ///     The items that are aligned to the right.
        /// </summary>
        public List<NavbarItem> RightAlignedItems { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public Navbar(List<NavbarItem> leftAlignedItems, List<NavbarItem> rightAlignedItems)
        {
            LeftAlignedItems = leftAlignedItems;
            RightAlignedItems = rightAlignedItems;

            Tint = Color.Transparent;
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);

            CreateLine();
            CreateQuaverLogo();

            AlignLeftItems();
            AlignRightItems();
        }

        /// <summary>
        ///     Creates the line sprite.
        /// </summary>
        private void CreateLine()
        {
            Line = new Line(Vector2.Zero, Color.LightGray, 2)
            {
                Parent = this,
                Position = new ScalableVector2(28, 54),
                Alpha = 0.90f
            };

            Line.EndPosition = new Vector2(WindowManager.Width - Line.X, Line.AbsolutePosition.Y);
        }

        /// <summary>
        ///     Creates the Quaver logo sprite.
        /// </summary>
        private void CreateQuaverLogo()
        {
            /*QuaverLogo = new Sprite
            {
                Parent = Line,
                Image = UserInterface.BlankBox,
                SpriteBatchOptions = new SpriteBatchOptions() { BlendState = BlendState.NonPremultiplied },
                Size = new ScalableVector2(150, 45)
            };

            QuaverLogo.Y -= QuaverLogo.Height;*/
        }

        /// <summary>
        ///     Aligns the items from the left.
        /// </summary>
        private void AlignLeftItems()
        {
            //var startingX = QuaverLogo.X + QuaverLogo.Width;
            var startingX = 0;

            for (var i = 0; i < LeftAlignedItems.Count; i++)
            {
                var item = LeftAlignedItems[i];

                item.Parent = Line;
                item.X = startingX + i * item.Width;
                item.Y -= item.Height;
            }
        }

        /// <summary>
        ///     Aligns the items from the right
        /// </summary>
        public void AlignRightItems()
        {
            var startingX = Width - Line.X * 2;

            for (var i = 0; i < RightAlignedItems.Count; i++)
            {
                var item = RightAlignedItems[i];
                item.Parent = Line;
                item.Y = -item.Height;

                if (i == 0)
                    item.X = startingX - item.Width * (i + 1);
                else
                {
                    var previous = RightAlignedItems[i - 1];
                    item.X = previous.X - item.Width + 1;
                }
            }
        }

        /// <summary>
        ///     Performs an exit animation for the navbar
        /// </summary>
        public void Exit()
        {
            Line.SpriteBatchOptions = new SpriteBatchOptions() {BlendState = BlendState.AlphaBlend};
            Line.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Line.Alpha, 0, 200));

            LeftAlignedItems.ForEach(x =>
            {
                x.BottomLine.SpriteBatchOptions = new SpriteBatchOptions() {BlendState = BlendState.AlphaBlend};
                x.BottomLine.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, x.BottomLine.Alpha, 0, 200));
                x.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, x.Alpha, 0, 200));

                x.Children.ForEach(y =>
                {
                    if (y is Sprite sprite)
                    {
                        y.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, sprite.Alpha, 0, 200));
                    }
                });
            });

            RightAlignedItems.ForEach(x =>
            {
                x.BottomLine.SpriteBatchOptions = new SpriteBatchOptions() {BlendState = BlendState.AlphaBlend};
                x.BottomLine.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, x.BottomLine.Alpha, 0, 200));
                x.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, x.Alpha, 0, 200));

                x.Children.ForEach(y =>
                {
                    if (y is Sprite sprite)
                    {
                        y.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, sprite.Alpha, 0, 200));
                    }
                });
            });
        }
    }
}