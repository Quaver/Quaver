using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Backgrounds;
using Wobble.Graphics.Buttons;
using Quaver.Shared.Graphics.Notifications;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemDetectOtherGames : OptionsItem
    {
        /// <summary>
        /// </summary>
        private RoundedButton Button { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItemDetectOtherGames(RectangleF containerRect, string name) : base(containerRect, name)
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

            Button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "DETECT", 18, Color.White);

            Button.Clicked += (sender, args) =>
            {
                OtherGameMapDatabaseCache.FindOsuStableInstallation();
                OtherGameMapDatabaseCache.FindEtternaInstallation();

                var count = 0;

                if (!string.IsNullOrEmpty(ConfigManager.OsuDbPath.Value))
                    count++;

                if (!string.IsNullOrEmpty(ConfigManager.EtternaDbPath.Value))
                    count++;

                if (count != 0)
                {
                    var message = $"Detected song databases for {count} other installed game";

                    if (count > 1)
                        message += "s.";
                    else
                        message += ".";

                    NotificationManager.Show(NotificationLevel.Success, message);
                    Logger.Important(message, LogType.Runtime);
                    ConfigManager.AutoLoadOsuBeatmaps.Value = true;
                }
                else
                {
                    var message = $"Could not find song databases for other installed games";
                    NotificationManager.Show(NotificationLevel.Warning, message);
                    Logger.Important(message, LogType.Runtime);
                }
            };
        }
    }
}