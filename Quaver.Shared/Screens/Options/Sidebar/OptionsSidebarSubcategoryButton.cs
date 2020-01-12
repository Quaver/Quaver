using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Options.Sections;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Sidebar
{
    public class OptionsSidebarSubcategoryButton : ImageButton
    {
        /// <summary>
        /// </summary>
        public static int HEIGHT => OptionsSidebarSectionButton.HEIGHT - 4;

        /// <summary>
        /// </summary>
        private Bindable<OptionsSection> SelectedSection { get; }

        /// <summary>
        /// </summary>
        private OptionsSubcategory Subcategory { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="selectedSection"></param>
        /// <param name="subcategory"></param>
        /// <param name="width"></param>
        public OptionsSidebarSubcategoryButton(Bindable<OptionsSection> selectedSection, OptionsSubcategory subcategory, float width)
            : base(UserInterface.BlankBox)
        {
            SelectedSection = selectedSection;
            Subcategory = subcategory;

            Size = new ScalableVector2(width, HEIGHT);
            Alpha = 0;

            CreateName();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            var alpha = IsHovered ? 0.30f : 0;
            Alpha = MathHelper.Lerp(Alpha, alpha, (float) Math.Min(dt / 20f, 1));

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateName()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "•   " + Subcategory.Name, 20)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true,
                X = Width * 0.18f,
                Alpha = 0,
                Tint = ColorHelper.HexToColor("#45D6F5")
            };

            Name.FadeTo(0.95f, Easing.Linear, 225);
        }
    }
}