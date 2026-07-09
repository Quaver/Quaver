using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Graphics.Buttons;
using Quaver.Shared.Graphics.Notifications;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemSuggestDifficulty : OptionsItem
    {
        /// <summary>
        /// </summary>
        private RoundedButton Button { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerWidth"></param>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItemSuggestDifficulty(RectangleF containerRect, string name) : base(containerRect, name)
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

            Button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "CALIBRATE", 18, Color.White);

            Button.Clicked += (sender, args) =>
            {
                foreach (GameMode mode in ModeHelper.AllModes)
                    UpdateSuggestedDifficulty(mode);

                NotificationManager.Show(NotificationLevel.Info, $"Suggested difficulties have been recalculated.");
            };
        }

        private static void UpdateSuggestedDifficulty(GameMode mode)
        {
            var profile = UserProfileDatabaseCache.Selected.Value;
            profile.PopulateStats();

            var rating = profile.Stats[mode].OverallRating;
            var diff = (int)(rating / 20f * 10);

            ConfigManager.PrioritizedMapDifficulty[mode].Value = diff;
        }
    }
}
