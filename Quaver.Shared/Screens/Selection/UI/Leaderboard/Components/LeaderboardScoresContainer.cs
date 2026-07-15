using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Skinning;
using TagLib.Ape;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Scheduling;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Components
{
    public class LeaderboardScoresContainer : PoolableScrollContainer<Score>, ILoadable, IFetchedScoreHandler
    {
        /// <summary>
        ///     The parent leaderboard container
        /// </summary>
        private LeaderboardContainer Container { get; }

        /// <summary>
        /// </summary>
        private Sprite ScrollbarBackground { get; set; }

        /// <summary>
        ///     Loading wheel displayed when scores are loading
        /// </summary>
        private LoadingWheel LoadingWheel { get; set; }

        /// <summary>
        ///     Gives the user a status update
        /// </summary>
        private SpriteTextPlus StatusText { get; set; }

        /// <summary>
        ///     Rows detached while scores are loading, ready to be rebound by the next fetch.
        /// </summary>
        private List<DrawableLeaderboardScore> ReusableRows { get; } = new List<DrawableLeaderboardScore>();

        /// <summary>
        ///     Rows waiting to be rebound. Processing one per frame prevents all cached text from being rebuilt at once.
        /// </summary>
        private Queue<(DrawableLeaderboardScore Row, Score Item, int Index)> PendingRowUpdates { get; } =
            new Queue<(DrawableLeaderboardScore Row, Score Item, int Index)>();

        /// <summary>
        ///     Rows that must stay hidden until their new content has been applied.
        /// </summary>
        private HashSet<DrawableLeaderboardScore> RowsPendingContent { get; } = new HashSet<DrawableLeaderboardScore>();

        /// <summary>
        ///     Whether a leaderboard fetch has been requested and the old rows should no longer be rebound.
        /// </summary>
        private bool LoadingRequested { get; set; }

        /// <summary>
        ///     The button to update the map to the latest version
        /// </summary>
        private IconButton UpdateButton { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        public LeaderboardScoresContainer(LeaderboardContainer container) : base(new List<Score>(), 12, 0,
            new ScalableVector2(container.ScoresContainerBackground.Width - 4, container.ScoresContainerBackground.Height - 4),
            new ScalableVector2(container.ScoresContainerBackground.Width - 4, container.ScoresContainerBackground.Height - 4))
        {
            Container = container;
            Alpha = 0;

            if (PoolSize != int.MaxValue)
                PoolSize = (int) (PoolSize * WindowManager.BaseToVirtualRatio) + 2;

            InputEnabled = true;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;
            IsMinScrollYEnabled = true;

            CreateScrollbar();
            CreateLoadingWheel();
            CreateStatusText();
            CreateUpdateButton();

            Container.FetchScoreTask.OnCompleted += OnScoresRetrieved;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Pool != null)
                ScrollbarBackground.Visible = PendingRowUpdates.Count == 0 && Pool.Count > 10 &&
                                              !Pool.Last().Item.IsEmptyScore;
            else
                ScrollbarBackground.Visible = false;

            InputEnabled = PendingRowUpdates.Count == 0 &&
                           GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            if (!LoadingRequested)
                ProcessNextPendingRow();

            // Only make the update perform hover if absolutely necessary
            UpdateButton.IsPerformingFadeAnimations = MapManager.Selected.Value != null
                                                      && MapManager.Selected.Value.NeedsOnlineUpdate &&
                                                      LoadingWheel.Alpha < 0.1f;

            base.Update(gameTime);
            ApplyPoolVisibility();
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container.FetchScoreTask.OnCompleted -= OnScoresRetrieved;
            PendingRowUpdates.Clear();
            RowsPendingContent.Clear();
            ReusableRows.ForEach(x => x.Destroy());
            ReusableRows.Clear();
            base.Destroy();
        }

        /// <summary>
        ///     Creates the scrollbar sprite and aligns it properly
        /// </summary>
        private void CreateScrollbar()
        {
            ScrollbarBackground = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = 30,
                Size = new ScalableVector2(4, Height),
                Tint = ColorHelper.HexToColor("#474747"),
                Visible = false
            };

            MinScrollBarY = -805 - (int) Scrollbar.Height / 2;
            Scrollbar.Width = ScrollbarBackground.Width;
            Scrollbar.Parent = ScrollbarBackground;
            Scrollbar.Alignment = Alignment.BotCenter;
            Scrollbar.Tint = Color.White;
        }

        /// <summary>
        ///     Creates the loading wheel for the screen
        /// </summary>
        private void CreateLoadingWheel()
        {
            LoadingWheel = new LoadingWheel
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(50, 50),
                Alpha = 0
            };
        }

        protected override PoolableSprite<Score> CreateObject(Score item, int index) => new DrawableLeaderboardScore(this, item, index, false);

        /// <summary>
        ///     Fades the wheel in to make it appear as if it is loading
        /// </summary>
        public void StartLoading()
        {
            LoadingRequested = true;

            ScheduleUpdate(() =>
            {
                LoadingWheel.Animations.RemoveAll(x => x.Properties != AnimationProperty.Rotation);
                LoadingWheel.FadeTo(1, Easing.Linear, 250);
                FadeStatusTextOut();
                RecyclePool();
                ContentContainer.Height = Height;

                SnapToTop();

                AvailableItems?.Clear();
                ScrollbarBackground.Visible = false;
                UpdateButton.ClearAnimations();
                UpdateButton.IsClickable = false;
                UpdateButton.FadeTo(0, Easing.Linear, 250);
            });
        }

        /// <summary>
        ///     Fades the wheel out to 0.
        /// </summary>
        public void StopLoading()
        {
            LoadingWheel.Animations.RemoveAll(x => x.Properties != AnimationProperty.Rotation);
            LoadingWheel.FadeTo(0, Easing.Linear, 250);
            FadeStatusTextOut();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="store"></param>
        public void HandleFetchedScores(Map map, FetchedScoreStore store)
        {
            if (map == null)
            {
                StatusText.Text = SelectionLocalization.Get("There is currently no map selected!").ToUpper();
                FadeStatusTextIn();
                return;
            }

            var isConnected = OnlineManager.Connected;
            var isDonator = OnlineManager.IsDonator;

            //var isConnected = true;
            //var isDonator = true;

            // User isn't online
            if (RequiresOnline() && !isConnected)
            {
                StatusText.Text = SelectionLocalization.Get("You must be online to access this leaderboard!").ToUpper();
                FadeStatusTextIn();
                return;
            }

            // User isn't a donator
            if (RequiresOnline() && RequiresDonator() && !isDonator)
            {
                StatusText.Text = SelectionLocalization.Get("You must be a donator to access this leaderboard!").ToUpper();
                FadeStatusTextIn();
                return;
            }

            // No scores are available
            if (store.Scores.Count == 0)
            {
                // The user's map is not up-to-date, so prompt them of this.
                if (map.NeedsOnlineUpdate)
                {
                    StatusText.Text = SelectionLocalization.Get("Your map is outdated. Please update it!").ToUpper();
                    UpdateButton.ClearAnimations();
                    UpdateButton.IsClickable = true;
                    UpdateButton.FadeTo(1, Easing.Linear, 250);
                }
                else
                {
                    UpdateButton.ClearAnimations();
                    UpdateButton.IsClickable = false;
                    UpdateButton.FadeTo(0, Easing.Linear, 250);

                    // The map isn't ranked, but the user is a donator, so they can access leaderboards on all maps
                    if (map.RankedStatus != RankedStatus.Ranked && isDonator && ConfigManager.LeaderboardSection.Value != LeaderboardType.Local)
                        StatusText.Text = SelectionLocalization.Get("Scores on this map will be unranked!").ToUpper();
                    else if (ConfigManager.LeaderboardSection.Value != LeaderboardType.Local)
                    {
                        switch (map.RankedStatus)
                        {
                            case RankedStatus.NotSubmitted:
                                StatusText.Text = SelectionLocalization.Get("This map is not submitted online!").ToUpper();
                                break;
                            case RankedStatus.Unranked:
                                StatusText.Text = SelectionLocalization.Get("This map is not ranked!").ToUpper();
                                break;
                            case RankedStatus.Ranked:
                                StatusText.Text = SelectionLocalization.Get("No scores available. Be the first!").ToUpper();
                                break;
                            case RankedStatus.DanCourse:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                        StatusText.Text = SelectionLocalization.Get("No scores available. Be the first!").ToUpper();
                }

                FadeStatusTextIn();
                return;
            }
        }

        public override void RecalculateContainerHeight(bool usePoolCount = false)
        {
            base.RecalculateContainerHeight(usePoolCount);

            if (Pool == null)
                return;

            // If the last score is empty, cut off the content container so it doesn't scroll.
            if (Pool.LastOrDefault()?.Item.IsEmptyScore ?? false)
                ContentContainer.Height = Height;
        }

        /// <summary>
        ///     If the leaderboard requires the user to be online
        /// </summary>
        /// <returns></returns>
        private bool RequiresOnline()
        {
            switch (ConfigManager.LeaderboardSection.Value)
            {
                case LeaderboardType.Local:
                    return false;
                default:
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     If the leaderboard requires donator privileges
        /// </summary>
        /// <returns></returns>
        private bool RequiresDonator() => RequiresOnline()
                                          && (ConfigManager.LeaderboardSection.Value == LeaderboardType.Country
                                          || ConfigManager.LeaderboardSection.Value == LeaderboardType.Friends
                                          || ConfigManager.LeaderboardSection.Value == LeaderboardType.All);

        /// <summary>
        ///     Creates <see cref="StatusText"/>
        /// </summary>
        private void CreateStatusText()
        {
            StatusText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "", 16)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Visible = false,
                Tint = SkinManager.Skin?.SongSelect?.LeaderboardStatusTextColor ?? Color.White
            };
        }

        /// <summary>
        ///     Creates <see cref="UpdateButton"/>
        /// </summary>
        private void CreateUpdateButton()
        {
            UpdateButton = new IconButton(UserInterface.BlankBox, (o, e) =>
                {
                    ThreadScheduler.Run(() => MapManager.UpdateMapToLatestVersion(MapManager.Selected.Value));
                    StartLoading();
                })
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(220, 40),
                Y = StatusText.Y + StatusText.Height + 28,
                Alpha = 0,
                IsPerformingFadeAnimations = false,
                IsClickable = false,
                Image = UserInterface.UpdateButton
            };
        }

        private void FadeStatusTextIn()
        {
            StatusText.Visible = true;
            StatusText.ClearAnimations();
            StatusText.FadeTo(1, Easing.Linear, 250);
        }

        private void FadeStatusTextOut()
        {
            StatusText.ClearAnimations();
            StatusText.Visible = false;
            StatusText.FadeTo(0, Easing.Linear, 250);
        }

        /// <summary>
        ///     Called upon retrieving
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScoresRetrieved(object sender, TaskCompleteEventArgs<Map, FetchedScoreStore> e)
        {
            ScheduleUpdate(() =>
            {
                RecyclePool();
                SnapToTop();

                AvailableItems = e.Result.Scores;

                if (AvailableItems == null)
                {
                    LoadingRequested = false;
                    return;
                }

                var MAX_SHOWN_ITEMS = Height / DrawableLeaderboardScore.ScoreHeight;

                // We don't have enough scores in the leaderboard, so fill it with empty scores, so the leaderboard
                // still preserves the table look
                if (AvailableItems.Count < MAX_SHOWN_ITEMS && AvailableItems.Count != 0)
                {
                    var count = MAX_SHOWN_ITEMS - AvailableItems.Count;

                    for (var i = 0; i < count; i++)
                        AvailableItems.Add(new Score { IsEmptyScore = true});
                }

                PopulatePool();
                LoadingRequested = false;
            });
        }

        /// <summary>
        ///     Snaps to the top of the container
        /// </summary>
        private void SnapToTop()
        {
            // Snap to the top of the container
            ContentContainer.Animations.Clear();
            ContentContainer.Y = 0;
            PreviousContentContainerY = ContentContainer.Y;
            TargetY = PreviousContentContainerY;
            PreviousTargetY = PreviousContentContainerY;

            PoolStartingIndex = 0;
        }

        private void RecyclePool()
        {
            PendingRowUpdates.Clear();
            RowsPendingContent.Clear();

            if (Pool is null)
                return;

            foreach (var row in Pool.OfType<DrawableLeaderboardScore>())
            {
                row.RemoveScheduledUpdates();
                row.ChildContainer.RemoveScheduledUpdates();
                RemoveContainedDrawable(row);
                row.SetScrollVisibility(false);
                ReusableRows.Add(row);
            }

            Pool.Clear();
        }

        private void PopulatePool()
        {
            Pool ??= new List<PoolableSprite<Score>>();

            var count = Math.Min(PoolSize, AvailableItems.Count);

            for (var i = 0; i < count; i++)
            {
                DrawableLeaderboardScore row;

                if (ReusableRows.Count != 0)
                {
                    var reusableIndex = ReusableRows.Count - 1;
                    row = ReusableRows[reusableIndex];
                    ReusableRows.RemoveAt(reusableIndex);
                }
                else
                {
                    row = (DrawableLeaderboardScore) CreateObject(AvailableItems[i], i);
                    row.DestroyIfParentIsNull = false;
                }

                row.Y = i * row.Height + PaddingTop;
                row.SetScrollVisibility(false);
                Pool.Add(row);
                PendingRowUpdates.Enqueue((row, AvailableItems[i], i));
                RowsPendingContent.Add(row);
            }

            RecalculateContainerHeight();
        }

        /// <summary>
        ///     Rebinds a single row so its text render targets are built separately from the remaining rows.
        /// </summary>
        private void ProcessNextPendingRow()
        {
            if (PendingRowUpdates.Count == 0)
                return;

            var (row, item, index) = PendingRowUpdates.Dequeue();
            RowsPendingContent.Remove(row);

            AddContainedDrawable(row);
            row.UpdateContent(item, index);

            var isVisible = row.ScreenRectangle.Intersects(ScreenRectangle);
            row.SetScrollVisibility(isVisible);

            if (isVisible)
                row.AddScheduledUpdate(() => row.ChildContainer.FadeIn());
        }

        private void ApplyPoolVisibility()
        {
            if (Pool == null)
                return;

            foreach (var score in Pool.OfType<DrawableLeaderboardScore>())
            {
                if (RowsPendingContent.Contains(score))
                {
                    score.SetScrollVisibility(false);
                    continue;
                }

                score.SetScrollVisibility(score.ScreenRectangle.Intersects(ScreenRectangle));
            }
        }
    }
}
