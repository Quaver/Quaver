using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings
{
    public abstract class MultiplayerSettingsText : IMultiplayerSettingsItem
    {
        /// <summary>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        public SpriteTextBitmap Value { get; set; }

        /// <summary>
        /// </summary>
        public Func<MenuDialog> CreateDialog { get; protected set; }

        /// <summary>
        /// </summary>
        /// <param name="createDialog"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public MultiplayerSettingsText(string name, string value, Func<MenuDialog> createDialog = null)
        {
            CreateDialog = createDialog;
            Name = name;

            Value = new SpriteTextBitmap(FontsBitmap.GothamRegular, value)
            {
                FontSize = 16
            };
        }

        /// <summary>
        /// </summary>
        public virtual void Destroy() => Value.Destroy();

        /// <summary>
        /// </summary>
        /// <param name="boolean"></param>
        /// <returns></returns>
        public static string BooleanToYesOrNo(bool boolean) => boolean ? "Yes" : "No";
    }
}