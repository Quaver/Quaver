using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Quaver.API.Enums;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Games;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Scheduling;
using Wobble.Window;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Filter
{
    public class MultiplayerLobbyFilterPanel : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<List<MultiplayerGame>> VisibleGames { get; }

        /// <summary>
        ///     Query used for the search box
        /// </summary>
        public Bindable<string> SearchQuery { get; } = new Bindable<string>("") { Value = ""};

        /// <summary>
        /// </summary>
        private MultiplayerLobbyFilterPanelBanner Banner { get; set; }

        /// <summary>
        ///     Items that are aligned from right to left
        /// </summary>
        private List<Drawable> RightItems { get; } = new List<Drawable>();

        /// <summary>
        /// </summary>
        private MultiplayerLobbyRulesetDropdown Ruleset { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerLobbyModeDropdown Mode { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerLobbyMapDropdown Map { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerLobbyVisibilityDropdown Visibility { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerLobbySearchBox SearchBox { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerLobbyGamesFound GamesFound { get; set; }

        /// <summary>
        /// </summary>
        public TaskHandler<int, int> FilterTask { get; }

        /// <summary>
        /// </summary>
        public MultiplayerLobbyFilterPanel(Bindable<List<MultiplayerGame>> visibleGames)
        {
            VisibleGames = visibleGames;
            Size = new ScalableVector2(WindowManager.Width, 88);
            Tint = ColorHelper.HexToColor("#242424");

            FilterTask = new TaskHandler<int, int>(StartFilterTask);
            CreateBanner();
            //CreateRulesetDropdown();
            CreateModeDropdown();
            CreateMapDropdown();
            CreateVisibilityDropdown();
            CreateSearchBox();
            CreateMatchesFound();

            if (ConfigManager.MultiplayerLobbyRulesetType != null)
            {
                ConfigManager.MultiplayerLobbyRulesetType.ValueChanged += OnRulesetChanged;
                ConfigManager.MultiplayerLobbyGameModeType.ValueChanged += OnGameModeChanged;
                ConfigManager.MultiplayerLobbyMapStatusType.ValueChanged += OnMapStatusChanged;
                ConfigManager.MultiplayerLobbyVisibilityType.ValueChanged += OnRoomVisibilityChanged;
            }

            SearchQuery.ValueChanged += OnSearchQueryChanged;

            AlignRightItems();
            FilterGames();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            if (ConfigManager.MultiplayerLobbyRulesetType != null)
            {
                ConfigManager.MultiplayerLobbyRulesetType.ValueChanged -= OnRulesetChanged;
                ConfigManager.MultiplayerLobbyGameModeType.ValueChanged -= OnGameModeChanged;
                ConfigManager.MultiplayerLobbyMapStatusType.ValueChanged -= OnMapStatusChanged;
                ConfigManager.MultiplayerLobbyVisibilityType.ValueChanged -= OnRoomVisibilityChanged;
            }

            SearchQuery.ValueChanged -= OnSearchQueryChanged;
            SearchQuery.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBanner()
        {
            Banner = new MultiplayerLobbyFilterPanelBanner(new ScalableVector2(960, Height))
            {
                Parent = this
            };
        }

        /// <summary>
        /// </summary>
        private void CreateRulesetDropdown()
        {
            Ruleset = new MultiplayerLobbyRulesetDropdown {Parent = this};
            RightItems.Add(Ruleset);
        }

        /// <summary>
        /// </summary>
        private void CreateModeDropdown()
        {
            Mode = new MultiplayerLobbyModeDropdown {Parent = this};
            RightItems.Add(Mode);
        }

        /// <summary>
        /// </summary>
        private void CreateMapDropdown()
        {
            Map = new MultiplayerLobbyMapDropdown {Parent = this};
            RightItems.Add(Map);
        }

        /// <summary>
        /// </summary>
        private void CreateVisibilityDropdown()
        {
            Visibility = new MultiplayerLobbyVisibilityDropdown {Parent = this};
            RightItems.Add(Visibility);
        }

        /// <summary>
        /// </summary>
        private void CreateSearchBox()
        {
            SearchBox = new MultiplayerLobbySearchBox(SearchQuery)
            {
                Parent = this,
                Alignment  = Alignment.MidLeft,
                X = 20
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMatchesFound()
        {
            GamesFound = new MultiplayerLobbyGamesFound(VisibleGames)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = SearchBox.X + SearchBox.Width + 25
            };
        }

        /// <summary>
        ///     Aligns the items from right to left
        /// </summary>
        private void AlignRightItems()
        {
            for (var i = 0; i < RightItems.Count; i++)
            {
                var item = RightItems[i];

                item.Parent = this;

                item.Alignment = Alignment.MidRight;

                const int padding = 20;
                var spacing = 28;

                if (i == 0)
                    item.X = -padding;
                else
                    item.X = RightItems[i - 1].X - RightItems[i - 1].Width - spacing;
            }
        }

        /// <summary>
        /// </summary>
        public void StartFilterTask() => FilterTask.Run(0);

        /// <summary>
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private int StartFilterTask(int arg1, CancellationToken token)
        {
            FilterGames();
            return 0;
        }

        /// <summary>
        /// </summary>
        public void FilterGames()
        {
            lock (VisibleGames.Value)
            {
                var games = OnlineManager.MultiplayerGames?.Values?.ToList() ?? new List<MultiplayerGame>();
                games = games?.FindAll(x => GameMeetsFilterRequirements(x, SearchQuery.Value));

                VisibleGames.Value = games;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static bool GameMeetsFilterRequirements(MultiplayerGame game, string query)
        {
            return GameMeetsSearchRequirement(game, query) && GameMeetsRulesetFilter(game)
                 && GameMeetsModeFilter(game) && GameMeetsMapStatusFilter(game)
                 && GameMeetsRoomVisibilityRequirement(game);
        }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static bool GameMeetsSearchRequirement(MultiplayerGame game, string query)
        {
            return string.IsNullOrEmpty(query) || game.Name.ToLower().Contains(query.ToLower())
                                               || game.Map.ToLower().Contains(query.ToLower());
        }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool GameMeetsRulesetFilter(MultiplayerGame game)
        {
            if (ConfigManager.MultiplayerLobbyRulesetType == null)
                return true;

            switch (ConfigManager.MultiplayerLobbyRulesetType.Value)
            {
                case MultiplayerLobbyRuleset.All:
                    return true;
                case MultiplayerLobbyRuleset.FreeForAll:
                    return game.Ruleset == MultiplayerGameRuleset.Free_For_All;
                case MultiplayerLobbyRuleset.Team:
                    return game.Ruleset == MultiplayerGameRuleset.Team;
                case MultiplayerLobbyRuleset.BattleRoyale:
                    return game.Ruleset == MultiplayerGameRuleset.Battle_Royale;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool GameMeetsModeFilter(MultiplayerGame game)
        {
            if (ConfigManager.MultiplayerLobbyGameModeType == null)
                return true;

            switch (ConfigManager.MultiplayerLobbyGameModeType.Value)
            {
                case MultiplayerLobbyGameMode.All:
                    return true;
                case MultiplayerLobbyGameMode.Keys4:
                    return (GameMode) game.GameMode == GameMode.Keys4;
                case MultiplayerLobbyGameMode.Keys7:
                    return (GameMode) game.GameMode == GameMode.Keys7;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool GameMeetsMapStatusFilter(MultiplayerGame game)
        {
            if (ConfigManager.MultiplayerLobbyMapStatusType == null)
                return true;

            switch (ConfigManager.MultiplayerLobbyMapStatusType.Value)
            {
                case MultiplayerLobbyMapStatus.All:
                    return true;
                case MultiplayerLobbyMapStatus.Imported:
                    return MapManager.FindMapFromOnlineId(game.MapId) != null;
                case MultiplayerLobbyMapStatus.Uploaded:
                    return game.MapId != -1;
                case MultiplayerLobbyMapStatus.Unsubmitted:
                    return game.MapId == -1;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool GameMeetsRoomVisibilityRequirement(MultiplayerGame game)
        {
            if (ConfigManager.MultiplayerLobbyVisibilityType == null)
                return true;

            switch (ConfigManager.MultiplayerLobbyVisibilityType.Value)
            {
                case MultiplayerLobbyRoomVisibility.All:
                    return true;
                case MultiplayerLobbyRoomVisibility.Open:
                    return !game.HasPassword;
                case MultiplayerLobbyRoomVisibility.Full:
                    return game.PlayerIds.Count >= game.MaxPlayers;
                case MultiplayerLobbyRoomVisibility.Password:
                    return game.HasPassword;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRulesetChanged(object sender, BindableValueChangedEventArgs<MultiplayerLobbyRuleset> e)
            => StartFilterTask();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameModeChanged(object sender, BindableValueChangedEventArgs<MultiplayerLobbyGameMode> e)
            => StartFilterTask();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapStatusChanged(object sender, BindableValueChangedEventArgs<MultiplayerLobbyMapStatus> e)
            => StartFilterTask();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRoomVisibilityChanged(object sender, BindableValueChangedEventArgs<MultiplayerLobbyRoomVisibility> e)
            => StartFilterTask();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSearchQueryChanged(object sender, BindableValueChangedEventArgs<string> e)
            => StartFilterTask();
    }
}