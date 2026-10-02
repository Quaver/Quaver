using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Options;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    /// <summary>
    ///     Generates a canonical skin.yml containing only properties opted into skin-author editing.
    /// </summary>
    public sealed class OptionsItemGenerateSkinV2Config : OptionsItem
    {
        private RoundedButton Button { get; }

        public OptionsItemGenerateSkinV2Config(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new RoundedButton
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                Tint = ColorHelper.HexToColor("#0FBAE5")
            };

            Button.SetLabel(FontManager.GetWobbleFont(Fonts.InterSemiBold), "GENERATE", 18, Color.White);
            Button.Clicked += OnClicked;
        }

        private void OnClicked(object sender, EventArgs args) =>
            OptionsActions.GenerateSkinConfig(focused => Focused = focused);
    }
}
