using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using osu_database_reader;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Screens.Select.UI.MapInfo.Leaderboards.Difficulty;
using Quaver.Screens.Select.UI.MapInfo.Leaderboards.Scores;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Screens;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Screens.Select.UI.MapInfo.Leaderboards
{
    public class Leaderboard : Sprite
    {
        /// <summary>
        ///     Reference to the select screen itself
        /// </summary>
        public SelectScreen Screen { get; }

        /// <summary>
        ///     Reference to the select screen's view.
        /// </summary>
        public SelectScreenView View { get; }

        /// <summary>
        ///    The line that divides the leaderboard section from the content.
        /// </summary>
        public Sprite DividerLine { get; private set; }

        /// <summary>
        ///     The list of leaderboard sections.
        /// </summary>
        public Dictionary<LeaderboardSectionType, LeaderboardSection> Sections { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="view"></param>
        /// <param name="container"></param>
        public Leaderboard(SelectScreen screen, SelectScreenView view, MapInfoContainer container)
        {
            Screen = screen;
            View = view;

            var banner = container.Banner;
            Size = new ScalableVector2(banner.Width - 2, 372);
            Tint = Color.Black;
            Alpha = 0;

            Y = banner.Y + banner.Height;
            X = banner.X;
            Alignment = Alignment.TopCenter;

            CreateDividerLine();
            CreateSections();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Sections[ConfigManager.SelectLeaderboardSection.Value].Update(gameTime);
            AnimateSections(gameTime);
            HandleInput();

            base.Update(gameTime);
        }

        /// <summary>
        ///     Animates each section and makes sure they're positioned correctly.
        /// </summary>
        /// <param name="gameTime"></param>
        private void AnimateSections(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Move active section in and inactive sections out.
            try
            {
                foreach (var section in Sections)
                {
                    section.Value.ScrollContainer.X = section.Key != ConfigManager.SelectLeaderboardSection.Value
                        ? MathHelper.Lerp(section.Value.ScrollContainer.X, -section.Value.ScrollContainer.Width - 100, (float) Math.Min(dt / 60, 1))
                        : MathHelper.Lerp(section.Value.ScrollContainer.X, 0, (float) Math.Min(dt / 60, 1));
                }
            }
            catch (InvalidOperationException)
            {
                // ignored
            }
        }

        /// <summary>
        ///     Handles general input for the entire leaderboard
        /// </summary>
        private void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0)
                return;

            // Add quick tab switching.
            if (KeyboardManager.IsUniqueKeyPress(Keys.Tab))
            {
                if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                    SwitchTabs(Direction.Backward);
                else
                    SwitchTabs(Direction.Forward);
            }
        }

        /// <summary>
        ///     The divider line for the leaderboard container.
        /// </summary>
        private void CreateDividerLine() => DividerLine = new Sprite()
        {
            Parent = this,
            Size = new ScalableVector2(Width, 1),
            Y = 25,
            Alpha = 0.25f
        };

        /// <summary>
        ///     Creates the different leaderboard sections.
        /// </summary>
        private void CreateSections()
        {
            Sections = new Dictionary<LeaderboardSectionType, LeaderboardSection>
            {
                [LeaderboardSectionType.DifficultySelection] = new LeaderboardSectionDifficulty(this),
                [LeaderboardSectionType.Local] = new LeaderboardSectionLocal(this),
                [LeaderboardSectionType.Global] = new LeaderboardSectionGlobal(this)
            };

            for (var i = 0; i < Sections.Count; i++)
            {
                var rankingSection = (LeaderboardSectionType) i;
                var section = Sections[rankingSection];

                if (i > 0)
                {
                    var previousSection = Sections[(LeaderboardSectionType) i - 1];
                    section.Button.X = previousSection.Button.X + previousSection.Button.Width + 10;
                }
            }
        }

        /// <summary>
        ///     Updates the leaderboard section.
        /// </summary>
        public void UpdateLeaderboard()
        {
            switch (ConfigManager.SelectLeaderboardSection.Value)
            {
                case LeaderboardSectionType.Local:
                    var localLeaderboard = (LeaderboardSectionLocal) Sections[LeaderboardSectionType.Local];
                    localLeaderboard.FetchAndUpdateLeaderboards(null);
                    break;
                // Ignore.
                case LeaderboardSectionType.Global:
                    var globalLeaderboard = (LeaderboardSectionGlobal) Sections[LeaderboardSectionType.Global];

                    // TODO: REPLACE WITH ONLINE SCORES
                    globalLeaderboard.FetchAndUpdateLeaderboards(null);
                    break;
            }
        }

        /// <summary>
        ///     Updates the leaderboard section with a given mapset. (For LeaderboardSectionDifficulty)
        /// </summary>
        /// <param name="set"></param>
        public void UpdateLeaderboard(Mapset set)
        {
            var difficultySection = (LeaderboardSectionDifficulty) Sections[LeaderboardSectionType.DifficultySelection];
            difficultySection.UpdateAsscoiatedMapset(set);
        }

        /// <summary>
        ///     Switches to another leaderboard tab.
        /// </summary>
        private void SwitchTabs(Direction direction)
        {
            var index = (int) ConfigManager.SelectLeaderboardSection.Value;

            switch (direction)
            {
                case Direction.Forward:
                    if (index + 1 < Sections.Count)
                        ConfigManager.SelectLeaderboardSection.Value = (LeaderboardSectionType) index + 1;
                    else
                        ConfigManager.SelectLeaderboardSection.Value = Sections.First().Key;
                    break;
                case Direction.Backward:
                    if (index - 1 >= 0)
                        ConfigManager.SelectLeaderboardSection.Value = (LeaderboardSectionType) index - 1;
                    else
                        ConfigManager.SelectLeaderboardSection.Value = Sections.Last().Key;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            // Update section.
            switch (ConfigManager.SelectLeaderboardSection.Value)
            {
                case LeaderboardSectionType.DifficultySelection:
                    UpdateLeaderboard(Screen.AvailableMapsets[View.MapsetContainer.SelectedMapsetIndex]);
                    break;
                case LeaderboardSectionType.Local:
                case LeaderboardSectionType.Global:
                    UpdateLeaderboard();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}