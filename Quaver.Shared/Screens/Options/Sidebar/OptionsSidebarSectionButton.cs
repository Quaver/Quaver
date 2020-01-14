using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Options.Sections;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Sidebar
{
    public class OptionsSidebarSectionButton : ImageButton
    {
        /// <summary>
        /// </summary>
        public static int HEIGHT { get; } = 66;

        /// <summary>
        /// </summary>
        private Bindable<OptionsSection> SelectedSection { get; }

        /// <summary>
        /// </summary>
        public OptionsSection Section { get; }

        /// <summary>
        /// </summary>
        private Sprite Icon { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        ///     The little vertical line to the left of the button
        /// </summary>
        private Sprite TickerFlag { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="selectedSection"></param>
        /// <param name="section"></param>
        /// <param name="width"></param>
        public OptionsSidebarSectionButton(Bindable<OptionsSection> selectedSection, OptionsSection section, float width)
            : base(UserInterface.BlankBox)
        {
            SelectedSection = selectedSection;
            Section = section;

            Size = new ScalableVector2(width, HEIGHT);
            Alpha = 0f;

            Image = UserInterface.OptionsSidebarButtonBackground;
            CreateIcon();
            CreateName();
            CreateTickerFlag();

            Clicked += (sender, args) =>
            {
                if (SelectedSection.Value == Section)
                    return;

                SelectedSection.Value = Section;
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var alpha = SelectedSection.Value == Section || IsHovered ? 1 : 0;
            var color = SelectedSection.Value == Section ? ColorHelper.HexToColor("#45D6F5") : Color.White;

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            Alpha = MathHelper.Lerp(Alpha, alpha, (float) Math.Min(dt / 20f, 1));
            TickerFlag.Alpha = Alpha;

            FadeToColor(color, dt, 20);
            TickerFlag.Tint = Tint;
            Icon.Tint = Tint;
            Name.Tint = Tint;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateIcon()
        {
            const float scale = 0.35f;

            Icon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 20,
                Size = new ScalableVector2(Height * scale, Height * scale),
                Image = Section.Icon,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateName()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), Section.Name, 21)
            {
                Parent = Icon,
                Alignment = Alignment.MidRight,
                X = 12,
                UsePreviousSpriteBatchOptions = true
            };

            Name.X += Name.Width;
        }

        /// <summary>
        /// </summary>
        private void CreateTickerFlag() => TickerFlag = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(4, HEIGHT),
            UsePreviousSpriteBatchOptions = true
        };
    }
}