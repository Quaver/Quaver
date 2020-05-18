using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemDetectOtherGames : OptionsItem
    {
        /// <summary>
        /// </summary>
        private IconButton Button { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItemDetectOtherGames(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new IconButton(UserInterface.DetectOtherGamesButton)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                UsePreviousSpriteBatchOptions = true
            };

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