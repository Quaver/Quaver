using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public sealed class DrawableMultiplayerTableItem : PoolableSprite<MultiplayerTableItem>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 60;

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Value { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableMultiplayerTableItem(PoolableScrollContainer<MultiplayerTableItem> container, MultiplayerTableItem item,
            int index) : base(container, item, index)
        {
            Size = new ScalableVector2(container.Width, HEIGHT);
            CreateName();
            CreateValue();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(MultiplayerTableItem item, int index)
        {
            Item = item;
            Index = index;

            Tint = index  % 2 == 0 ? ColorHelper.HexToColor("#363636") : ColorHelper.HexToColor("#242424");

            if (item.SelectedGame.Value == null)
                return;

            ScheduleUpdate(() =>
            {
                Name.Text = item.GetName();
                Value.Text = item.GetValue();

                var container = (DrawableMultiplayerTable) Container;

                if (container.IsMultiplayer)
                {
                    if (item.Selector != null)
                    {
                        if (Item.Selector is Dropdown)
                        {
                            Item.Selector.Parent = Container.ContentContainer;
                            Item.Selector.Alignment = Alignment.TopRight;
                            Item.Selector.Y = Y + Height / 2f - Item.Selector.Height / 2f;
                        }
                        else
                        {
                            Item.Selector.Parent = this;
                            Item.Selector.Alignment = Alignment.MidRight;
                        }

                        Item.Selector.X = -Name.X;
                        Item.Selector.UsePreviousSpriteBatchOptions = true;
                        Item.UpdateSelectorState();

                        Value.Visible = false;
                    }
                }
            });
        }

        /// <summary>
        /// </summary>
        private void CreateName()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 14,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateValue()
        {
            Value = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                UsePreviousSpriteBatchOptions = true
            };
        }
    }
}