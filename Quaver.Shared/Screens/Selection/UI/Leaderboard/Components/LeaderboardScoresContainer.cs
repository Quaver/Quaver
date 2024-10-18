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
        ///     If the scores have finished loading
        /// </summary>
        private bool FinishedLoading { get; set; }

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
                ScrollbarBackground.Visible = Pool.Count > 10 && !Pool.Last().Item.IsEmptyScore;
            else
                ScrollbarBackground.Visible = false;

            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            if (FinishedLoading && Pool.Count >= 10 && Pool.First().Parent != ContentContainer)
            {
                Pool.ForEach(x =>
                {
                    AddContainedDrawable(x);

                    var score = x as DrawableLeaderboardScore;
                    score?.AddScheduledUpdate(() => score.ChildContainer.FadeIn());
                });
            }

            // Only make the update perform hover if absolutely necessary
            UpdateButton.IsPerformingFadeAnimations = MapManager.Selected.Value != null
                                                      && MapManager.Selected.Value.NeedsOnlineUpdate &&
                                                      LoadingWheel.Alpha < 0.1f;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container.FetchScoreTask.OnCompleted -= OnScoresRetrieved;
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
            ScheduleUpdate(() =>
            {
                LoadingWheel.Animations.RemoveAll(x => x.Properties != AnimationProperty.Rotation);
                LoadingWheel.FadeTo(1, Easing.Linear, 250);
                FadeStatusTextOut();
                ClearPool();
                ContentContainer.Height = Height;

                SnapToTop();

                AvailableItems?.Clear();
                ScrollbarBackground.Visible = false;
                FinishedLoading = false;

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
            FinishedLoading = false;
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
                StatusText.Text = "There is currently no map selected!".ToUpper();
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
                StatusText.Text = "You must be online to access this leaderboard!".ToUpper();
                FadeStatusTextIn();
                return;
            }

            // User isn't a donator
            if (RequiresOnline() && RequiresDonator() && !isDonator)
            {
                StatusText.Text = "You must be a donator to access this leaderboard!".ToUpper();
                FadeStatusTextIn();
                return;
            }

            // No scores are available
            if (store.Scores.Count == 0)
            {
                // The user's map is not up-to-date, so prompt them of this.
                if (map.NeedsOnlineUpdate)
                {
                    StatusText.Text = "Your map is outdated. Please update it!".ToUpper();
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
                        StatusText.Text = "Scores on this map will be unranked!".ToUpper();
                    else if (ConfigManager.LeaderboardSection.Value != LeaderboardType.Local)
                    {
                        switch (map.RankedStatus)
                        {
                            case RankedStatus.NotSubmitted:
                                StatusText.Text = "This map is not submitted online!".ToUpper();
                                break;
                            case RankedStatus.Unranked:
                                StatusText.Text = "This map is not ranked!".ToUpper();
                                break;
                            case RankedStatus.Ranked:
                                StatusText.Text = "No scores available. Be the first!".ToUpper();
                                break;
                            case RankedStatus.DanCourse:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                        StatusText.Text = "No scores available. Be the first!".ToUpper();
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
                                          || ConfigManager.LeaderboardSection.Value == LeaderboardType.All 
                                          || ConfigManager.LeaderboardSection.Value == LeaderboardType.Clan);

        /// <summary>
        ///     Creates <see cref="StatusText"/>
        /// </summary>
        private void CreateStatusText()
        {
            StatusText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
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
            StatusText.ClearAnimations();
            StatusText.FadeTo(1, Easing.Linear, 250);
        }

        private void FadeStatusTextOut()
        {
            StatusText.ClearAnimations();
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
                ClearPool();
                SnapToTop();

                AvailableItems = e.Result.Scores;

                if (AvailableItems == null)
                    return;

                var MAX_SHOWN_ITEMS = Height / DrawableLeaderboardScore.ScoreHeight;

                // We don't have enough scores in the leaderboard, so fill it with empty scores, so the leaderboard
                // still preserves the table look
                if (AvailableItems.Count < MAX_SHOWN_ITEMS && AvailableItems.Count != 0)
                {
                    var count = MAX_SHOWN_ITEMS - AvailableItems.Count;

                    for (var i = 0; i < count; i++)
                        AvailableItems.Add(new Score { IsEmptyScore = true});
                }

                try
                {
                    CreatePool(false);
                }
                catch (Exception)
                {
                    // ignored
                }

                FinishedLoading = true;
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

        private void ClearPool()
        {
            if (Pool is null)
                return;

            try
            {
                foreach (var x in new List<Drawable>(Pool))
                    x.Destroy();

                Pool.Clear();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
