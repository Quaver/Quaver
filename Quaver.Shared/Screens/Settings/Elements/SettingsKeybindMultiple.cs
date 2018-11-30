using Microsoft.Xna.Framework;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsKeybindMultiple : SettingsItem
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        public SettingsKeybindMultiple(SettingsDialog dialog, string name) : base(dialog, name)
        {
            var btn = new BorderedTextButton("Change", Color.White)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -20,
                UsePreviousSpriteBatchOptions = true,
                Text = { UsePreviousSpriteBatchOptions = true }
            };

            btn.Height -= 6;
        }
    }
}