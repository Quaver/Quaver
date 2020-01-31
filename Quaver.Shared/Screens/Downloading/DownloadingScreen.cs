using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Helpers;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.MapsetSearch;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Downloading.UI.Search;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.Music;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Theater;
using Wobble.Audio;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Scheduling;

namespace Quaver.Shared.Screens.Downloading
{
    public sealed class DownloadingScreen : QuaverScreen
    {
        /// <summary>
        /// </summary>
        private QuaverScreenType PreviousScreen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Download;

        /// <summary>
        ///     The currently displayed mapsets
        /// </summary>
        public BindableList<DownloadableMapset> Mapsets { get; } = new BindableList<DownloadableMapset>(new List<DownloadableMapset>())
        {
            Value = new List<DownloadableMapset>()
        };

        /// <summary>
        ///     The currently selected mapset
        /// </summary>
        public Bindable<DownloadableMapset> SelectedMapset { get; } = new Bindable<DownloadableMapset>(null);

        /// <summary>
        ///     The user's current search query
        /// </summary>
        public Bindable<string> CurrentSearchQuery { get; } = new Bindable<string>("") {Value = ""};

        /// <summary>
        ///     The currently filtered game mode
        /// </summary>
        public Bindable<DownloadFilterMode> FilterGameMode { get; } = new Bindable<DownloadFilterMode>(DownloadFilterMode.All)
        {
            Value = DownloadFilterMode.All
        };

        /// <summary>
        ///     The currently filtered ranked status
        /// </summary>
        public Bindable<DownloadFilterRankedStatus> FilterRankedStatus { get; } = new Bindable<DownloadFilterRankedStatus>(DownloadFilterRankedStatus.Ranked)
        {
            Value = DownloadFilterRankedStatus.Ranked
        };

        /// <summary>
        /// </summary>
        public BindableFloat MinDifficulty { get; } = new BindableFloat(0, 0, 9999) {Value = 0};

        /// <summary>
        /// </summary>
        public BindableFloat MaxDifficulty { get; } = new BindableFloat(0, 0, float.MaxValue) {Value = 9999};

        /// <summary>
        /// </summary>
        public BindableFloat MinBpm { get; } = new BindableFloat(0, 0, float.MaxValue) {Value = 0};

        /// <summary>
        /// </summary>
        public BindableFloat MaxBpm { get; } = new BindableFloat(0, 0, float.MaxValue) {Value = 9999};

        /// <summary>
        /// </summary>
        public BindableInt MinLength { get; } = new BindableInt(0, 0, int.MaxValue) {Value = 0};

        /// <summary>
        /// </summary>
        public BindableInt MaxLength { get; } = new BindableInt(0, 0, int.MaxValue) {Value = int.MaxValue};

        /// <summary>
        /// </summary>
        public BindableInt MinLongNotePercent { get; } = new BindableInt(0, 0, 100) {Value = 0};

        /// <summary>
        /// </summary>
        public BindableInt MaxLongNotePercent { get; } = new BindableInt(0, 0, 100) {Value = 100};

        /// <summary>
        /// </summary>
        public BindableInt MinPlayCount { get; } = new BindableInt(0, 0, int.MaxValue) {Value = 0};

        /// <summary>
        /// </summary>
        public BindableInt MaxPlayCount { get; } = new BindableInt(0, 0, int.MaxValue) {Value = int.MaxValue};

        /// <summary>
        /// </summary>
        public Bindable<string> MinUploadDate { get; } = new Bindable<string>("") { Value = "01-01-1970"};

        /// <summary>
        /// </summary>
        public Bindable<string> MaxUploadDate { get; } = new Bindable<string>("")  { Value = "12-31-9999"};

        /// <summary>
        /// </summary>
        public Bindable<string> MinLastUpdateDate { get; } = new Bindable<string>("") { Value = "01-01-1970"};

        /// <summary>
        /// </summary>
        public Bindable<string> MaxLastUpdateDate { get; } = new Bindable<string>("") { Value = "12-31-9999"};

        /// <summary>
        /// </summary>
        public BindableInt MinCombo { get; } = new BindableInt(0, 0, int.MaxValue) { Value = 0};

