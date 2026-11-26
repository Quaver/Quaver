using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.Leaderboard;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection.UI.Panel.Leaderboard
{
    internal enum LeaderboardTextField
    {
        Rank,
        Clan,
        Username,
        Time,
        Rating,
        ComboAccuracy
    }

    /// <summary>
    ///     Owns the shared text atlas and draw layers used by every leaderboard row
    /// </summary>
    internal sealed class LeaderboardRowsView : IDisposable
    {
        private SkinV2SelectionLeaderboardConfig Config { get; }
        private SharedTextAtlas TextAtlasValue { get; set; }

        internal SharedTextAtlas TextAtlas => TextAtlasValue ?? throw new InvalidOperationException("Leaderboard text was requested before scores were assigned.");

        internal Container BackgroundLayer { get; }
        internal Container GradeLayer { get; }
        internal Container AvatarLayer { get; }
        internal Container FlagLayer { get; }
        internal Container ModifierLayer { get; }
        internal Container ClockLayer { get; }
        internal Container TextLayer { get; }
        internal Container BeatabilityLayer { get; }

        internal LeaderboardRowsView(Container parent, SkinV2SelectionLeaderboardConfig config)
        {
            Config = config;
            BackgroundLayer = CreateLayer(parent);
            GradeLayer = CreateLayer(parent);
            AvatarLayer = CreateLayer(parent);
            FlagLayer = CreateLayer(parent);
            ModifierLayer = CreateLayer(parent);
            ClockLayer = CreateLayer(parent);
            TextLayer = CreateLayer(parent);
            BeatabilityLayer = CreateLayer(parent);
        }

        /// <summary>
        ///     Renders every score label once so pooled rows only switch atlas regions while scrolling.
        /// </summary>
        internal void SetScores(IReadOnlyList<Score> scores)
        {
            TextAtlasValue?.Dispose();
            TextAtlasValue = null;

            if (scores == null || scores.Count == 0)
                return;

            var font = FontManager.GetWobbleFont(Config.Font);
            var definitions = new List<SharedTextAtlasDefinition>(scores.Count * 6);

            for (var i = 0; i < scores.Count; i++)
            {
                var score = scores[i];
                var accuracy = GetDisplayedAccuracy(score);

                AddDefinition(definitions, i, LeaderboardTextField.Rank, $"{i + 1}.", font, Config.FontSize);
                AddDefinition(definitions, i, LeaderboardTextField.Clan, string.IsNullOrEmpty(score.ClanTag) ? string.Empty : $"[{score.ClanTag}]", font, Config.FontSize);
                AddDefinition(definitions, i, LeaderboardTextField.Username, score.Name ?? string.Empty, font, Config.FontSize);
                AddDefinition(definitions, i, LeaderboardTextField.Time, FormatElapsedTime(score.DateTime), font,  Config.MetadataFontSize);
                AddDefinition(definitions, i, LeaderboardTextField.Rating, StringHelper.RatingToString(score.PerformanceRating), font, Config.RatingFontSize);
                AddDefinition(definitions, i, LeaderboardTextField.ComboAccuracy, $"x{score.MaxCombo:N0} | {StringHelper.AccuracyToString(accuracy)}", font, Config.FontSize);
            }

            TextAtlasValue = new SharedTextAtlas(definitions);
        }

        internal void Resize(ScalableVector2 size)
        {
            BackgroundLayer.Size = size;
            GradeLayer.Size = size;
            AvatarLayer.Size = size;
            FlagLayer.Size = size;
            ModifierLayer.Size = size;
            ClockLayer.Size = size;
            TextLayer.Size = size;
            BeatabilityLayer.Size = size;
        }

        internal static string GetTextKey(int index, LeaderboardTextField field) => $"{index}:{(int) field}";

        internal static float GetDisplayedAccuracy(Score score) => (float)
            (ConfigManager.LeaderboardSection.Value == LeaderboardType.Local && ConfigManager.LeaderboardRankedAccuracy.Value ? score.RankedAccuracy : score.Accuracy);

        /// <summary>
        ///     Formats score age
        /// </summary>
        internal static string FormatElapsedTime(string value)
        {
            if (!DateTime.TryParse(value, out var date))
                return string.Empty;

            var difference = DateTime.Now - date;
            if (difference.TotalDays > 365)
                return $"{(int) (difference.TotalDays / 365)}y";
            if (difference.TotalDays > 30)
                return $"{(int) (difference.TotalDays / 30)}mo";
            if (difference.TotalDays > 7)
                return $"{(int) (difference.TotalDays / 7)}w";
            if (difference.TotalDays > 0)
                return $"{(int) difference.TotalDays}d";
            if (difference.TotalHours > 0)
                return $"{(int) difference.TotalHours}h";
            if (difference.TotalMinutes > 0)
                return $"{(int) difference.TotalMinutes}m";

            var seconds = (int) difference.TotalSeconds;
            return seconds <= 0 ? "now" : $"{seconds}s";
        }

        public void Dispose()
        {
            TextAtlasValue?.Dispose();
            TextAtlasValue = null;
        }

        private static Container CreateLayer(Container parent) => new()
        {
            Parent = parent,
            UsePreviousSpriteBatchOptions = true
        };

        private static void AddDefinition(ICollection<SharedTextAtlasDefinition> definitions, int index, LeaderboardTextField field, string text, WobbleFontStore font, int fontSize) =>
            definitions.Add(new SharedTextAtlasDefinition(GetTextKey(index, field), text, font, fontSize));
    }
}
