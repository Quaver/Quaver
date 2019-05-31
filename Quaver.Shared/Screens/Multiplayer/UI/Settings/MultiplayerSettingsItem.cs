using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings
{
    public class MultiplayerSettingsItem : PoolableSprite<IMultiplayerSettingsItem>
    {
        public sealed override int HEIGHT { get; } = 42;

        public SpriteTextBitmap Name { get; }

        public MultiplayerSettingsItem(PoolableScrollContainer<IMultiplayerSettingsItem> container, IMultiplayerSettingsItem item, int index)
            : base(container, item, index)
        {
            Size = new ScalableVector2(Container.Width, HEIGHT);
            X = 0;
            Alpha = 1;

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

        public override void UpdateContent(IMultiplayerSettingsItem item, int index)
        {
        }

        public virtual void Destroy()
        {
            Item.Destroy();
            base.Destroy();
        }
    }
}