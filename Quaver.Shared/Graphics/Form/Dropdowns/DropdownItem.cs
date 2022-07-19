using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Graphics.Form.Dropdowns
{
    public class DropdownItem : ImageButton
    {
        /// <summary>
        ///     The parent dropdown sprite
        /// </summary>
        public Dropdown Dropdown { get; }

        /// <summary>
        ///     The index of <see cref="Dropdown"/> options this item represents
        /// </summary>
        public int Index { get; }

        /// <summary>
        ///     The text for the individual item
        /// </summary>
        public SpriteTextPlus Text { get; private set; }

        /// <summary>
        ///     The sprite that lights up when hovered
        /// </summary>
        private Sprite HoverSprite { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dropdown"></param>
        /// <param name="index"></param>
        /// <param name="image"></param>
        public DropdownItem(Dropdown dropdown, int index, Texture2D image = null) : base(image ?? WobbleAssets.WhiteBox)
        {
            Dropdown = dropdown;
            Index = index;

            Size = Dropdown.Size;
            Tint = ColorHelper.HexToColor("#181818");

            CreateHoverSprite();
            CreateText();

            Hovered += OnHovered;
            LeftHover += OnHoverLeft;
            Clicked += OnClicked;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (HoverSprite.Image != Image)
                HoverSprite.Image = Image;

            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates <see cref="HoverSprite"/>
        /// </summary>
        private void CreateHoverSprite()
        {
            HoverSprite = new Sprite
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
                Image = Image
            };
        }

        /// <summary>
        ///     Creates <see cref="Text"/>
        /// </summary>
        private void CreateText()
        {
            Text = new SpriteTextPlus(Dropdown.SelectedText.Font, Dropdown.Options[Index], Dropdown.FontSize)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Dropdown.PaddingX,
                Alpha = 1
            };

            if (Dropdown.MaxWidth != 0)
                Text.TruncateWithEllipsis(Dropdown.MaxWidth);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHovered(object sender, EventArgs e)
        {
            if (!Dropdown.Opened)
                return;

            HoverSprite.ClearAnimations();
            HoverSprite.FadeTo(Dropdown.HighlightAlpha, Easing.Linear, 75);

            SkinManager.Skin?.SoundHover?.CreateChannel()?.Play();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHoverLeft(object sender, EventArgs e)
        {
            HoverSprite.ClearAnimations();
            HoverSprite.FadeTo(0f, Easing.Linear, 75);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClicked(object sender, EventArgs e)
        {
            if (!Dropdown.Opened)
                return;

            Dropdown.SelectItem(this);
            Dropdown.Close();
        }
    }
}