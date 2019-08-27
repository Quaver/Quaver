using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Select.UI.Leaderboard;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

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
        /// </summary>
        /// <param name="container"></param>
        public LeaderboardScoresContainer(LeaderboardContainer container) : base(new List<Score>(), 10, 0,
            new ScalableVector2(container.Width, 664), new ScalableVector2(container.Width, 664))
        {
            Container = container;
            Image = UserInterface.LeaderboardScoresPanel;

            CreateScrollbar();
            CreateLoadingWheel();
            CreateStatusText();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ScrollbarBackground.Visible = AvailableItems.Count != 0;
            base.Update(gameTime);
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

        protected override PoolableSprite<Score> CreateObject(Score item, int index) => null;

        /// <summary>
        ///     Fades the wheel in to make it appear as if it is loading
        /// </summary>
        public void StartLoading()
        {
            LoadingWheel.Animations.RemoveAll(x => x.Properties != AnimationProperty.Rotation);
            LoadingWheel.FadeTo(1, Easing.Linear, 250);
            FadeStatusTextOut();
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
                // TODO: Button to update the map
                if (map.NeedsOnlineUpdate)
                    StatusText.Text = "Your map is outdated. Please update it!".ToUpper();
                else
                {
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

            // Show scores
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
                case LeaderboardType.Global:
                case LeaderboardType.Mods:
                case LeaderboardType.Country:
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     If the leaderboard requires donator privileges
        /// </summary>
        /// <returns></returns>
        private bool RequiresDonator() => RequiresOnline() && ConfigManager.LeaderboardSection.Value == LeaderboardType.Country;

        /// <summary>
        ///     Creates <see cref="StatusText"/>
        /// </summary>
        private void CreateStatusText()
        {
            StatusText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
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
    }
}