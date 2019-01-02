/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Shared.Screens.Download.UI.Filter
{
    public class DownloadSearchFilters : Sprite
    {
        /// <summary>
        /// </summary>
        private DownloadScreenView View { get; }

        /// <summary>
        /// </summary>
        private SpriteText TextFilter { get; set; }

        /// <summary>
        ///     The filter to change the game mode.
        /// </summary>
        private RadioFilter GameModeFilter { get; set; }

        /// <summary>
        ///     The filter to change the ranked status
        /// </summary>
        private RadioFilter RankedStatusFilter { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        public DownloadSearchFilters(DownloadScreenView view)
        {
            View = view;
            Size = new ScalableVector2(400, 120);
            Tint = Color.Black;
            Alpha = 0.75f;

            CreateTextFilters();
            CreateGameModeFilter();
            CreateRankedStatusFilter();

            AddBorder(Color.White);
        }

        /// <summary>
        /// </summary>
        private void CreateTextFilters() => TextFilter = new SpriteText(Fonts.Exo2Bold, "Filter", 14)
        {
            Parent = this,
            Y = 15,
            X = 15,
            Tint = Colors.MainAccent
        };

        /// <summary>
        /// </summary>
        private void CreateGameModeFilter() => GameModeFilter = new RadioFilter(this, "Game Mode:", new List<string>
        {
            "4 Keys",
            "7 Keys"
        }, OnGameModeFilterChanged)
        {
            Parent = this,
            Y = TextFilter.Y + TextFilter.Height + 5,
            X = TextFilter.X
        };

        /// <summary>
        /// </summary>
        private void CreateRankedStatusFilter() => RankedStatusFilter = new RadioFilter(this, "Ranked Status:", new List<string>
        {
            "Ranked",
            "Unranked",
        }, OnRankedStatusFilterChanged)
        {
            Parent = this,
            Y = GameModeFilter.Y + GameModeFilter.Height + 15,
            X = TextFilter.X
        };

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="index"></param>
        private void OnGameModeFilterChanged(string val, int index)
        {
            var mode = GameMode.Keys4;

            switch (val)
            {
                case "4 Keys":
                    mode = GameMode.Keys4;
                    break;
                case "7 Keys":
                    mode = GameMode.Keys7;
                    break;
            }

            var screen = (DownloadScreen) View.Screen;
            screen.CurrentGameMode = mode;

            View.SearchBox.SearchForMapsets(View.SearchBox.SearchBox.RawText, screen.CurrentGameMode, screen.CurrentRankedStatus, false);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="val"></param>
        /// <param name="index"></param>
        private void OnRankedStatusFilterChanged(string val, int index)
        {
            var status = RankedStatus.Ranked;

            switch (val)
            {
                case "Ranked":
                    status = RankedStatus.Ranked;
                    break;
                case "Unranked":
                    status = RankedStatus.Unranked;
                    break;
            }

            var screen = (DownloadScreen) View.Screen;
            screen.CurrentRankedStatus = status;

            View.SearchBox.SearchForMapsets(View.SearchBox.SearchBox.RawText, screen.CurrentGameMode, screen.CurrentRankedStatus, false);
        }
    }
}