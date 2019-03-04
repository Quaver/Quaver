/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Form;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Shared.Graphics.Overlays.Chat.Components.Users
{
    public class OnlineUserFilters : Sprite
    {
        /// <summary>
        ///     Reference to the parent chat overlay
        /// </summary>
        public ChatOverlay Overlay { get; }

        /// <summary>
        ///     The text that says "Filters"
        /// </summary>
        private SpriteText TextFilters { get; set; }

        /// <summary>
        ///     The button to select the "All" Filter.
        /// </summary>
        private SelectableBorderedTextButton AllFilterButton { get; set; }

        /// <summary>
        ///     The button to select the "Friends" Filter.
        /// </summary>
        private SelectableBorderedTextButton FriendsFilterButton { get; set; }

        /// <summary>
        ///     The button to filter to countries
        /// </summary>
        private SelectableBorderedTextButton CountryFilterButton { get; set; }

        /// <summary>
        ///    Text that search "Search"
        /// </summary>
        private SpriteText TextSearch { get; set; }

        /// <summary>
        ///     The textbox to search for users.
        /// </summary>
        public Textbox SearchTextbox { get; private set; }

        /// <summary>
        ///     The divider line at the bottom.
        /// </summary>
        public Sprite DividerLine { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="overlay"></param>
        public OnlineUserFilters(ChatOverlay overlay)
        {
            Overlay = overlay;
            Parent = Overlay.OnlineUsersHeader;
            Y = Parent.Height + 1;
            X = -1;

            Size = new ScalableVector2(Parent.Width, 80);
            Tint = Color.Black;
            Alpha = 0.85f;

            ConfigManager.SelectedOnlineUserFilterType.ValueChanged += OnSelectedOnlineUserFilterTypeChanged;

            CreateTextFilter();
            CreateAllButton();
            CreateFriendsButton();
            CreateCountryButton();
            CreateTextSearch();
            CreateSearchTextbox();
            CreateDividerLine();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ConfigManager.SelectedOnlineUserFilterType.ValueChanged -= OnSelectedOnlineUserFilterTypeChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Creates the text that says "Filter:"
        /// </summary>
        private void CreateTextFilter()
        {
            TextFilters = new SpriteText(Fonts.Exo2SemiBold, "Filter:", 12)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(10, 10),
            };
        }

        /// <summary>
        ///     The button that says "All" in the filter
        /// </summary>
        private void CreateAllButton()
        {
            AllFilterButton = new SelectableBorderedTextButton("All", ColorHelper.HexToColor("#9d84ec"),
                ConfigManager.SelectedOnlineUserFilterType.Value == OnlineUserFilterType.All)
            {
                Parent = TextFilters,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(0, TextFilters.Height),
                X = TextFilters.Width + 5,
                Text =
                {
                    FontSize = 11,
                    Font = Fonts.SourceSansProSemiBold
                }
            };

            AllFilterButton.Width = AllFilterButton.Text.Width + 8;
            AllFilterButton.Height = AllFilterButton.Text.Height + 6;

            AllFilterButton.Clicked += (sender, args) =>
            {
                ConfigManager.SelectedOnlineUserFilterType.Value = OnlineUserFilterType.All;
                Overlay.OnlineUserList?.FilterUsers();
            };
        }

        /// <summary>
        ///     The button that says "Friends" in the filter.
        /// </summary>
        private void CreateFriendsButton()
        {
            FriendsFilterButton = new SelectableBorderedTextButton("Friends", ColorHelper.HexToColor("#9d84ec"),
                ConfigManager.SelectedOnlineUserFilterType.Value == OnlineUserFilterType.Friends)
            {
                Parent = TextFilters,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(0, AllFilterButton.Height),
                X = AllFilterButton.X + AllFilterButton.Width + 10,
                Text =
                {
                    FontSize = 11,
                    Font = Fonts.SourceSansProSemiBold
                }
            };

            FriendsFilterButton.Width = FriendsFilterButton.Text.Width + 8;
            FriendsFilterButton.Height = FriendsFilterButton.Text.Height + 6;

            FriendsFilterButton.Clicked += (sender, args) =>
            {
                ConfigManager.SelectedOnlineUserFilterType.Value = OnlineUserFilterType.Friends;
                Overlay.OnlineUserList?.FilterUsers();
            };
        }

        /// <summary>
        ///     Creates the button that says "Country" in the filter.
        /// </summary>
        private void CreateCountryButton()
        {
            CountryFilterButton = new SelectableBorderedTextButton("Country", ColorHelper.HexToColor("#9d84ec"),
                ConfigManager.SelectedOnlineUserFilterType.Value == OnlineUserFilterType.Country)
            {
                Parent = TextFilters,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(0, AllFilterButton.Height),
                X = FriendsFilterButton.X + FriendsFilterButton.Width + 10,
                Text =
                {
                    FontSize = 11,
                    Font = Fonts.SourceSansProSemiBold
                }
            };

            CountryFilterButton.Width = CountryFilterButton.Text.Width + 8;
            CountryFilterButton.Height = CountryFilterButton.Text.Height + 6;

            CountryFilterButton.Clicked += (sender, args) =>
            {
                ConfigManager.SelectedOnlineUserFilterType.Value = OnlineUserFilterType.Country;
                Overlay.OnlineUserList?.FilterUsers();
            };
        }

        /// <summary>
        ///     Called when the filter type has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedOnlineUserFilterTypeChanged(object sender,
            BindableValueChangedEventArgs<OnlineUserFilterType> e)
        {
            AllFilterButton.Selected = e.Value == OnlineUserFilterType.All;
            FriendsFilterButton.Selected = e.Value == OnlineUserFilterType.Friends;
            CountryFilterButton.Selected = e.Value == OnlineUserFilterType.Country;
        }

        /// <summary>
        ///     Creates the text that says "Search"
        /// </summary>
        private void CreateTextSearch()
        {
            TextSearch = new SpriteText(Fonts.Exo2SemiBold, "Search:", 12)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(TextFilters.X, TextFilters.Y + TextFilters.Height + 15),
            };
        }

        /// <summary>
        ///     Creates the textbox to search for users.
        /// </summary>
        private void CreateSearchTextbox()
        {
            SearchTextbox = new Textbox(new ScalableVector2(150, TextSearch.Height), Fonts.Exo2Regular, 8)
            {
                Parent = TextSearch,
                X = TextSearch.Width + 5,
                Alignment = Alignment.MidLeft,
                Tint = Color.Black,
                Alpha = 0.25f,
                Cursor = { Y = 5},
            };

            SearchTextbox.AddBorder(Color.White, 2);

            SearchTextbox.StoppedTypingActionCalltime = 100;
            SearchTextbox.OnStoppedTyping += text => Overlay.OnlineUserList?.FilterUsers(text);
        }

        private void CreateDividerLine() => DividerLine = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.BotLeft,
            Size = new ScalableVector2(Width, 2),
            Alpha = 0.35f
        };
    }
}
