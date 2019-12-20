using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
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

        /// <summary>
        /// </summary>
        public Color BackgroundColor => Index % 2 == 0 ? ColorHelper.HexToColor("#363636") : ColorHelper.HexToColor("#242424");

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
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleHoverAnimations(gameTime);
            HandleClick();

            if (Item.NeedsStateUpdate)
                UpdateContent(Item, Index);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Item.Dispose();
            base.Destroy();
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

            Tint = BackgroundColor;

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

                Item.NeedsStateUpdate = false;
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

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleHoverAnimations(GameTime gameTime)
        {
            var color = IsHovered() && !IsSelectorHovered() && DialogManager.Dialogs.Count == 0 ? ColorHelper.HexToColor("#575757"): BackgroundColor;
            FadeToColor(color, gameTime.ElapsedGameTime.TotalMilliseconds, 30);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private bool IsSelectorHovered()
        {
            var container = (DrawableMultiplayerTable) Container;

            if (container.IsMultiplayer)
                return container.Pool.Any(x => x.Item.Selector is Button b && b.IsHovered || x.Item.Selector is Dropdown d && d.Opened);

            return false;
        }

        /// <summary>
        /// </summary>
        private void HandleClick()
        {
            if (!IsHovered() || IsSelectorHovered() || DialogManager.Dialogs.Count != 0)
                return;

            if (!MouseManager.IsUniqueClick(MouseButton.Left))
                return;

            Item.ClickAction?.Invoke();
        }
    }
}