using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Quaver.Server.Client;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI.Leaderboard;
using Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings;
using Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings.Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Managers;
using Wobble.Scheduling;

namespace Quaver.Shared.Screens.V2.Selection.UI.Panel.Leaderboard
{
    /// <summary>
    ///     Leaderboard content section
    /// </summary>
    internal sealed class LeaderboardPanel : Container
    {
        private SkinV2SelectionLeaderboardConfig Config { get; }
        private RoundedButton LeftControl { get; }
        private V2Dropdown<LeaderboardType> TypeDropdown { get; }
        private LeaderboardList Scores { get; }
        private LeaderboardFooter Footer { get; }
        private LoadingWheel LoadingWheel { get; }
        private TaskHandler<Map, FetchedScoreStore> FetchScoreTask { get; }

        internal LeaderboardPanel(SkinV2SelectionLeaderboardConfig config, Container overlayHost, Container footerHost)
        {
            Config = config;
            Footer = new LeaderboardFooter(Config)
            {
                Parent = footerHost,
                Size = footerHost.Size
            };
            footerHost.SizeChanged += OnFooterSizeChanged;

            LeftControl = new RoundedButton
            {
                Parent = this,
                Size = new ScalableVector2(Config.LeftControlWidth, Config.HeaderHeight),
                CornerRadius = Config.CornerRadius,
                Tint = SkinV2Color.Parse(Config.ComponentColor),
                PerformHoverFade = false,
                IsClickable = false,
                IsInteractionEnabled = false
            };

            TypeDropdown = new V2Dropdown<LeaderboardType>(Config.TypeDropdownWidth, ConfigManager.LeaderboardSection, CreateLeaderboardEntries(), FontManager.GetWobbleFont(Config.Font), CreateDropdownStyle(), overlayHost)
            {
                Parent = this,
                Size = new ScalableVector2(Config.TypeDropdownWidth, Config.HeaderHeight)
            };

            Scores = new LeaderboardList(Config)
            {
                Parent = this
            };

            LoadingWheel = new LoadingWheel
            {
                Parent = this,
                Size = new ScalableVector2(50, 50),
                Alpha = 0
            };

            FetchScoreTask = new TaskHandler<Map, FetchedScoreStore>(FetchScores);
            FetchScoreTask.OnCompleted += OnFetchedScores;

            MapManager.Selected.ValueChanged += OnMapChanged;
            ConfigManager.LeaderboardSection.ValueChanged += OnLeaderboardSectionChanged;
            ConfigManager.DisplayFailedLocalScores.ValueChanged += OnDisplayFailedLocalScoresChanged;
            ModManager.ModsChanged += OnModsChanged;
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
            ScoreDatabaseCache.ScoreDeleted += OnScoreDeleted;
            ScoreDatabaseCache.LocalMapScoresDeleted += OnMapLocalScoresDeleted;

            SizeChanged += OnSizeChanged;
            RequestScores();
        }

        /// <summary>
        ///     Enables leaderboard scrolling only while its tab is displayed
        /// </summary>
        internal void SetActive(bool active) => Scores.SetActive(active);

        private static IReadOnlyList<DropdownEntry<LeaderboardType>> CreateLeaderboardEntries() =>
            Enum.GetValues(typeof(LeaderboardType)).Cast<LeaderboardType>()
                .Select(type => (DropdownEntry<LeaderboardType>) new DropdownOption<LeaderboardType>(type, SelectionLocalization.Get(type.ToString())))
                .ToList();

        private SkinV2DropdownConfig CreateDropdownStyle() => new()
        {
            Height = Config.HeaderHeight,
            ItemHeight = Config.HeaderHeight,
            HorizontalPadding = Config.Padding,
            CornerRadius = Config.CornerRadius,
            Font = Config.Font,
            FontSize = Config.FontSize,
            TriggerColor = Config.ComponentColor,
            ItemColor = Config.ComponentColor,
            HoverColor = Config.HoverColor,
            SelectedItemColor = Config.SelectedColor,
            TextColor = Config.TextColor,
            SelectedTextColor = Config.TextColor,
            IconColor = Config.TextColor,
            ScrollbarWidth = Config.ScrollbarWidth,
            ScrollbarColor = Config.ScrollbarColor,
            ScrollbarTrackColor = Config.ScrollbarTrackColor
        };

        /// <summary>
        ///     Clears the old rows and starts the delayed score fetch
        /// </summary>
        private void RequestScores()
        {
            Scores.SetScores(new List<Score>());
            Footer.SetScores(default);

            LoadingWheel.ClearAnimations();
            LoadingWheel.FadeTo(1, Easing.Linear, 250);

            FetchScoreTask.Run(MapManager.Selected.Value, 400);
        }

