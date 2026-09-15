using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Wobble.Graphics.Buttons;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemUpdateOnlineOffsets : OptionsItem
    {
        /// <summary>
        /// </summary>
        private RoundedButton Button { get; }

        public OptionsItemUpdateOnlineOffsets(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new RoundedButton
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                Tint = ColorHelper.HexToColor("#F2994A")
            };

            Button.SetLabel(FontManager.GetWobbleFont(Fonts.InterSemiBold), "UPDATE", 18, Color.White);

            Button.Clicked += (sender, args) => MapMetadataUpdater.UpdateOnlineOffsets();
        }
    }
}
