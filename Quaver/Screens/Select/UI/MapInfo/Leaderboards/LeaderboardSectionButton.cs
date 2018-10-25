using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.Select.UI.MapInfo.Leaderboards
{
    public class LeaderboardSectionButton : TextButton
    {
        /// <summary>
        ///     Reference to the leaderboard section itself.
        /// </summary>
        public LeaderboardSection Section { get; }

        /// <summary>
        ///     The line that shows if the button is highlighted.
        /// </summary>
        private Sprite HighlightLine { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="section"></param>
        /// <param name="text"></param>
        public LeaderboardSectionButton(LeaderboardSection section, string text)
            : base(UserInterface.BlankBox, BitmapFonts.Exo2Regular, text, 16)
        {
            Section = section;

            Alpha = 0;
            Size = Text.Size;
            Y = 2;

            Height = Section.Leaderboard.DividerLine.Y - Section.Leaderboard.DividerLine.Height;

            HighlightLine = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Size = new ScalableVector2(0, 1),
                Y = Height - 1
            };

            Text.X = 1;

            Clicked += (o, e) =>
            {
                ConfigManager.SelectLeaderboardSection.Value = Section.SectionType;

                switch (ConfigManager.SelectLeaderboardSection.Value)
                {
                    case LeaderboardSectionType.DifficultySelection:
                        var selectScreen = (SelectScreen) Section.Leaderboard.View.Screen;
                        Section.Leaderboard.UpdateLeaderboard(selectScreen.AvailableMapsets[Section.Leaderboard.View.MapsetContainer.SelectedMapsetIndex]);
                        break;
                    case LeaderboardSectionType.Local:
                    case LeaderboardSectionType.Global:
                        Section.Leaderboard.UpdateLeaderboard();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (IsHovered || Section.SectionType == ConfigManager.SelectLeaderboardSection.Value)
            {
                HighlightLine.Width = MathHelper.Lerp(HighlightLine.Width, Width, (float) Math.Min(dt / 60, 1));
                Text.FadeToColor(Color.White, dt, 60);
            }
            else
            {
                HighlightLine.Width = MathHelper.Lerp(HighlightLine.Width, 0, (float) Math.Min(dt / 60, 1));
                Text.FadeToColor(Colors.MainAccent, dt, 60);
            }

            base.Update(gameTime);
        }
    }
}