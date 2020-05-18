using System.IO;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemExportSkin : OptionsItem
    {
        /// <summary>
        /// </summary>
        private IconButton Button { get; }

        public OptionsItemExportSkin(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new IconButton(UserInterface.OptionsExportSkinButton)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                UsePreviousSpriteBatchOptions = true
            };

            Button.Clicked += (sender, args) => SkinManager.Export();
        }
    }
}