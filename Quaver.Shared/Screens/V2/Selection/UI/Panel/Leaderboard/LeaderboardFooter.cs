using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI.Leaderboard;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection.UI.Panel.Leaderboard
{
    /// <summary>
    ///     Displays the selected leaderboard's personal best and the score directly ahead of it
    /// </summary>
    internal sealed class LeaderboardFooter : Container
    {
        private const float HeaderContentGap = 5;

        private SkinV2SelectionLeaderboardConfig Config { get; }
        private RoundedButton PersonalBestLabel { get; }
        private RoundedButton PositionSurface { get; }
        private FlexContainer PositionLayout { get; }
        private Sprite Trophy { get; }
        private SpriteTextPlus PositionRank { get; }
        private SpriteTextPlus PositionTotal { get; }
        private Container AheadGroup { get; }
        private RoundedButton AheadLabel { get; }
        private RoundedButton AheadStatsSurface { get; }
        private FlexContainer AheadStatsLayout { get; }
        private SpriteTextPlus AheadSummary { get; }
        private List<Sprite> AheadModifierIcons { get; } = new();
        private LeaderboardList PersonalBestRow { get; }
        private SpriteTextPlus NoPersonalBest { get; }

        internal LeaderboardFooter(SkinV2SelectionLeaderboardConfig config)
        {
            Config = config;

            PersonalBestLabel = CreateLabelSurface(SelectionLocalization.Get("Personal Best"), 1, Config.FooterLabelColor);
            PersonalBestLabel.Parent = this;

            PositionSurface = CreateSurface(1, Config.ComponentColor);
            PositionSurface.Parent = this;
            PositionSurface.CornerRadii = new RoundedRectCornerRadii(0, Config.CornerRadius, Config.CornerRadius, 0);

            PositionLayout = CreateHeaderLayout(PositionSurface);
            Trophy = new Sprite
            {
                Parent = PositionLayout,
                Size = new ScalableVector2(20, 18),
                Region = CapsuleIcons.Get(CapsuleIcon.Trophy),
                Tint = SkinV2Color.Parse(Config.RatingColor),
                UsePreviousSpriteBatchOptions = true
            };
            PositionRank = CreateHeaderText(PositionLayout, "#1", Config.RatingColor);
            PositionTotal = CreateHeaderText(PositionLayout, "of #1", Config.MetadataColor);

            AheadGroup = new Container { Parent = this };
            AheadLabel = CreateLabelSurface("Top #1", 1, Config.FooterLabelColor);
            AheadLabel.Parent = AheadGroup;
            AheadLabel.CornerRadii = new RoundedRectCornerRadii(Config.CornerRadius, 0, 0, Config.CornerRadius);

            AheadStatsSurface = CreateSurface(1, Config.ComponentColor);
            AheadStatsSurface.Parent = AheadGroup;
            AheadStatsSurface.CornerRadii = new RoundedRectCornerRadii(0, Config.CornerRadius, Config.CornerRadius, 0);
            AheadStatsLayout = CreateHeaderLayout(AheadStatsSurface);
            AheadSummary = CreateHeaderText(AheadStatsLayout, string.Empty, Config.TextColor);

            PersonalBestRow = new LeaderboardList(Config, false, false)
            {
                Parent = this
            };
            PersonalBestRow.SetActive(false);

            NoPersonalBest = new SpriteTextPlus(FontManager.GetWobbleFont(Config.Font), SelectionLocalization.Get("No Personal Best Set"), Config.FontSize)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Tint = SkinV2Color.Parse(Config.TextColor),
                UsePreviousSpriteBatchOptions = true
            };

            SizeChanged += OnSizeChanged;
            SetScores(default);
        }

        /// <summary>
        ///     Rebinds the footer to the personal best returned by the selected leaderboard
        /// </summary>
        internal void SetScores(FetchedScoreStore store)
        {
            var scores = store.Scores ?? new List<Score>();

            var personalBest = store.PersonalBest;
            var hasPersonalBest = personalBest != null;
            PersonalBestLabel.Visible = true;
            PersonalBestLabel.UpdateWhenInvisible = true;
            NoPersonalBest.Visible = !hasPersonalBest;
            NoPersonalBest.UpdateWhenInvisible = !hasPersonalBest;

            PersonalBestRow.SetScores(personalBest == null ? new List<Score>() : new List<Score> { personalBest });
            PersonalBestRow.Visible = hasPersonalBest;
            PersonalBestRow.UpdateWhenInvisible = hasPersonalBest;

            var rankIndex = personalBest == null ? -1 : scores.FindIndex(score => SameScore(score, personalBest));
            var hasPosition = rankIndex >= 0;
            PositionSurface.Visible = hasPosition;
            PositionSurface.UpdateWhenInvisible = hasPosition;
            PersonalBestLabel.CornerRadii = hasPosition ? new RoundedRectCornerRadii(Config.CornerRadius, 0, 0, Config.CornerRadius) : RoundedRectCornerRadii.All(Config.CornerRadius);

            if (hasPosition)
            {
                PositionRank.Text = $"#{rankIndex + 1}";
                PositionTotal.Text = $"of #{scores.Count}";
                ResizePositionSurface();
            }

            var hasScoreAhead = rankIndex > 0;
            AheadGroup.Visible = hasScoreAhead;
            AheadGroup.UpdateWhenInvisible = hasScoreAhead;
            if (hasScoreAhead)
                UpdateScoreAhead(scores[rankIndex - 1], rankIndex);

            LayoutFooter();
        }

        private void UpdateScoreAhead(Score score, int rank)
        {
            AheadLabel.SetLabel(FontManager.GetWobbleFont(Config.Font), $"Top #{rank}", Config.FontSize, SkinV2Color.Parse(Config.TextColor));
            AheadLabel.Width = AheadLabel.Label.Width + Config.Padding * 2;

            foreach (var icon in AheadModifierIcons)
                icon.Destroy();
            AheadModifierIcons.Clear();

            foreach (var region in LeaderboardRow.GetModifierRegions(score))
            {
                AheadModifierIcons.Add(new Sprite
                {
                    Parent = AheadStatsLayout,
                    Size = new ScalableVector2(Config.ModifierIconWidth, Config.ModifierIconHeight),
                    Region = region,
                    UsePreviousSpriteBatchOptions = true
                });
            }

            AheadSummary.Text = $"{StringHelper.RatingToString(score.PerformanceRating)} | " + StringHelper.AccuracyToString(LeaderboardRowsView.GetDisplayedAccuracy(score));
            ResizeAheadGroup();
        }

        private void ResizePositionSurface()
        {
            SetFlexItem(PositionLayout, Trophy);
            SetFlexItem(PositionLayout, PositionRank);
            SetFlexItem(PositionLayout, PositionTotal);

            var contentWidth = Trophy.Width + PositionRank.Width + PositionTotal.Width + HeaderContentGap * 2;
            PositionSurface.Width = contentWidth + Config.Padding * 2;
            PositionLayout.Size = PositionSurface.Size;

            PositionLayout.RefreshLayout();
        }

        private void ResizeAheadGroup()
        {
            var contentWidth = AheadSummary.Width;
            foreach (var icon in AheadModifierIcons)
            {
                SetFlexItem(AheadStatsLayout, icon, 0);
                contentWidth += icon.Width;
            }

            var itemCount = AheadModifierIcons.Count + 1;
            contentWidth += HeaderContentGap * Math.Max(0, itemCount - 1);
            SetFlexItem(AheadStatsLayout, AheadSummary, 1);

            AheadStatsSurface.Width = contentWidth + Config.Padding * 2;
            AheadStatsSurface.X = AheadLabel.Width;
            AheadStatsLayout.Size = AheadStatsSurface.Size;

            AheadStatsLayout.RefreshLayout();
            AheadGroup.Size = new ScalableVector2(AheadLabel.Width + AheadStatsSurface.Width, Config.HeaderHeight);
        }

        private void OnSizeChanged(object sender, ScalableVector2 size)
        {
            PersonalBestRow.ApplySize(new ScalableVector2(
                Math.Max(1, Width - Config.Padding * 2), 
                Config.RowHeight));

            LayoutFooter();
        }

        private void LayoutFooter()
        {
            PersonalBestLabel.Position = new ScalableVector2(
                Config.Padding, 
                Config.Padding);

            PositionSurface.Position = new ScalableVector2(
                PersonalBestLabel.X + PersonalBestLabel.Width, 
                Config.Padding);

            AheadGroup.Position = new ScalableVector2(
                Math.Max(Config.Padding, Width - Config.Padding - AheadGroup.Width), 
                Config.Padding);

            PersonalBestRow.Position = new ScalableVector2(
                Config.Padding,
                Config.Padding + Config.HeaderHeight + Config.Gap);
        }

        private RoundedButton CreateLabelSurface(string text, float minimumWidth, string color)
        {
            var surface = CreateSurface(minimumWidth, color);
            surface.SetLabel(FontManager.GetWobbleFont(Config.Font), text, Config.FontSize, SkinV2Color.Parse(Config.TextColor));
            surface.Width = Math.Max(minimumWidth, surface.Label.Width + Config.Padding * 2);

            return surface;
        }

        private RoundedButton CreateSurface(float width, string color) => new()
        {
            Size = new ScalableVector2(width, Config.HeaderHeight),
            CornerRadius = Config.CornerRadius,
            Tint = SkinV2Color.Parse(color),
            PerformHoverFade = false,
            IsClickable = false,
            IsInteractionEnabled = false
        };

        private FlexContainer CreateHeaderLayout(RoundedButton parent) => new()
        {
            Parent = parent,
            Size = parent.Size,
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            JustifyContent = FlexJustifyContent.Center,
            ColumnGap = HeaderContentGap,
            UsePreviousSpriteBatchOptions = true
        };

        private SpriteTextPlus CreateHeaderText(FlexContainer parent, string text, string color) =>
            new(FontManager.GetWobbleFont(Config.Font), text, Config.FontSize)
            {
                Parent = parent,
                Tint = SkinV2Color.Parse(color),
                UsePreviousSpriteBatchOptions = true
            };

        private static void SetFlexItem(FlexContainer layout, Drawable item, int order = 0) =>
            layout.SetItemOptions(item, new FlexItemOptions { Basis = item.Width, Shrink = 0, Order = order });

        private static bool SameScore(Score score, Score personalBest)
        {
            if (ReferenceEquals(score, personalBest))
                return true;

            if (score == null || personalBest == null)
                return false;

            if (score.Id > 0 && personalBest.Id > 0)
                return score.Id == personalBest.Id;

            return score.Name == personalBest.Name && score.DateTime == personalBest.DateTime && score.TotalScore == personalBest.TotalScore && score.Mods == personalBest.Mods;
        }
    }
}
