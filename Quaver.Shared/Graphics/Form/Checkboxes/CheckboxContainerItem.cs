using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Form.Checkboxes
{
    public sealed class CheckboxContainerItem : PoolableSprite<ICheckboxContainerItem>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 50;

        /// <summary>
        /// </summary>
        public ImageButton Button { get; private set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Checkbox { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public CheckboxContainerItem(PoolableScrollContainer<ICheckboxContainerItem> container, ICheckboxContainerItem item, int index)
            : base(container, item, index)
        {
            Size = new ScalableVector2(container.Width, HEIGHT);
            Alpha = 0;

            CreateButton();
            CreateName();
            CreateCheckbox();

            Item.IsSelected = Item.GetSelectedState();
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(ICheckboxContainerItem item, int index)
        {
            Item = item;
            Index = index;

            ScheduleUpdate(() =>
            {
                Name.Text = item.GetName();
                Checkbox.Image = FontAwesome.Get(Item.IsSelected ? FontAwesomeIcon.fa_check : FontAwesomeIcon.fa_check_box_empty);
            });
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new CheckboxItemButton(UserInterface.BlankBox, Container)
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
            };

            Button.Clicked += (sender, args) =>
            {
                Item.IsSelected = !Item.IsSelected;
                Item.OnToggle();
                UpdateContent(Item, Index);
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
            Checkbox = new Sprite()
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