using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Helpers;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.MapsetSearch;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Downloading.UI.Search;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.Music;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Theater;
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
        public Bindable<string> CurrentSearchQuery { get; } = new Bindable<string>("") { Value = "" };

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
        public BindableFloat MinDifficulty { get; } = new BindableFloat(0, 0, 9999) { Value = 0 };

        /// <summary>
        /// </summary>
        public BindableFloat MaxDifficulty { get; } = new BindableFloat(0, 0, float.MaxValue) { Value = 9999 };

        /// <summary>
        /// </summary>
        public BindableFloat MinBpm { get; } = new BindableFloat(0, 0, float.MaxValue) { Value = 0 };

        /// <summary>
        /// </summary>
        public BindableFloat MaxBpm { get; } = new BindableFloat(0, 0, float.MaxValue) { Value = 9999 };

        /// <summary>
        /// </summary>
        public BindableInt MinLength { get; } = new BindableInt(0, 0, int.MaxValue) { Value = 0 };

        /// <summary>
        /// </summary>
        public BindableInt MaxLength { get; } = new BindableInt(0, 0, int.MaxValue) { Value = int.MaxValue };

        /// <summary>
        /// </summary>
        public BindableInt MinLongNotePercent { get; } = new BindableInt(0, 0, 100) { Value = 0 };

        /// <summary>
        /// </summary>
        public BindableInt MaxLongNotePercent { get; } = new BindableInt(0, 0, 100) { Value = 100 };

        /// <summary>
        /// </summary>
        public BindableInt MinPlayCount { get; } = new BindableInt(0, 0, int.MaxValue) { Value = 0 };

        /// <summary>
        /// </summary>
        public BindableInt MaxPlayCount { get; } = new BindableInt(0, 0, int.MaxValue) { Value = int.MaxValue };

        /// <summary>
        /// </summary>
        public Bindable<string> MinUploadDate { get; } = new Bindable<string>("") { Value = "01-01-1970" };

        /// <summary>
        /// </summary>
        public Bindable<string> MaxUploadDate { get; } = new Bindable<string>("") { Value = "12-31-9999" };

        /// <summary>
        /// </summary>
        public Bindable<string> MinLastUpdateDate { get; } = new Bindable<string>("") { Value = "01-01-1970" };

        /// <summary>
        /// </summary>
        public Bindable<string> MaxLastUpdateDate { get; } = new Bindable<string>("") { Value = "12-31-9999" };

        /// <summary>
        /// </summary>
        public BindableInt MinCombo { get; } = new BindableInt(0, 0, int.MaxValue) { Value = 0 };

        /// <summary>
        /// </summary>
        public BindableInt MaxCombo { get; } = new BindableInt(int.MaxValue, 0, int.MaxValue) { Value = int.MaxValue };

        /// <summary>
        /// </summary>
        public Bindable<bool> DisplayOwnedMapsets => ConfigManager.DownloadDisplayOwnedMapsets ?? new Bindable<bool>(true) { Value = true };

        /// <summary>
        /// </summary>
        public Bindable<int> Page { get; } = new Bindable<int>(0) { Value = 0 };

        /// <summary>
        ///     Determines if the user has reached the end of the mapset list
        /// </summary>
        public Bindable<bool> ReachedEnd { get; } = new Bindable<bool>(false) { Value = false };

        /// <summary>
        /// </summary>
        public Bindable<DownloadSortBy> SortBy { get; } = new Bindable<DownloadSortBy>(DownloadSortBy.Newest)
        {
            Value = DownloadSortBy.Newest
        };

        /// <summary>
        /// </summary>
        public Bindable<bool> ReverseSort => ConfigManager.DownloadReverseSort ?? new Bindable<bool>(false) { Value = false };

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
        ///     The previous search query for the screen
        /// </summary>
        public static string PreviousSearchQuery { get; set; }

        /// <summary>
        ///     If the download screen has recommended difficulty before
        /// </summary>
        private static bool HasRecommendedDifficulty { get; set; }

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
            ModManager.RemoveSpeedMods();

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

            ScreenExiting += OnScreenExiting;

            SearchTask = new TaskHandler<int, int>(SearchMapsets);

