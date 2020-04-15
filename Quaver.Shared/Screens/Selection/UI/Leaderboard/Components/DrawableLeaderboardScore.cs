using Quaver.Shared.Assets;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Components
{
    public class DrawableLeaderboardScore : PoolableSprite<Score>
    {
        public static int ScoreHeight => 66;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = ScoreHeight;

        /// <summary>
        ///     If the score is a personal best score
        /// </summary>
        public bool IsPersonalBest { get; }

        /// <summary>
        ///     The child score container
        /// </summary>
        public DrawableLeaderboardScoreContainer ChildContainer { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="isPersonalBest"></param>
        public DrawableLeaderboardScore(PoolableScrollContainer<Score> container, Score item, int index, bool isPersonalBest) : base(container, item, index)
        {
            IsPersonalBest = isPersonalBest;
            Size = new ScalableVector2(560, 66);
            Alpha = 0;

            ChildContainer = new DrawableLeaderboardScoreContainer(this)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true
            };

            ModManager.ModsChanged += OnModsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ModManager.ModsChanged -= OnModsChanged;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(Score item, int index)
        {
            Item = item;
            Index = index;

            ChildContainer.UpdateContent(this);
        }

        private void OnModsChanged(object sender, ModsChangedEventArgs e) => UpdateContent(Item, Index);
    }
}