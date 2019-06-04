using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings
{
    public class MultiplayerSettingsItem : PoolableSprite<IMultiplayerSettingsItem>
    {
        /// <summary>
        /// </summary>
        public sealed override int HEIGHT { get; } = 42;

        /// <summary>
        /// </summary>
        public SpriteTextBitmap Name { get; }

        /// <summary>
        /// </summary>
        private MultiplayerSettingsItemButton Button { get; }

        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public MultiplayerSettingsItem(PoolableScrollContainer<IMultiplayerSettingsItem> container, IMultiplayerSettingsItem item, int index)
            : base(container, item, index)
        {
            Size = new ScalableVector2(Container.Width, HEIGHT);
            X = 0;
            Alpha = 1;

            Button = new MultiplayerSettingsItemButton(Container, (o, e) =>
            {
                if (Item.CreateDialog != null)
                    DialogManager.Show(Item.CreateDialog());
            })
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
                Tint = Colors.MainAccent,
                UsePreviousSpriteBatchOptions = true
            };

            Name = new SpriteTextBitmap(FontsBitmap.GothamRegular, item.Name)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true,
                FontSize = 16,
                X = 22
            };

            item.Value.Parent = this;
            item.Value.UsePreviousSpriteBatchOptions = true;
            item.Value.Alignment = Alignment.MidRight;
            item.Value.X = -Name.X - 4;
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
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(IMultiplayerSettingsItem item, int index)
        {
        }

        public override void Destroy()
        {
            Button.Destroy();
            ButtonManager.Remove(Button);
            Item.Destroy();
            base.Destroy();
        }
    }
}