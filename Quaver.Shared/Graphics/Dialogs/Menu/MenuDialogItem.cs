using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Graphics.Dialogs.Menu
{
    public class MenuDialogItem : PoolableSprite<IMenuDialogOption>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override int HEIGHT { get; } = 38;

        /// <summary>
        /// </summary>
        private ImageButton Button { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Name { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public MenuDialogItem(MenuDialog dialog, PoolableScrollContainer<IMenuDialogOption> container, IMenuDialogOption item, int index)
            : base(container, item, index)
        {
            Size = new ScalableVector2(Container.Width, HEIGHT);
            Alpha = 0;

            Button = new ImageButton(UserInterface.BlankBox, (o, e) =>
            {
                Item.ClickAction();
                DialogManager.Dismiss(dialog);
            })
            {
                Parent = this,
                Size = Size,
                Alpha = 0
            };

            Name = new SpriteTextBitmap(FontsBitmap.GothamRegular, Item.Name)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 15,
                FontSize = 14,
                Tint = Item.Color
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Button.Alpha = MathHelper.Lerp(Button.Alpha, Button.IsHovered ? 0.4f : 0f,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));


            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Button.Destroy();
            ButtonManager.Remove(Button);
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(IMenuDialogOption item, int index)
        {
        }
    }
}