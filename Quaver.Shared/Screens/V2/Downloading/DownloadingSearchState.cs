using System;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.V2.Downloading
{
    internal enum DownloadSearchTab
    {
        Mapsets,
        Playlists
    }

    internal enum DownloadSearchRankedStatus
    {
        All,
        Unranked,
        Ranked,
        ClanRanked
    }

    /// <summary>
    ///     Local-only state for the first V2 Download search-header slice.
    /// </summary>
    internal sealed class DownloadingSearchState : IDisposable
    {
        public Bindable<DownloadSearchTab> ActiveTab { get; } =
            new Bindable<DownloadSearchTab>(DownloadSearchTab.Mapsets);

        public Bindable<bool> MapsetsExpanded { get; } = new Bindable<bool>(false);

        public Bindable<string> MapsetQuery { get; } = new Bindable<string>(string.Empty);

        public Bindable<string> PlaylistQuery { get; } = new Bindable<string>(string.Empty);

        public Bindable<bool> ShowOwnedMapsets { get; } = new Bindable<bool>(true);

        public Bindable<bool> ShowOwnedPlaylists { get; } = new Bindable<bool>(false);

        /// <summary>
        ///     Zero represents all keymodes; positive values use the underlying GameMode value.
        /// </summary>
        public Bindable<int> Keymode { get; } = new Bindable<int>(0);

        public Bindable<DownloadSearchRankedStatus> RankedStatus { get; } =
            new Bindable<DownloadSearchRankedStatus>(DownloadSearchRankedStatus.Ranked);

        public BindableFloat MinimumDifficulty { get; } = new BindableFloat(0, 0, 99.99f);

        public BindableFloat MaximumDifficulty { get; } = new BindableFloat(99.99f, 0, 99.99f);

        public BindableFloat MinimumLongNotePercentage { get; } = new BindableFloat(0, 0, 100);

        public BindableFloat MaximumLongNotePercentage { get; } = new BindableFloat(100, 0, 100);

        public BindableFloat MinimumNotesPerSecond { get; } = new BindableFloat(0, 0, 9999);

        public BindableFloat MaximumNotesPerSecond { get; } = new BindableFloat(9999, 0, 9999);

        public BindableFloat MinimumBpm { get; } = new BindableFloat(0, 0, 9999);

        public BindableFloat MaximumBpm { get; } = new BindableFloat(9999, 0, 9999);

        public void Dispose()
        {
            ActiveTab.Dispose();
            MapsetsExpanded.Dispose();
            MapsetQuery.Dispose();
            PlaylistQuery.Dispose();
            ShowOwnedMapsets.Dispose();
            ShowOwnedPlaylists.Dispose();
            Keymode.Dispose();
            RankedStatus.Dispose();
            MinimumDifficulty.Dispose();
            MaximumDifficulty.Dispose();
            MinimumLongNotePercentage.Dispose();
            MaximumLongNotePercentage.Dispose();
            MinimumNotesPerSecond.Dispose();
            MaximumNotesPerSecond.Dispose();
            MinimumBpm.Dispose();
            MaximumBpm.Dispose();
        }
    }
}
