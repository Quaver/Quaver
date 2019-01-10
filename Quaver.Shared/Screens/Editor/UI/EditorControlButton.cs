using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI
{
    public class EditorControlButton : JukeboxButton
    {
        /// <summary>
        ///     The tooltip used to show what the button does.
        /// </summary>
        private Sprite Tooltip { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText TooltipText { get; set; }

        /// <summary>
        ///     The amount of space between the control button and the tooltip.
        /// </summary>
        private int Padding { get; }

        /// <summary>
        /// </summary>
        private string Name { get; }

        /// <summary>
        /// </summary>
        private bool HoverAnimationAdded { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="image"></param>
        /// <param name="name"></param>
        /// <param name="padding"></param>
        /// <param name="clickAction"></param>
        public EditorControlButton(Texture2D image, string name, int padding, EventHandler clickAction = null) : base(image, clickAction)
        {
            Name = name;
            Padding = padding;

            CreateTooltip();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (IsHovered && !HoverAnimationAdded)
            {
                Tooltip.ClearAnimations();
                TooltipText.ClearAnimations();
                Tooltip.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Tooltip.Alpha, 1f, 200));
                Tooltip.MoveToX(Padding, Easing.OutQuint, 600);
                HoverAnimationAdded = true;
            }

            if (!IsHovered && HoverAnimationAdded)
            {
                Tooltip.ClearAnimations();
                TooltipText.ClearAnimations();

                Tooltip.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Tooltip.Alpha, 0f, 100));
                Tooltip.MoveToX(Padding - 20, Easing.OutQuint, 600);
                HoverAnimationAdded = false;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateTooltip()
        {
            TooltipText = new SpriteText(Fonts.Exo2SemiBold, Name, 13)
            {
                Alpha = 0
            };

            Tooltip = new Sprite()
            {
                Parent = this,
                X = Padding - 20,
                Size = new ScalableVector2(TooltipText.Width + 10, TooltipText.Height + 10),
                Tint = Color.Black,
                Alignment = Alignment.MidLeft,
                Alpha = 0,
                SetChildrenAlpha = true
            };

            Tooltip.AddBorder(Color.White);
            Tooltip.Border.Alpha = 0;

            TooltipText.Parent = Tooltip;
            TooltipText.Alignment = Alignment.MidCenter;
            TooltipText.TextAlignment = Alignment.MidCenter;
        }
    }
}