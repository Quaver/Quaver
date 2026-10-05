using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Wobble.Graphics.Buttons;
using Quaver.Shared.Graphics.Notifications;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Ranked;
using Quaver.Shared.Options;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemUpdateRankedStatuses : OptionsItem
    {
        /// <summary>
        /// </summary>
        private RoundedButton Button { get; }

        public OptionsItemUpdateRankedStatuses(RectangleF containerRect, string name) : base(containerRect, name)
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

            Button.Clicked += (sender, args) => OptionsActions.UpdateRankedStatuses();
        }

        public static void Run(bool fromOptions = true) => OptionsActions.UpdateRankedStatuses(fromOptions);
    }
}
