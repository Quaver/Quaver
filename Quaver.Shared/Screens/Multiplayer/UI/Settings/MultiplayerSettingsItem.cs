using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings
{
    public abstract class MultiplayerSettingsItem : PoolableSprite<MultiplayerSettingsContainer>
    {
        public sealed override int HEIGHT { get; } = 42;

        protected MultiplayerSettingsItem(PoolableScrollContainer<MultiplayerSettingsContainer> container, MultiplayerSettingsContainer item, int index)
            : base(container, item, index)
        {
            Size = new ScalableVector2(Container.Width, HEIGHT);
            X = 2;
            Alpha = 1;
        }
    }
}