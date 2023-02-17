using System;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemSuggestDifficulty : OptionsItem
    {
        /// <summary>
        /// </summary>
        private IconButton Button { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerWidth"></param>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItemSuggestDifficulty(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new IconButton(UserInterface.OptionsCalibrateOffsetButton)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                UsePreviousSpriteBatchOptions = true
            };

            Button.Clicked += (sender, args) =>
            {
                foreach (GameMode mode in Enum.GetValues(typeof(GameMode)))
                    UpdateSuggestedDifficulty(mode);

                NotificationManager.Show(NotificationLevel.Info, $"Suggested difficulties have been recalculated.");
            };
        }

        private static void UpdateSuggestedDifficulty(GameMode mode)
        {
            var profile = UserProfileDatabaseCache.Selected.Value;
            profile.PopulateStats();

            var rating = profile.Stats[mode].OverallRating;
            var diff = (int) (rating / 20f * 10);

            switch (mode)
            {
                case GameMode.Keys4:
                    ConfigManager.PrioritizedMapDifficulty4K.Value = diff;
                    break;
                case GameMode.Keys7:
                    ConfigManager.PrioritizedMapDifficulty7K.Value = diff;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}
