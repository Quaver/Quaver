using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Selection.UI.Profile
{
    public class LocalProfileTable : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<UserProfile> Profile { get; }

        /// <summary>
        /// </summary>
        private List<LocalProfileTableItem> Items { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="size"></param>
        public LocalProfileTable(Bindable<UserProfile> profile, ScalableVector2 size)
        {
            Profile = profile;
            Size = size;
            Alpha = 0;
            SetChildrenAlpha = true;
            Tint = Color.Transparent;

            InitializeItems();
        }

        /// <summary>
        /// </summary>
        private void InitializeItems()
        {
            var mode = ConfigManager.SelectedGameMode?.Value ?? GameMode.Keys4;
            var stats = Profile.Value.Stats[mode];

            var topScore = stats.GetTopScore();
            var perf = stats.JudgementCounts[Judgement.Perf] == 0 ? 1 : stats.JudgementCounts[Judgement.Perf];
            var ratio = stats.JudgementCounts[Judgement.Marv] / (float) perf;

            Items = new List<LocalProfileTableItem>();

            if (Profile.Value.IsOnline)
            {
                Items.Add(new LocalProfileTableItem(SelectionLocalization.Get("Global Rank"), GetRankString(stats.GlobalRank)));
                Items.Add(new LocalProfileTableItem(SelectionLocalization.Get("Country Rank"), GetRankString(stats.CountryRank)));
            }

            if (!Profile.Value.IsOnline)
            {
                Items.Add(new LocalProfileTableItem(SelectionLocalization.Get("Highest Rated Score"),
                    $"{(topScore != null ? StringHelper.RatingToString(topScore.PerformanceRating) : SelectionLocalization.Get("Never Played"))}"));

                Items.Add(new LocalProfileTableItem(SelectionLocalization.Get("Total Pauses"), $"{stats.PauseCount:n0}"));
            }

            Items.AddRange(new []
            {
                new LocalProfileTableItem(SelectionLocalization.Get("Overall Rating"), StringHelper.RatingToString(stats.OverallRating)),
                new LocalProfileTableItem(SelectionLocalization.Get("Overall Accuracy"), StringHelper.AccuracyToString((float) stats.OverallAccuracy)),
                new LocalProfileTableItem(SelectionLocalization.Get("Total Score"), $"{stats.TotalScore:n0}"),
                new LocalProfileTableItem(SelectionLocalization.Get("Total Hits"), $"{stats.JudgementCounts.Values.Sum() - stats.JudgementCounts[Judgement.Miss]:n0}"),
                new LocalProfileTableItem(SelectionLocalization.Get("Play Count"), $"{stats.PlayCount:n0}"),
                new LocalProfileTableItem(SelectionLocalization.Get("Pass/Failure Count"), $"{stats.PlayCount - stats.FailCount:n0}/{stats.FailCount:n0}"),
                new LocalProfileTableItem(SelectionLocalization.Get("Max Combo"), $"{stats.MaxCombo:n0}x"),
                new LocalProfileTableItem(SelectionLocalization.Get("Marvelous/Perfect Ratio"), $"{ratio:0.00}"),
            });

            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];

                item.Parent = this;
                item.Size = new ScalableVector2(Width, Height / Items.Count);
                item.Y = item.Height * i;
                item.Tint = i % 2 == 0 ? ColorHelper.HexToColor("#363636") : ColorHelper.HexToColor("#242424");
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        private string GetRankString(int rank)
        {
            if (rank == -1)
                return SelectionLocalization.Get("Never Played");

            return $"#{rank:n0}";
        }
    }
}