#if !VISUAL_TESTS
            // SetRichPresence();
#endif
            View = new DownloadingScreenView(this);

            if (PreviousSearchQuery != null)
                CurrentSearchQuery.Value = PreviousSearchQuery;
            else if (OnlineManager.Connected && OnlineManager.Self?.Stats[ConfigManager.SelectedGameMode.Value]?.OverallPerformanceRating < 150)
                CurrentSearchQuery.Value = "Easy";

            StartSearchTask();
        }

        public override void OnFirstUpdate()
        {
            if (AudioEngine.Track != null)
                AudioEngine.Track?.Stop();

            if (!HasRecommendedDifficulty)
            {
                ShowRecommendedDifficultyDialog();
                HasRecommendedDifficulty = true;
            }
        }

        public void OnScreenExiting(object sender, ScreenExitingEventArgs e)
        {
            ShouldPreviewPlay = false;
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
            if (Exiting)
                return;

            if (DialogManager.Dialogs.Count != 0)
                return;

            HandleKeyPressEscape();
            HandleKeyPressCtrlP();
            HandleKeyPressNext();
            HandleKeyPressPrevious();
            HandleKeyPressEnter();
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

            if (!KeyboardManager.IsCtrlDown())
                return;

            if (!KeyboardManager.IsUniqueKeyPress(Keys.P))
                return;

            ShouldPreviewPlay = !ShouldPreviewPlay;

            Logger.Important($"Playing preview audio: {ShouldPreviewPlay}", LogType.Runtime);

            NotificationManager.Show(NotificationLevel.Info, !ShouldPreviewPlay ? $"Music is currently paused!" : $"Music is now playing!");

            if (CurrentPreview.IsDisposed)
                return;

            if (ShouldPreviewPlay && !CurrentPreview.IsPlaying)
                CurrentPreview.Play();
            else if (!ShouldPreviewPlay && CurrentPreview.IsPlaying)
                CurrentPreview.Pause();
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressPrevious()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Left) && !KeyboardManager.IsUniqueKeyPress(Keys.Up))
                return;

            var state = KeyboardManager.CurrentState;

            if (state.IsKeyDown(Keys.LeftAlt) || state.IsKeyDown(Keys.RightAlt))
                return;

            if (Mapsets.Value.Count == 0)
                return;

            var index = Mapsets.Value.IndexOf(SelectedMapset.Value);

            if (index == -1 || index == 0)
                return;

            SelectedMapset.Value = Mapsets.Value[index - 1];
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressEnter()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Enter))
                return;

            if (SelectedMapset.Value == null)
                return;

            if (MapsetDownloadManager.CurrentDownloads.Any(x => x.MapsetId == SelectedMapset.Value.Id))
            {
                NotificationManager.Show(NotificationLevel.Warning, $"This mapset is already downloading!");
                return;
            }

            MapsetDownloadManager.Download(SelectedMapset.Value.Id, SelectedMapset.Value.Artist, SelectedMapset.Value.Title);
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressNext()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Right) && !KeyboardManager.IsUniqueKeyPress(Keys.Down))
                return;

            var state = KeyboardManager.CurrentState;

            if (state.IsKeyDown(Keys.LeftAlt) || state.IsKeyDown(Keys.RightAlt))
                return;

            if (Mapsets.Value.Count == 0)
                return;

            var index = Mapsets.Value.IndexOf(SelectedMapset.Value);

            if (index == -1 || index == Mapsets.Value.Count - 1)
                return;

            SelectedMapset.Value = Mapsets.Value[index + 1];
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
            {
                ReachedEnd.Value = true;
                return;
            }

            if (SearchTask.IsRunning)
                SearchTask.Cancel();

            SearchTask.Run(0, 50);
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
                List<DownloadableMapset> mapsets;

                var request = CreateSearchRequest(Page.Value);

                var result = request.ExecuteRequest();

                if (result.Mapsets == null)
                {
                    Mapsets.Value = new List<DownloadableMapset>();
                    return 0;
                }

                result.Mapsets.ForEach(x => x.IsOwned = MapDatabaseCache.FindSet(x.Id) != null);

                mapsets = !DisplayOwnedMapsets.Value ? result?.Mapsets?.FindAll(x => !x.IsOwned) : result.Mapsets;

                if (!DisplayOwnedMapsets.Value)
                {
                    List<DownloadableMapset> nextPageSets = null;
                    const int MAX_MAPSETS_PER_PAGE = 50;

                    while (!DisplayOwnedMapsets.Value && (nextPageSets == null || nextPageSets?.Count == MAX_MAPSETS_PER_PAGE)
                                                      && mapsets.Count < 10)
                    {
                        Page.ChangeWithoutTrigger(Page.Value + 1);
                        nextPageSets = CreateSearchRequest(Page.Value).ExecuteRequest().Mapsets;

                        nextPageSets.ForEach(x => x.IsOwned = MapDatabaseCache.FindSet(x.Id) != null);
                        mapsets.AddRange(nextPageSets.FindAll(x => !x.IsOwned));
                    }
                }

                mapsets = SortMapsets(mapsets);

                PreviousPageMapsets = new List<DownloadableMapset>(result?.Mapsets);

                if (mapsets == null)
                {
                    Mapsets.Value = new List<DownloadableMapset>();
                    return 0;
                }

                if (Page.Value == 0)
                {
                    Mapsets.Value = mapsets;

                    if (Mapsets.Value.Count != 0)
                        SelectedMapset.Value = Mapsets.Value.First();
                }
                else
                    Mapsets.AddRange(mapsets);
            }

            return 0;
        }

        /// <summary>
        ///        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private APIRequestMapsetSearch CreateSearchRequest(int page)
        {
            return new APIRequestMapsetSearch(CurrentSearchQuery.Value, FilterGameMode.Value,
                FilterRankedStatus.Value, MinDifficulty.Value, MaxDifficulty.Value, MinBpm.Value,
                MaxBpm.Value, MinLength.Value, MaxLength.Value, MinLongNotePercent.Value, MaxLongNotePercent.Value,
                MinPlayCount.Value, MaxPlayCount.Value, MinUploadDate.Value, MaxUploadDate.Value,
                MinLastUpdateDate.Value, MaxLastUpdateDate.Value, MinCombo.Value, MaxCombo.Value, page);
        }
        /// <summary>
        /// </summary>
        /// <param name="mapsets"></param>
        /// <returns></returns>
        private List<DownloadableMapset> SortMapsets(List<DownloadableMapset> mapsets) => mapsets;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSearchQueryChanged(object sender, BindableValueChangedEventArgs<string> e)
        {
            PreviousSearchQuery = e.Value;
            Page.Value = 0;
            ReachedEnd.Value = false;
        }

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

            ThreadScheduler.Run(async () =>
            {
                await Task.Delay(250);

                if (SelectedMapset.Value != mapset)
                {
                    Logger.Debug($"Skipped preview load on: {mapset.Artist} - {mapset.Title}", LogType.Runtime, false);
                    return;
                }

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
                DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
                DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
                DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);

                RichPresenceHelper.UpdateRichPresence("In the menus", "Downloading Maps");
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
        }

        /// <summary>
        ///     Shows a dialog to where the game can recommend a difficulty for them
        /// </summary>
        public void ShowRecommendedDifficultyDialog()
        {
            DialogManager.Show(new YesNoDialog("RECOMMEND DIFFICULTY",
                "Would you like to find recommended maps around your skill level\nfor the selected game mode?", () =>
                {
                    if (!OnlineManager.Connected || OnlineManager.Self == null)
                    {
                        CurrentSearchQuery.Value = "Easy";
                        return;
                    }

                    var rating = OnlineManager.Self.Stats[ConfigManager.SelectedGameMode.Value].OverallPerformanceRating;

                    var aproxLevel = rating / 20f;

                    if (rating == 0 || aproxLevel < 5)
                        CurrentSearchQuery.Value = "Easy";
                    else if (aproxLevel < 10)
                        CurrentSearchQuery.Value = "Normal";
                    else if (aproxLevel < 20)
                        CurrentSearchQuery.Value = "Hard";
                    else if (aproxLevel < 28)
                        CurrentSearchQuery.Value = "Insane";
                    else
                        CurrentSearchQuery.Value = "";
                }));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InMenus, -1, "", 1, "", 0);
    }
}