using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Form.Checkboxes
{
    public sealed class CheckboxContainerItem : PoolableSprite<ICheckboxContainerItem>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 50;

        /// <summary>
        ///     The checkbox itself.
        /// </summary>
        public Checkbox Checkbox { get; set; }

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

            CreateCheckbox();

            Item.IsSelected = Item.GetSelectedState();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Checkbox.Update(gameTime);

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
                Checkbox.Name.Text = item.GetName();
                Checkbox.Sprite.Image = FontAwesome.Get(Item.IsSelected ? FontAwesomeIcon.fa_check : FontAwesomeIcon.fa_check_box_empty);
            });
        }

        /// <summary>
        /// </summary>
        private void CreateCheckbox()
        {
            Checkbox = new Checkbox(Size)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Name = { UsePreviousSpriteBatchOptions = true, },
                Sprite = { UsePreviousSpriteBatchOptions = true },
                Button = { UsePreviousSpriteBatchOptions = true }
            };

            Checkbox.Button.Clicked += (sender, args) =>
            {
                Item.IsSelected = !Item.IsSelected;
                Item.OnToggle();
                UpdateContent(Item, Index);
            };
        }
    }
}