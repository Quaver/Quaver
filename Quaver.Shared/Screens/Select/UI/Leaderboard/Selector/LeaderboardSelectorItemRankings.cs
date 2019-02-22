/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.Shared.Config;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Select.UI.Leaderboard.Selector
{
    public class LeaderboardSelectorItemRankings : LeaderboardSelectorItem
    {
        /// <summary>
        ///     The type of leaderboard this button is for.
        /// </summary>
        public LeaderboardType Type { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="text"></param>
        public LeaderboardSelectorItemRankings(LeaderboardType type, string text) :
            base(text, type == ConfigManager.LeaderboardSection.Value)
        {
            Type = type;
            ConfigManager.LeaderboardSection.ValueChanged += OnLeaderboardSectionChange;

            Clicked += (sender, args) =>
            {
                if (ConfigManager.LeaderboardSection.Value == Type)
                    return;

                ConfigManager.LeaderboardSection.Value = Type;
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ConfigManager.LeaderboardSection.ValueChanged -= OnLeaderboardSectionChange;
            base.Destroy();
        }

        /// <summary>
        ///     Called when the leaderboard section has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeaderboardSectionChange(object sender, BindableValueChangedEventArgs<LeaderboardType> e)
        {
            Selected = Type == e.Value;
            Alpha = Selected ? 0.25f: 0;
        }
    }
}
