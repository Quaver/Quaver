using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Rating;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Selection.UI.Leaderboard.Components;
using Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings;
using Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings.Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings;
using Quaver.Shared.Skinning;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Logging;
using Wobble.Managers;
using Wobble.Scheduling;
using Logger = Wobble.Logging.Logger;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard
{
    public class LeaderboardContainer : Sprite, ILoadable
    {
        /// <summary>
        ///     Displays "LEADERBOARD"
        /// </summary>
        private SpriteTextPlus Header { get; set; }

        /// <summary>
        ///     Allows the user to select between different leaderboard types
        /// </summary>
        private LeaderboardTypeDropdown TypeDropdown { get; set; }

        /// <summary>
        ///     The background for <see cref="ScoresContainer"/>
        /// </summary>
        public Sprite ScoresContainerBackground { get; private set; }

        /// <summary>
        ///     Displays the scores of the leaderboard
        /// </summary>
        public LeaderboardScoresContainer ScoresContainer { get; set; }

        /// <summary>
        ///     A header above the user's personal best score
        /// </summary>
        private SpriteTextPlus PersonalBestHeader { get; set; }

        /// <summary>
        ///     Displays the user's personal best score for the leaderboard section
        /// </summary>
        private LeaderboardPersonalBestScore PersonalBestScore { get; set; }

        /// <summary>
        ///     Task that's ran when fetching for leaderboard scores
        /// </summary>
        public TaskHandler<Map, FetchedScoreStore> FetchScoreTask { get; }

        /// <summary>
        ///     Trophy symbol that shows what the user's PB rank is
        /// </summary>
        private Sprite PersonalBestTrophy { get; set; }

        /// <summary>
        ///     Displays the user's personal best rank
        /// </summary>
        private SpriteTextPlus PersonalBestRank { get; set; }

        /// <summary>
        /// </summary>
        public LeaderboardContainer()
        {
            Size = new ScalableVector2(564, 838);
            Alpha = 0f;
            AutoScaleHeight = true;

            FetchScoreTask = new TaskHandler<Map, FetchedScoreStore>(FetchScores);
            FetchScoreTask.OnCompleted += OnFetchedScores;

            CreateHeaderText();
            CreateRankingDropdown();
            CreateScoresContainer();
            CreatePersonalBestHeader();
            CreatePersonalBestScore();
            CreateTrophy();
            CreatePersonalBestRank();

            ListHelper.Swap(Children, Children.IndexOf(TypeDropdown), Children.IndexOf(ScoresContainerBackground));

            MapManager.Selected.ValueChanged += OnMapChanged;

            if (ConfigManager.LeaderboardSection != null)
                ConfigManager.LeaderboardSection.ValueChanged += OnLeaderboardSectionChanged;

            if (ConfigManager.DisplayFailedLocalScores != null)
                ConfigManager.DisplayFailedLocalScores.ValueChanged += OnDisplayFailedLocalScoresChanged;

            ModManager.ModsChanged += OnModsChanged;
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
            ScoreDatabaseCache.ScoreDeleted += OnScoreDeleted;
            ScoreDatabaseCache.LocalMapScoresDeleted += OnMapLocalScoresDeleted;

            FetchScores();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            FetchScoreTask?.Dispose();

            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            if (ConfigManager.LeaderboardSection != null)
            {
                // ReSharper disable once DelegateSubtraction
                ConfigManager.LeaderboardSection.ValueChanged -= OnLeaderboardSectionChanged;
            }

            if (ConfigManager.DisplayFailedLocalScores != null)
            {
                // ReSharper disable once DelegateSubtraction
                ConfigManager.DisplayFailedLocalScores.ValueChanged -= OnDisplayFailedLocalScoresChanged;
            }

            ModManager.ModsChanged -= OnModsChanged;

            // ReSharper disable once DelegateSubtraction
            OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;
            ScoreDatabaseCache.ScoreDeleted -= OnScoreDeleted;
            ScoreDatabaseCache.LocalMapScoresDeleted -= OnMapLocalScoresDeleted;

            base.Destroy();
        }

        /// <summary>
        ///    Creates <see cref="Header"/>
        /// </summary>
        private void CreateHeaderText()
        {
            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoHeavy), "LEADERBOARD", 30)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Tint = SkinManager.Skin?.SongSelect?.LeaderboardTitleColor ?? Color.White
            };
        }

        /// <summary>
        ///     Creates <see cref="TypeDropdown"/>
        /// </summary>
        private void CreateRankingDropdown()
        {
            TypeDropdown = new LeaderboardTypeDropdown
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Y = Header.Y / 2f
            };
        }

        /// <summary>
        ///     Creates <see cref="ScoresContainer"/>
        /// </summary>
        private void CreateScoresContainer()
        {
            ScoresContainerBackground = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Y = Header.Y + Header.Height + 8,
                Size = new ScalableVector2(Width,664),
                Image = SkinManager.Skin?.SongSelect?.LeaderboardPanel ?? UserInterface.LeaderboardScoresPanel,
                AutoScaleHeight = true
            };

            ScoresContainer = new LeaderboardScoresContainer(this)
            {
                Parent = ScoresContainerBackground,
                Alignment = Alignment.MidCenter,
            };
        }

        /// <summary>
        ///     Creates <see cref="PersonalBestHeader"/>
        /// </summary>
        private void CreatePersonalBestHeader()
        {
            PersonalBestHeader = new SpriteTextPlus(Header.Font, "PERSONAL BEST", Header.FontSize)
            {
                Parent = this,
                Y = ScoresContainerBackground.Y + ScoresContainerBackground.Height + 28,
                Tint = SkinManager.Skin?.SongSelect?.PersonalBestTitleColor ?? Color.White
            };
        }

        /// <summary>
        /// </summary>
        private void CreateTrophy()
        {
            PersonalBestTrophy = new Sprite
            {
                Parent = this,
                Y = PersonalBestHeader.Y + 2,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(25, 25),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_trophy),
                Tint = SkinManager.Skin?.SongSelect?.PersonalBestTrophyColor ?? ColorHelper.HexToColor("#E9B736"),
                Alpha = 0
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePersonalBestRank()
        {
            PersonalBestRank = new SpriteTextPlus(Header.Font, "#50", Header.FontSize - 2)
            {
                Parent = this,
                Y = PersonalBestTrophy.Y - 3,
                Alignment = Alignment.TopRight,
                Alpha = 0,
                Tint = SkinManager.Skin?.SongSelect?.PersonalBestRankColor ?? Color.White
            };
        }

        /// <summary>
        ///     Creates <see cref="PersonalBestScore"/>
        /// </summary>
        private void CreatePersonalBestScore()
        {
            PersonalBestScore = new LeaderboardPersonalBestScore(this)
            {
                Parent = this,
                Y = PersonalBestHeader.Y + PersonalBestHeader.Height + 6
            };
        }

        /// <summary>
        ///     Initiates a task to fetch scores for the selected map
        /// </summary>
        public void FetchScores()
        {
            if (MapManager.Selected.Value != null)
                MapManager.Selected.Value.NeedsOnlineUpdate = false;

            StopLoading();
            FetchScoreTask.Run(MapManager.Selected.Value, 400);
            StartLoading();
        }

        /// <summary>
        ///     Fetches scores for the passed in map
        /// </summary>
        /// <returns></returns>
        private FetchedScoreStore FetchScores(Map map, CancellationToken token)
        {
            if (map == null)
                return new FetchedScoreStore(new List<Score>());

            FetchedScoreStore scores;

            switch (ConfigManager.LeaderboardSection.Value)
            {
                case LeaderboardType.Local:
                    scores = new ScoreFetcherLocal().Fetch(map);
                    break;
                case LeaderboardType.Global:
                    scores = new ScoreFetcherGlobal().Fetch(map);
                    break;
                case LeaderboardType.Mods:
                    scores = new ScoreFetcherMods().Fetch(map);
                    break;
                case LeaderboardType.Country:
                    scores = new ScoreFetcherCountry().Fetch(map);
                    break;
                case LeaderboardType.Rate:
                    scores = new ScoreFetcherRate().Fetch(map);
                    break;
                case LeaderboardType.Friends:
                    scores = new ScoreFetcherFriends().Fetch(map);
                    break;
                case LeaderboardType.All:
                    scores = new ScoreFetcherAll().Fetch(map);
                    break;
                case LeaderboardType.Clan:
                    scores = new ScoreFetcherClan().Fetch(map);
                    break;
                default:
                    scores = new FetchedScoreStore();
                    break;
            }

            // Set scores to use during gameplay
            if (OnlineManager.CurrentGame != null)
                return scores;

            MapManager.Selected.Value.Scores.Value = scores.Scores;
            ScoresHelper.SetRatingProcessors(MapManager.Selected.Value.Scores.Value);

            return scores;
        }

        /// <summary>
        ///     Called when having successfully fetched scores
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFetchedScores(object sender, TaskCompleteEventArgs<Map, FetchedScoreStore> e)
        {
            Logger.Debug($"Fetched {e.Result.Scores?.Count} {ConfigManager.LeaderboardSection?.Value} scores for map: {e.Input} | " +
                         $"Has PB: {e.Result.PersonalBest != null}", LogType.Runtime);

            StopLoading();

            Children.ForEach(x =>
            {
                if (x is IFetchedScoreHandler handler)
                    handler.HandleFetchedScores(e.Input, e.Result);
            });

            ScoresContainer.HandleFetchedScores(e.Input, e.Result);

            PersonalBestTrophy.ClearAnimations();
            PersonalBestRank.ClearAnimations();

            // Handle personal best rank
            if (ConfigManager.LeaderboardSection != null && ConfigManager.LeaderboardSection.Value != LeaderboardType.Local)
            {
                var rank = e.Result.Scores?.FindIndex(x => x.Name == e.Result.PersonalBest?.Name);

                if (rank == -1)
                    return;

                PersonalBestRank.Text = $"#{rank + 1} of Top {e.Result.Scores?.Count}";
                PersonalBestTrophy.X = -PersonalBestRank.Width - 10;

                const int animTime = 250;
                PersonalBestTrophy.FadeTo(1, Easing.Linear, animTime);
                PersonalBestRank.FadeTo(1, Easing.Linear, animTime);
            }
        }

        /// <summary>
        ///     Called when the selected map has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            ScheduleUpdate(() =>
            {
                e.OldValue?.ClearScores();
                e.Value?.ClearScores();
            });

            FetchScores();
        }

        /// <summary>
        ///     Called when the user changes the selected leaderboard section
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeaderboardSectionChanged(object sender, BindableValueChangedEventArgs<LeaderboardType> e) => FetchScores();

        /// <summary>
        ///     Called when the user changes the option to display failed local scores
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisplayFailedLocalScoresChanged(object sender, BindableValueChangedEventArgs<bool> e)
        {
            if (ConfigManager.LeaderboardSection == null || ConfigManager.LeaderboardSection.Value != LeaderboardType.Local)
                return;

            FetchScores();
        }

        /// <summary>
        ///     Called when the user selects new mods while their leaderboard section is selected mods
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e)
        {
            if (ConfigManager.LeaderboardSection == null ||
                ConfigManager.LeaderboardSection.Value != LeaderboardType.Mods && ConfigManager.LeaderboardSection.Value != LeaderboardType.Rate)
                return;

            FetchScores();
        }

        /// <summary>
        /// </summary>
        public void StartLoading()
        {
            foreach (var x in new List<Drawable>(Children))
            {
                if (x is ILoadable loadable)
                    loadable.StartLoading();

                ScoresContainer.StartLoading();

                const int animTime = 20;

                PersonalBestTrophy.ClearAnimations();
                PersonalBestTrophy.FadeTo(0, Easing.Linear, animTime);

                PersonalBestRank.ClearAnimations();
                PersonalBestRank.FadeTo(0, Easing.Linear, animTime);
            }
        }

        /// <summary>
        /// </summary>
        public void StopLoading()
        {
            foreach (var x in new List<Drawable>(Children))
            {
                if (x is ILoadable loadable)
                    loadable.StopLoading();

                ScoresContainer.StopLoading();
            }
        }

        /// <summary>
        ///     Whenever the user connects to the server in song select, it will automatically
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (e.Value != ConnectionStatus.Connected || ConfigManager.LeaderboardSection.Value == LeaderboardType.Local)
                return;

            ScheduleUpdate(FetchScores);
        }

        /// <summary>
        ///     Called when the user deletes a local score.
        ///     Refreshes the leaderboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScoreDeleted(object sender, ScoreDeletedEventArgs e)
        {
            if (ConfigManager.LeaderboardSection == null || ConfigManager.LeaderboardSection.Value != LeaderboardType.Local)
                return;

            FetchScores();
        }

        /// <summary>
         ///     Called when the user deletes a map's scores
         /// </summary>
         /// <param name="sender"></param>
         /// <param name="e"></param>
        private void OnMapLocalScoresDeleted(object sender, LocalScoresDeletedEventArgs e)
        {
            if (ConfigManager.LeaderboardSection == null || ConfigManager.LeaderboardSection.Value != LeaderboardType.Local)
                return;

            FetchScores();
        }
    }
}
