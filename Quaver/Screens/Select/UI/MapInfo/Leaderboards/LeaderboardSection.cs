using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Screens.Select.UI.MapInfo.Leaderboards
{
    public abstract class LeaderboardSection
    {
        /// <summary>
        ///     The section (in enum form)
        /// </summary>
        public LeaderboardSectionType SectionType { get; }

        /// <summary>
        ///     Reference to the parent leaderboard.
        /// </summary>
        public Leaderboard Leaderboard { get; }

        /// <summary>
        ///     The button to select this leaderboard section.
        /// </summary>
        public LeaderboardSectionButton Button { get; }

        /// <summary>
        ///     Holds the content of the leaderboard section.
        /// </summary>
        public ScrollContainer ScrollContainer { get; }

        ///  <summary>
        ///
        ///  </summary>
        /// <param name="sectionType"></param>
        /// <param name="leaderboard"></param>
        ///  <param name="name"></param>
        public LeaderboardSection(LeaderboardSectionType sectionType, Leaderboard leaderboard, string name)
        {
            SectionType = sectionType;
            Leaderboard = leaderboard;
            Button = new LeaderboardSectionButton(this, name) {Parent = Leaderboard};

            var size = new ScalableVector2(Leaderboard.Width,
                Leaderboard.Height - Leaderboard.DividerLine.Y + Leaderboard.DividerLine.Height);

            ScrollContainer = new ScrollContainer(size, size)
            {
                Parent = Leaderboard,
                Size = size,
                Y = Leaderboard.DividerLine.Y + 5,
                Alpha = 0
            };
        }

        /// <summary>
        ///     Updates the section itself.
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            if (DialogManager.Dialogs.Count == 0)
                HandleInput();
        }

        /// <summary>
        ///     Handles input for the section.
        /// </summary>
        protected abstract void HandleInput();

        /// <summary>
        ///     Handles scrolling input for the container.
        /// </summary>
        protected void HandleScrollingInput()
        {
            var selectScreenView = (SelectScreenView) Leaderboard.Screen.View;

            // If the mouse is in the bounds of the scroll container, then
            // allow that to scroll, and turn off mapset scrolling.
            if (GraphicsHelper.RectangleContains(ScrollContainer.ScreenRectangle, MouseManager.CurrentState.Position) && DialogManager.Dialogs.Count == 0)
            {
                ScrollContainer.InputEnabled = true;
                selectScreenView.MapsetContainer.InputEnabled = false;
            }
            // If the mouse is outside the bounds of the scroll container, then turn mapset scrolling back on.
            else
            {
                ScrollContainer.InputEnabled = false;
                selectScreenView.MapsetContainer.InputEnabled = true;
            }
        }
    }
}