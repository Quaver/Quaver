using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Resources;
using Quaver.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.Results.UI.Statistics
{
    public class ResultsScoreStatistics : HeaderedContainer
    {
        /// <summary>
        ///     Reference to the score processor that was played.
        /// </summary>
        private ResultsScreen Screen { get; }

        /// <summary>
        ///     The list of stat containers that we currently have.
        /// </summary>
        private List<StatisticContainer> StatsContainers { get; }

        /// <summary>
        ///     The currently selected container.
        /// </summary>
        private int SelectedContainer { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="breakdown"></param>
        /// <param name="stats"></param>
        public ResultsScoreStatistics(ResultsScreen screen, JudgementBreakdown breakdown, List<StatisticContainer> stats = null)
                                    : base(new Vector2(breakdown.Width, breakdown.Height),
                                                "Statistics", BitmapFonts.Exo2Regular, 22, Alignment.MidCenter, 50, Colors.DarkGray)
        {
            Screen = screen;
            X = Width / 2f + 10;
            StatsContainers = stats;

            Content = CreateContent();
            Content.Parent = this;
            Content.Y = 50;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected sealed override Sprite CreateContent()
        {
            // Create the sprite that acts as a parent for all of our content. (Graphs, Tab Buttons).
            var content = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(ContentSize.X, ContentSize.Y),
                Tint = Color.Black,
                Alpha = 0.45f
            };

            // If there aren't any stats to display, then display a message to the user.
            if (StatsContainers.Count == 0)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new SpriteText(BitmapFonts.Exo2Regular, "No statistics available. Play the map or watch the replay to retrieve them.", 14)
                {
                    Parent = content,
                    Alignment = Alignment.MidCenter,
                    TextAlignment = Alignment.MidCenter,
                };

                return content;
            }

            // Create all the tabs for each of our containers.
            CreateTabs(content);

            return content;
        }

        /// <summary>
        ///     Creates the tabs for each individual container we have.
        /// </summary>
        /// <param name="content"></param>
        private void CreateTabs(Drawable content)
        {
            // Create the container that houses all of the buttons.
            var buttonContainer = new Sprite
            {
                Parent = content,
                Alpha = 0f,
                Size = new ScalableVector2(content.Width - 75, 35),
                Alignment = Alignment.BotCenter,
                Tint = Colors.DarkGray,
                Y = -5,
            };

            // Go through each container and c
            for (var i = 0; i < StatsContainers.Count; i++)
            {
                // Set the parent of the content to the actual content, so that we can display it.
                // We want to give full control here over sprite creation, so we don't change it at all.
                StatsContainers[i].Content.Parent = content;
                StatsContainers[i].Content.SetChildrenAlpha = true;

                // Create Tab Button
                StatsContainers[i].Button = new TextButton(UserInterface.BlankBox, BitmapFonts.Exo2Regular,
                                                            StatsContainers[i].Name.ToUpper(), 16)
                {
                    Parent = buttonContainer,
                    Alignment = Alignment.MidLeft,
                    Size = new ScalableVector2(150, buttonContainer.Height),
                    Text = {Tint = Color.Black}
                };

                // Make a shorter reference to the tab button for easy access.
                var btn = StatsContainers[i].Button;

                // The first button should be the one that is selected.
                if (i == 0)
                {
                    btn.Tint = Colors.SecondaryAccent;
                    btn.Text.Tint = Color.Black;
                }
                // Any other buttons need to be placed as inactive.
                else
                {
                    btn.Tint = Color.Black;
                    btn.Alpha = 0.50f;
                    btn.Text.Tint = Color.White;

                    // Set button position
                    btn.X = btn.Width * i + i * 10;

                    // Make sure that the the alpha of inactive ones are invisible.
                    StatsContainers[i].Content.Alpha = 0;
                }

                // When the button is clicked, we'll want to change the selected container.
                btn.Clicked += (sender, args) =>
                {
                    if ((TextButton)sender == StatsContainers[SelectedContainer].Button)
                        return;

                    for (var j = 0; j < StatsContainers.Count; j++)
                    {
                        if (StatsContainers[j].Button != (TextButton)sender)
                            continue;

                        SelectedContainer = j;
                        break;
                    }
                };
            }
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            for (var i = 0; i < StatsContainers.Count; i++)
            {
                var container = StatsContainers[i];

                if (i != SelectedContainer)
                {
                    // Make sure button is black and unfocused.
                    container.Button.FadeToColor(container.Button.IsHovered ? Colors.SecondaryAccentInactive : Color.Black, dt, 60);

                    container.Button.Alpha = MathHelper.Lerp(container.Button.Alpha, 0.5f, (float) Math.Min(dt / 60, 1));
                    container.Button.Text.Tint = Color.White;

                    // Make sure that the container content itself is hidden
                    container.Content.Alpha = MathHelper.Lerp(container.Content.Alpha, 0, (float)Math.Min(dt / 240, 1));
                }
                else
                {
                    // Make sure button is yellow and focused.
                    container.Button.Alpha = MathHelper.Lerp(container.Button.Alpha, 1f, (float)Math.Min(dt / 60, 1));
                    container.Button.FadeToColor(Colors.SecondaryAccent, dt, 60);
                    container.Button.Text.Tint = Color.Black;

                    // Make sure that the contaier content itself is visible
                    container.Content.Alpha = MathHelper.Lerp(container.Content.Alpha, 1, (float)Math.Min(dt / 240, 1));
                }
            }

            base.Update(gameTime);
        }
    }
}
