using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Select.UI.Leaderboard;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Scheduling;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Components
{
    public class LeaderboardPersonalBestScore : Sprite, ILoadable, IFetchedScoreHandler
    {
        /// <summary>
        ///     The parent leaderboard container
        /// </summary>
        private LeaderboardContainer Container { get; }

        /// <summary>
        /// </summary>
        private LoadingWheel LoadingWheel { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus NoPersonalBestScore { get; set; }

        /// <summary>
        ///     The displayed score
        /// </summary>
        private DrawableLeaderboardScore Score { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        public LeaderboardPersonalBestScore(LeaderboardContainer container)
        {
            Container = container;
            Size = new ScalableVector2(Container.Width, 70);

            Image = SkinManager.Skin?.SongSelect?.PersonalBestPanel ?? UserInterface.PersonalBestScorePanel;

            CreateLoadingWheel();
            CreateNoPersonalBestScoreText();

            Container.FetchScoreTask.OnCompleted += OnScoresFetched;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container.FetchScoreTask.OnCompleted -= OnScoresFetched;
            base.Destroy();
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
                Size = new ScalableVector2(30, 30),
                Alpha = 0
            };
        }

        /// <summary>
        ///     Fades the wheel in to make it appear as if it is loading
        /// </summary>
        public void StartLoading()
        {
            LoadingWheel.Animations.RemoveAll(x => x.Properties != AnimationProperty.Rotation);
            LoadingWheel.FadeTo(1, Easing.Linear, 250);

            NoPersonalBestScore.ClearAnimations();
            NoPersonalBestScore.FadeTo(0, Easing.Linear, 200);

            Score?.Destroy();
        }

        /// <summary>
        ///     Fades the wheel out to 0.
        /// </summary>
        public void StopLoading()
        {
            LoadingWheel.Animations.RemoveAll(x => x.Properties != AnimationProperty.Rotation);
            LoadingWheel.FadeTo(0, Easing.Linear, 250);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="store"></param>
        public void HandleFetchedScores(Map map, FetchedScoreStore store)
        {
            if (store.PersonalBest != null)
                return;

            NoPersonalBestScore.ClearAnimations();
            NoPersonalBestScore.FadeTo(1, Easing.Linear, 200);
        }

        /// <summary>
        /// </summary>
        private void CreateNoPersonalBestScoreText()
        {
            NoPersonalBestScore = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "NO PERSONAL BEST SET", 20)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Alpha = 0,
                Tint = SkinManager.Skin?.SongSelect?.NoPersonalBestColor ?? Color.White
            };
        }

        private void OnScoresFetched(object sender, TaskCompleteEventArgs<Map, FetchedScoreStore> e)
        {
            ScheduleUpdate(() =>
            {
                Score?.Destroy();

                if (e.Result.PersonalBest == null)
                    return;

                Score = new DrawableLeaderboardScore(null, e.Result.PersonalBest, 1, true)
                {
                    Parent = this,
                    Alignment = Alignment.MidCenter
                };

                Score.UpdateContent(e.Result.PersonalBest, 1);
            });
        }
    }
}