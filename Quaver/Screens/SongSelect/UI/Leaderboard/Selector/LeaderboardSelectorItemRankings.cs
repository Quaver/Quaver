using System;
using Quaver.Config;
using Wobble.Bindables;

namespace Quaver.Screens.SongSelect.UI.Leaderboard.Selector
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
            base(text, type == ConfigManager.LeaderboardSection.Value, null)
        {
            Type = type;
            ConfigManager.LeaderboardSection.ValueChanged += OnLeaderboardSectionChange;
            Clicked += (sender, args) => { ConfigManager.LeaderboardSection.Value = Type; };
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