        /// <summary>
        /// </summary>
        public BindableInt MaxCombo { get; } = new BindableInt(int.MaxValue, 0, int.MaxValue) { Value = int.MaxValue};

        /// <summary>
        /// </summary>
        public Bindable<bool> DisplayOwnedMapsets => ConfigManager.DownloadDisplayOwnedMapsets ?? new Bindable<bool>(true) {Value = true};

        /// <summary>
        /// </summary>
        public Bindable<int> Page { get; } = new Bindable<int>(0) { Value = 0};

        /// <summary>
        /// </summary>
        public Bindable<DownloadSortBy> SortBy { get; } = new Bindable<DownloadSortBy>(DownloadSortBy.Artist) { Value = DownloadSortBy.Artist};

        /// <summary>
        /// </summary>
        public Bindable<bool> ReverseSort => ConfigManager.DownloadReverseSort ?? new Bindable<bool>(false) {Value = false};

        /// <summary>
        /// </summary>
        public TaskHandler<int, int> SearchTask { get; private set; }

        /// <summary>
        /// </summary>
        private List<DownloadableMapset> PreviousPageMapsets { get; set; }

        /// <summary>
        ///    Cached audio tracks for the song previews
        /// </summary>
        public Dictionary<int, IAudioTrack> AudioPreviews { get; private set; } = new Dictionary<int, IAudioTrack>();

        /// <summary>
        ///     The song preview that is currently playing
        /// </summary>
        private IAudioTrack CurrentPreview { get; set; } = new AudioTrackVirtual(10000);

        /// <summary>
        ///     Determines if the audio preview should be playing
        /// </summary>
        private bool ShouldPreviewPlay { get; set; } = true;

        /// <summary>
        /// </summary>
        public DownloadingScreen()
        {
            PreviousScreen = QuaverScreenType.Menu;
            Initialize();
        }

        /// <summary>
        /// </summary>
        public DownloadingScreen(QuaverScreenType previousScreen = QuaverScreenType.Menu)
        {
            PreviousScreen = previousScreen;
            Initialize();
        }

        /// <summary>
        /// </summary>
        private void Initialize()
        {
            if (AudioEngine.Track != null && AudioEngine.Track.IsPlaying)
                AudioEngine.Track?.Stop();

            CurrentSearchQuery.ValueChanged += OnSearchQueryChanged;
            FilterGameMode.ValueChanged += OnGameModeChanged;
            FilterRankedStatus.ValueChanged += OnRankedStatusChanged;
            MinDifficulty.ValueChanged += OnMinDifficultyChanged;
            MaxDifficulty.ValueChanged += OnMaxDifficultyChanged;
            MinBpm.ValueChanged += OnMinBpmChanged;
            MaxBpm.ValueChanged += OnMaxBpmChanged;
            MinLength.ValueChanged += OnMinLengthChanged;
            MaxLength.ValueChanged += OnMaxLengthChanged;
            MinLongNotePercent.ValueChanged += OnMinLongNotePercentChanged;
            MaxLongNotePercent.ValueChanged += OnMaxLongNotePercentChanged;
            MinPlayCount.ValueChanged += OnMinPlayCountChanged;
            MaxPlayCount.ValueChanged += OnMaxPlayCountChanged;
            MinUploadDate.ValueChanged += OnMinUploadDateChanged;
            MaxUploadDate.ValueChanged += OnMaxUploadDateChanged;
            MinLastUpdateDate.ValueChanged += OnMinLastUpdateDateChanged;
            MaxLastUpdateDate.ValueChanged += OnMaxLastUpdateDateChanged;
            DisplayOwnedMapsets.ValueChanged += OnDisplayOwnedMapsetsChanged;
            ReverseSort.ValueChanged += OnReverseSortChanged;
            MinCombo.ValueChanged += OnMinComboChanged;
            MaxCombo.ValueChanged += OnMaxComboChanged;
            Page.ValueChanged += OnPageChanged;
            SelectedMapset.ValueChanged += OnSelectedMapsetChanged;
            SortBy.ValueChanged += OnSortByChanged;

            SearchTask = new TaskHandler<int, int>(SearchMapsets);

#if !VISUAL_TESTS
           // SetRichPresence();
#endif
            View = new DownloadingScreenView(this);
            StartSearchTask();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0)
                return;

