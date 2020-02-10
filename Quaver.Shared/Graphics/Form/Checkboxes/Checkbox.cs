using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Form.Checkboxes
{
    public class Checkbox : Container
    {
        /// <summary>
        /// </summary>
        public ImageButton Button { get; private set; }

        /// <summary>
        /// </summary>
        public SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        public Sprite Sprite { get; set; }

        /// <summary>
        ///     Creates a new checkbox.
        /// </summary>
        /// <param name="size"></param>
        public Checkbox(ScalableVector2 size)
        {
            Size = size;

            CreateButton();
            CreateName();
            CreateCheckbox();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Button.Alpha = Button.IsHovered ? 0.35f : 0;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new CheckboxItemButton(UserInterface.BlankBox, this)
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
            };
        }

        /// <summary>
        /// </summary>
        private void CreateName()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 21)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 14
            };
        }

        /// <summary>
        /// </summary>
        private void CreateCheckbox()
        {
            Sprite = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(Height * 0.40f, Height * 0.40f),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_check_sign_in_a_rounded_black_square)
            };
        }
    }
}