using System;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings
{
    public interface IMultiplayerSettingsItem
    {
        string Name { get; set; }
        SpriteTextBitmap Value { get; set; }
        Func<MenuDialog> CreateDialog { get; }
        void Destroy();
    }
}