            HandleKeyPressEscape();
            HandleKeyPressCtrlP();
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressEscape()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                return;

            ExitToPreviousScreen();
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressCtrlP()
        {
            var state = KeyboardManager.CurrentState;

            if (state.IsKeyUp(Keys.LeftControl) && state.IsKeyUp(Keys.RightControl))
                return;

            if (!KeyboardManager.IsUniqueKeyPress(Keys.P))
                return;

            ShouldPreviewPlay = !ShouldPreviewPlay;

            Logger.Important($"Playing preview audio: {ShouldPreviewPlay}", LogType.Runtime);

            NotificationManager.Show(NotificationLevel.Info, ShouldPreviewPlay ? $"Music is currently paused!" : $"Music is now playing!");

            if (CurrentPreview.IsDisposed)
                return;

            if (ShouldPreviewPlay && !CurrentPreview.IsPlaying)
                CurrentPreview.Play();
            else if (!ShouldPreviewPlay && CurrentPreview.IsPlaying)
                CurrentPreview.Pause();
        }

        /// <summary>
        /// </summary>
        public void ExitToPreviousScreen()
        {
            switch (PreviousScreen)
            {
                case QuaverScreenType.Select:
                    Exit(() => new SelectionScreen());
                    break;
                case QuaverScreenType.Lobby:
                    if (OnlineManager.Connected)
                        Exit(() => new MultiplayerLobbyScreen());
                    else
                        Exit(() => new MainMenuScreen());
                    break;
                case QuaverScreenType.Multiplayer:
                    if (OnlineManager.CurrentGame != null)
                        Exit(() => new MultiplayerGameScreen());
                    else
                        Exit(() => new MainMenuScreen());
                    break;
                case QuaverScreenType.Music:
                    Exit(() => new MusicPlayerScreen());
                    break;
                case QuaverScreenType.Theatre:
                    Exit(() => new TheaterScreen());
                    break;
                default:
                    Exit(() => new MainMenuScreen());
                    break;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxPlayCountChanged(object sender, BindableValueChangedEventArgs<int> e) => StartSearchTask();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinPlayCountChanged(object sender, BindableValueChangedEventArgs<int> e) => StartSearchTask();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            CurrentSearchQuery?.Dispose();
            FilterGameMode?.Dispose();
            FilterRankedStatus?.Dispose();
            MinDifficulty?.Dispose();
            MaxDifficulty?.Dispose();
            MinBpm?.Dispose();
            MaxBpm?.Dispose();
            MinLength?.Dispose();
            MaxLength?.Dispose();
            SearchTask?.Dispose();
            MinLongNotePercent?.Dispose();
            MaxLongNotePercent?.Dispose();
            MinPlayCount?.Dispose();
            MaxPlayCount?.Dispose();
            MinUploadDate?.Dispose();
            MaxUploadDate?.Dispose();
            Mapsets?.Dispose();
            SelectedMapset?.Dispose();
            SortBy?.Dispose();
            DisposePreviews();
            MinLastUpdateDate?.Dispose();
            MaxLastUpdateDate?.Dispose();
            MinCombo?.Dispose();
            MaxCombo?.Dispose();

            // ReSharper disable twice DelegateSubtraction
            DisplayOwnedMapsets.ValueChanged -= OnDisplayOwnedMapsetsChanged;
            ReverseSort.ValueChanged -= OnReverseSortChanged;

            if (DisplayOwnedMapsets != ConfigManager.DownloadDisplayOwnedMapsets)
                DisplayOwnedMapsets?.Dispose();

            if (ReverseSort != ConfigManager.DownloadReverseSort)
                ReverseSort?.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void StartSearchTask()
        {
            // Handles whether or not the search task will be performed again when scrolling to the bottom
            if (Page.Value == 0)
            {
                Mapsets.Value = new List<DownloadableMapset>();
                PreviousPageMapsets = new List<DownloadableMapset>();
            }
            else if (PreviousPageMapsets.Count < 50)
                return;

            if (SearchTask.IsRunning)
                SearchTask.Cancel();

            SearchTask.Run(0, 250);
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private int SearchMapsets(int val, CancellationToken token)
        {
            lock (Mapsets)
            {
                var request = new APIRequestMapsetSearch(CurrentSearchQuery.Value, FilterGameMode.Value,
                    FilterRankedStatus.Value, MinDifficulty.Value, MaxDifficulty.Value, MinBpm.Value,
                    MaxBpm.Value, MinLength.Value, MaxLength.Value, MinLongNotePercent.Value, MaxLongNotePercent.Value,
                    MinPlayCount.Value, MaxPlayCount.Value, MinUploadDate.Value, MaxUploadDate.Value,
                    MinLastUpdateDate.Value, MaxLastUpdateDate.Value, MinCombo.Value, MaxCombo.Value, Page.Value);

                var result = request.ExecuteRequest();

                List<DownloadableMapset> mapsets;

                result.Mapsets.ForEach(x => x.IsOwned = MapDatabaseCache.FindSet(x.Id) != null);

                mapsets = !DisplayOwnedMapsets.Value ? result?.Mapsets?.FindAll(x => !x.IsOwned) : result.Mapsets;
                mapsets = SortMapsets(mapsets);

                PreviousPageMapsets = result?.Mapsets ?? new List<DownloadableMapset>();

                if (mapsets == null)
                {
                    Mapsets.Value = new List<DownloadableMapset>();
                    return 0;
                }

                if (Page.Value == 0)
                    Mapsets.Value = mapsets;
                else
                    Mapsets.AddRange(mapsets);
            }

            return 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        private List<DownloadableMapset> SortMapsets(List<DownloadableMapset> mapsets)
        {
            if (mapsets == null || mapsets.Count <= 1)
                return mapsets;

            switch (SortBy.Value)
            {
                case DownloadSortBy.Newest:
                    if (ReverseSort.Value)
                        return mapsets.OrderByDescending(x => x.DateLastUpdated).ToList();

                    return mapsets;
                case DownloadSortBy.Artist:
                    if (ReverseSort.Value)
                        return mapsets.OrderByDescending(x => x.Artist).ToList();

                    return mapsets.OrderBy(x => x.Artist).ToList();
                case DownloadSortBy.Title:
                    if (ReverseSort.Value)
                        return mapsets.OrderByDescending(x => x.Title).ToList();

                    return mapsets.OrderBy(x => x.Title).ToList();
                case DownloadSortBy.Creator:
                    if (ReverseSort.Value)
                        return mapsets.OrderByDescending(x => x.CreatorUsername).ToList();

                    return mapsets.OrderBy(x => x.CreatorUsername).ToList();
                case DownloadSortBy.Bpm:
                    if (ReverseSort.Value)
                        return mapsets.OrderByDescending(x => x.Bpms.Max()).ToList();

                    return mapsets.OrderBy(x => x.Bpms.Max()).ToList();
                case DownloadSortBy.Length:
                    if (ReverseSort.Value)
                        return mapsets.OrderByDescending(x => x.MaxLengthSeconds).ToList();

                    return mapsets.OrderBy(x => x.MaxLengthSeconds).ToList();
                case DownloadSortBy.Difficulty:
                    if (ReverseSort.Value)
                        return mapsets.OrderByDescending(x => x.DifficultyRange.Max()).ToList();

                    return mapsets.OrderBy(x => x.DifficultyRange.Max()).ToList();
                case DownloadSortBy.LNs:
                    if (ReverseSort.Value)
                        return mapsets.OrderByDescending(x => x.MaxLongNotePercent).ToList();

                    return mapsets.OrderBy(x => x.MaxLongNotePercent).ToList();
                case DownloadSortBy.PlayCount:
                    if (ReverseSort.Value)
                        return mapsets.OrderByDescending(x => x.MaxPlayCount).ToList();

                    return mapsets.OrderBy(x => x.MaxPlayCount).ToList();
                case DownloadSortBy.MaxCombo:
                    if (ReverseSort.Value)
                        return mapsets.OrderByDescending(x => x.MaxCombo).ToList();

                    return mapsets.OrderBy(x => x.MaxCombo).ToList();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSearchQueryChanged(object sender, BindableValueChangedEventArgs<string> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameModeChanged(object sender, BindableValueChangedEventArgs<DownloadFilterMode> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankedStatusChanged(object sender, BindableValueChangedEventArgs<DownloadFilterRankedStatus> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxDifficultyChanged(object sender, BindableValueChangedEventArgs<float> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinDifficultyChanged(object sender, BindableValueChangedEventArgs<float> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxBpmChanged(object sender, BindableValueChangedEventArgs<float> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinBpmChanged(object sender, BindableValueChangedEventArgs<float> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxLengthChanged(object sender, BindableValueChangedEventArgs<int> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinLengthChanged(object sender, BindableValueChangedEventArgs<int> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxLongNotePercentChanged(object sender, BindableValueChangedEventArgs<int> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinLongNotePercentChanged(object sender, BindableValueChangedEventArgs<int> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxUploadDateChanged(object sender, BindableValueChangedEventArgs<string> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinUploadDateChanged(object sender, BindableValueChangedEventArgs<string> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisplayOwnedMapsetsChanged(object sender, BindableValueChangedEventArgs<bool> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReverseSortChanged(object sender, BindableValueChangedEventArgs<bool> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSortByChanged(object sender, BindableValueChangedEventArgs<DownloadSortBy> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinLastUpdateDateChanged(object sender, BindableValueChangedEventArgs<string> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxLastUpdateDateChanged(object sender, BindableValueChangedEventArgs<string> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinComboChanged(object sender, BindableValueChangedEventArgs<int> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxComboChanged(object sender, BindableValueChangedEventArgs<int> e) => Page.Value = 0;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageChanged(object sender, BindableValueChangedEventArgs<int> e) => StartSearchTask();

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedMapsetChanged(object sender, BindableValueChangedEventArgs<DownloadableMapset> e)
            => LoadAudioPreview();

        /// <summary>
        ///     Loads an plays the audio preview for the selected map
        /// </summary>
        private void LoadAudioPreview()
        {
            if (CurrentPreview != null && CurrentPreview.IsPlaying)
                CurrentPreview?.Stop();

            if (SelectedMapset.Value == null)
                return;

            var mapset = SelectedMapset.Value;

            ThreadScheduler.Run(() =>
            {
                lock (CurrentPreview)
                lock (AudioPreviews)
                {
                    try
                    {
                        if (CurrentPreview != null && CurrentPreview.IsPlaying)
                            CurrentPreview?.Stop();

                        if (AudioPreviews.ContainsKey(mapset.Id))
                        {
                            CurrentPreview = AudioPreviews[mapset.Id];
                            CurrentPreview.Seek(0);

                            if (ShouldPreviewPlay)
                                CurrentPreview.Play();
                            return;
                        }

                        var uri = new Uri($"https://cdn.quavergame.com/audio-previews/{mapset.Id}.mp3");
                        CurrentPreview = new AudioTrack(uri, false, false);
                        AudioPreviews.Add(mapset.Id, CurrentPreview);

                        if (ShouldPreviewPlay)
                            CurrentPreview.Play();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, LogType.Network);
                    }
                }
            });
        }

        /// <summary>
        /// </summary>
        private void SetRichPresence()
        {
            try
            {
                DiscordHelper.Presence.Details = "Downloading Maps";
                DiscordHelper.Presence.State = "In the menus";
                DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
                DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
                DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);
                DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        private void DisposePreviews()
        {
            foreach (var track in AudioPreviews.Values)
            {
                if (!track.IsDisposed)
                    track.Dispose();

                if (!CurrentPreview.IsDisposed)
                    CurrentPreview.Dispose();
            }

            AudioPreviews = null;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InMenus, -1, "", 1, "", 0);
    }
}