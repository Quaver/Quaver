/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Download.UI.Drawable;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Download.UI
{
    public class DownloadSearchBox : Sprite
    {
        /// <summary>
        /// </summary>
        private DownloadScreenView View { get; }

        /// <summary>
        /// </summary>
        private SpriteText TextSearch { get; set; }

        /// <summary>
        /// </summary>
        public Textbox SearchBox { get; private set; }

        /// <summary>
        /// </summary>
        private SpriteText TextMapsetsFound{ get; set; }

        /// <summary>
        ///     Dictates if we're currently in the middle of searching for sets.
        /// </summary>
        public Bindable<bool> IsSearching { get; }

        /// <summary>
        ///     To cancel tasks.
        /// </summary>
        private CancellationTokenSource Source { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        public DownloadSearchBox(DownloadScreenView view)
        {
            View = view;
            Size = new ScalableVector2(400, 120);
            Tint = Color.Black;
            Alpha = 0.75f;
            IsSearching = new Bindable<bool>(false);

            CreateTextSearch();
            CreateSearchBox();
            CreateTextMapsetsFound();

            AddBorder(Color.White);

            Source?.Cancel();
            Source?.Dispose();
            Source = new CancellationTokenSource();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            SearchBox.AlwaysFocused = !IsSearching.Value && DialogManager.Dialogs.Count == 0;
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateTextSearch() => TextSearch = new SpriteText(Fonts.Exo2Bold, "Search", 14)
        {
            Parent = this,
            Y = 15,
            X = 15,
            Tint = Colors.MainAccent
        };

        /// <summary>
        /// </summary>
        private void CreateSearchBox()
        {
            SearchBox = new Textbox(new ScalableVector2(Width - 30, 30), Fonts.Exo2SemiBold, 14, "", "", null, OnStoppedTyping)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                X = TextSearch.X,
                Y = TextSearch.Y + TextSearch.Height + 5,
                Tint = Colors.DarkGray,
            };

            SearchBox.AddBorder(Colors.MainAccent, 2);

            var magnifyingGlass = new Sprite
            {
                Parent = SearchBox,
                Alignment = Alignment.MidRight,
                X = -10,
                Size = new ScalableVector2(15, 15),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_magnifying_glass)
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="query"></param>
        private void OnStoppedTyping(string query)
        {
            var screen = (DownloadScreen) View.Screen;
            SearchForMapsets(query, screen.CurrentGameMode, screen.CurrentRankedStatus, false);
        }

        /// <summary>
        /// </summary>
        private void CreateTextMapsetsFound() => TextMapsetsFound = new SpriteText(Fonts.SourceSansProBold, "Searching...", 13)
        {
            Parent = this,
            X = TextSearch.X,
            Y = SearchBox.Y + SearchBox.Height + 8,
        };

        /// <summary>
        /// </summary>
        /// <param name="mapsets"></param>
        private void UpdateMapsetsFoundText(JObject mapsets) =>
            TextMapsetsFound.Text = $"{mapsets["mapsets"].Count()}{(mapsets["mapsets"].Count() >= 20 ? "+" : "")} Results Found";

        /// <summary>
        /// </summary>
        public void SearchForMapsets(string query, GameMode mode, RankedStatus status, bool addToBottom)
        {
            Logger.Important($"Searching for mapsets w/ query: {mode} | {status} | {query}...", LogType.Network);

            // Make sure all buttons aren't clickable and that we're searching.
            Button.IsGloballyClickable = false;
            IsSearching.Value = true;

            TextMapsetsFound.Text = "Searching...";

            var screen = (DownloadScreen) View.Screen;

            // Destroy all existing mapsets & reset the current page.
            if (!addToBottom)
            {
                View.ScrollContainer.ClearMapsets();
                screen.CurrentPage = 0;
            }
            // Increment the current page since we're adding more sets.
            else
                screen.CurrentPage++;

            // Cancel the already existing token.
            Source?.Cancel();
            Source?.Dispose();
            Source = new CancellationTokenSource();

            // Start searching for new sets.
            Task.Run(async () =>
            {
                try
                {
                    Source.Token.ThrowIfCancellationRequested();
                    await Task.Delay(300, Source.Token);
                    Source.Token.ThrowIfCancellationRequested();

                    var mapsets = OnlineManager.Client?.SearchMapsets(mode, status, query, screen.CurrentPage);

                    if (mapsets == null)
                    {
                        IsSearching.Value = false;
                        screen.LastSearchedCount = 0;
                        return;
                    }

                    screen.LastSearchedCount = mapsets["mapsets"].Count();

                    View.ScrollContainer.AddMapsets(mapsets["mapsets"]
                        .Select(mapset => new DownloadableMapset(View.ScrollContainer, mapset)).ToList(), addToBottom);

                    UpdateMapsetsFoundText(mapsets);
                    Logger.Important($"Found {mapsets["mapsets"].Count()} mapsets w/ status code: {mapsets["status"]}", LogType.Network);
                }
                catch (Exception e)
                {
                    // ignored.
                    Logger.Error(e, LogType.Runtime);
                    screen.LastSearchedCount = 0;
                }

                IsSearching.Value = false;
                Button.IsGloballyClickable = true;
            });
        }
    }
}