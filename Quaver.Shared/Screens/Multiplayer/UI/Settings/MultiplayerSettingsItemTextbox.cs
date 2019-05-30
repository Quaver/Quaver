using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings
{
    public class MultiplayerSettingsItemTextbox : MultiplayerSettingsItem
    {
        public MultiplayerSettingsItemTextbox(PoolableScrollContainer<MultiplayerSettingsContainer> container, MultiplayerSettingsContainer item, int index)
            : base(container, item, index)
        {
        }

        public override void UpdateContent(MultiplayerSettingsContainer item, int index)
        {
        }
    }
}