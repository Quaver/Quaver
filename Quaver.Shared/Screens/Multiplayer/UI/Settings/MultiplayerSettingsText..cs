using Quaver.Shared.Assets;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings
{
    public abstract class MultiplayerSettingsText : IMultiplayerSettingsItem
    {
        public string Name { get; set; }

        public SpriteTextBitmap Value { get; set; }

        public MultiplayerSettingsText(string name, string value)
        {
            Name = name;

            Value = new SpriteTextBitmap(FontsBitmap.GothamRegular, value)
            {
                FontSize = 16
            };
        }

        public virtual void Destroy() => Value.Destroy();

        public static string BooleanToYesOrNo(bool boolean) => boolean ? "Yes" : "No";
    }
}