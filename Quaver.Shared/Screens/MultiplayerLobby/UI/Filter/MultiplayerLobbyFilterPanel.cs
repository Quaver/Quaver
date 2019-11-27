using System;
using System.Collections.Generic;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Filter
{
    public class MultiplayerLobbyFilterPanel : Sprite
    {
        /// <summary>
        ///     Query used for the search box
        /// </summary>
        private Bindable<string> SearchQuery { get; } = new Bindable<string>("") { Value = ""};

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
        public MultiplayerLobbyFilterPanel()
        {
            Size = new ScalableVector2(WindowManager.Width, 88);
            Tint = ColorHelper.HexToColor("#242424");

            CreateBanner();
            CreateRulesetDropdown();
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
                X = 25
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMatchesFound()
        {
            GamesFound = new MultiplayerLobbyGamesFound
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

                const int padding = 25;
                var spacing = 28;

                if (i == 0)
                    item.X = -padding;
                else
                    item.X = RightItems[i - 1].X - RightItems[i - 1].Width - spacing;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRulesetChanged(object sender, BindableValueChangedEventArgs<MultiplayerLobbyRuleset> e)
        {
            Console.WriteLine($"Ruleset changed to: {e.Value}");
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameModeChanged(object sender, BindableValueChangedEventArgs<MultiplayerLobbyGameMode> e)
        {
            Console.WriteLine($"Game Mode Changed to: {e.Value}");
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapStatusChanged(object sender, BindableValueChangedEventArgs<MultiplayerLobbyMapStatus> e)
        {
            Console.WriteLine($"Map status changed to: {e.Value}");
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRoomVisibilityChanged(object sender, BindableValueChangedEventArgs<MultiplayerLobbyRoomVisibility> e)
        {
            Console.WriteLine($"Room Visibility changed to: {e.Value}");
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSearchQueryChanged(object sender, BindableValueChangedEventArgs<string> e)
        {
            Console.WriteLine($"Search query changed to: {e.Value}");
        }
    }
}