        /// <summary>
        ///     Retrieves the selected leaderboard type without coupling score loading to row rendering
        /// </summary>
        private static FetchedScoreStore FetchScores(Map map, CancellationToken token)
        {
            if (map == null || token.IsCancellationRequested)
                return new FetchedScoreStore(new List<Score>());

            FetchedScoreStore scores = ConfigManager.LeaderboardSection.Value switch
            {
                LeaderboardType.Local => new ScoreFetcherLocal().Fetch(map),
                LeaderboardType.Global => new ScoreFetcherGlobal().Fetch(map),
                LeaderboardType.Mods => new ScoreFetcherMods().Fetch(map),
                LeaderboardType.Country => new ScoreFetcherCountry().Fetch(map),
                LeaderboardType.Rate => new ScoreFetcherRate().Fetch(map),
                LeaderboardType.Friends => new ScoreFetcherFriends().Fetch(map),
                LeaderboardType.All => new ScoreFetcherAll().Fetch(map),
                LeaderboardType.Clan => new ScoreFetcherClan().Fetch(map),
                _ => new FetchedScoreStore(new List<Score>())
            };

            if (OnlineManager.CurrentGame == null && !token.IsCancellationRequested)
            {
                map.Scores.Value = scores.Scores;
                ScoresHelper.SetRatingProcessors(map.Scores.Value);
            }

            return scores;
        }

        private void OnFetchedScores(object sender, TaskCompleteEventArgs<Map, FetchedScoreStore> args) =>
            ScheduleUpdate(() =>
            {
                if (IsDisposed || args.Input != MapManager.Selected.Value)
                    return;

                LoadingWheel.ClearAnimations();
                LoadingWheel.FadeTo(0, Easing.Linear, 250);

                Scores.SetScores(args.Result.Scores ?? new List<Score>());
                Footer.SetScores(args.Result);
            });

        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> args) => RequestScores();

        private void OnLeaderboardSectionChanged(object sender, BindableValueChangedEventArgs<LeaderboardType> args) => RequestScores();

        private void OnDisplayFailedLocalScoresChanged(object sender, BindableValueChangedEventArgs<bool> args)
        {
            if (ConfigManager.LeaderboardSection.Value == LeaderboardType.Local)
                RequestScores();
        }

        private void OnModsChanged(object sender, ModsChangedEventArgs args)
        {
            if (ConfigManager.LeaderboardSection.Value is LeaderboardType.Mods or LeaderboardType.Rate)
                RequestScores();
        }

        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> args)
        {
            if (args.Value == ConnectionStatus.Connected && ConfigManager.LeaderboardSection.Value != LeaderboardType.Local)
                RequestScores();
        }

        private void OnScoreDeleted(object sender, ScoreDeletedEventArgs args)
        {
            if (ConfigManager.LeaderboardSection.Value == LeaderboardType.Local)
                RequestScores();
        }

        private void OnMapLocalScoresDeleted(object sender, LocalScoresDeletedEventArgs args)
        {
            if (ConfigManager.LeaderboardSection.Value == LeaderboardType.Local)
                RequestScores();
        }

        private void OnFooterSizeChanged(object sender, ScalableVector2 size) => Footer.Size = size;

        /// <summary>
        ///     Updates the header controls and score rows positions
        /// </summary>
        private void OnSizeChanged(object sender, ScalableVector2 size)
        {
            LeftControl.Position = new ScalableVector2(Config.Padding, Config.Padding);
            TypeDropdown.Position = new ScalableVector2(
                Math.Max(Config.Padding, Width - Config.Padding - Config.TypeDropdownWidth),
                Config.Padding);

            var scoresY = Config.Padding + Config.HeaderHeight + Config.Gap;
            Scores.Position = new ScalableVector2(Config.Padding, scoresY);

            var scoresWidth = Math.Max(1, Width - Config.Padding);
            var scoresHeight = Math.Max(1, Height - scoresY - Config.Padding);
            Scores.ApplySize(new ScalableVector2(scoresWidth, scoresHeight));

            LoadingWheel.Position = new ScalableVector2(
                Scores.X + (Scores.Width - LoadingWheel.Width) / 2f,
                Scores.Y + (Scores.Height - LoadingWheel.Height) / 2f);
        }

        public override void Destroy()
        {
            FetchScoreTask.OnCompleted -= OnFetchedScores;
            FetchScoreTask.Dispose();

            MapManager.Selected.ValueChanged -= OnMapChanged;
            ModManager.ModsChanged -= OnModsChanged;

            ConfigManager.LeaderboardSection.ValueChanged -= OnLeaderboardSectionChanged;
            ConfigManager.DisplayFailedLocalScores.ValueChanged -= OnDisplayFailedLocalScoresChanged;


            OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;

            ScoreDatabaseCache.ScoreDeleted -= OnScoreDeleted;
            ScoreDatabaseCache.LocalMapScoresDeleted -= OnMapLocalScoresDeleted;

            if (Footer.Parent != null)
                Footer.Parent.SizeChanged -= OnFooterSizeChanged;

            base.Destroy();
        }
    }
}
