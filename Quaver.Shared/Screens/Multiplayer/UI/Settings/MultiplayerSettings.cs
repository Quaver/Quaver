using System.Collections.Generic;
using Quaver.Shared.Assets;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings
{
    public class MultiplayerSettings : Sprite
    {
        public MultiplayerSettingsScrollContainer ScrollContainer { get; }

        public MultiplayerSettings(List<MultiplayerSettingsContainer> settingsContainer)
        {
            Image = UserInterface.MultiplayerSettingaPanel;
            Size = new ScalableVector2(650, 338);

            ScrollContainer = new MultiplayerSettingsScrollContainer(settingsContainer)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
            };
        }
    }
}