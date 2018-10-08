using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Quaver.Helpers;
using Quaver.Screens.Results.UI;
using Quaver.Screens.Results.UI.ScoreResults;
using Quaver.Screens.Results.UI.Statistics;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Screens.Results
{
    public class ResultsScreenView : ScreenView
    {
        /// <summary>
        ///     The sprite that displays the map's information.
        /// </summary>
        private MapInformation MapInformation { get; set; }

        /// <summary>
        ///     The sprite that displays the score results information.
        /// </summary>
        private ScoreResultsTable ScoreResults { get; set; }

        /// <summary>
        ///     Information regarding online results.
        /// </summary>
        private ScoreResultsTable OnlineResults { get; set; }

        /// <summary>
        ///     Sprite that gives the breakdown of judgements.
        /// </summary>
        public JudgementBreakdown JudgementBreakdown { get; private set; }

        /// <summary>
        ///     The container for the buttons.
        /// </summary>
        public ResultsButtonContainer ButtonContainer { get; private set; }

        /// <summary>
        ///     The sprite that contains all of the score data.
        /// </summary>
        private ResultsScoreStatistics ScoreStatistics { get; set; }

        /// <summary>
        ///     Screen transitioner for fade effects.
        /// </summary>
        private Sprite ScreenTransitioner { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultsScreenView(Screen screen) : base(screen)
        {
            // BackgroundManager.Background.Strength = 8;
            BackgroundManager.Background.Dim = 60;

            CreateMapInformation();
            CreateScoreResults();
            CreateOnlineResultsInfo();
            CreateJudgementBreakdown();
            CreateButtonContainer();
            CreateScoreStatistics();

            // Create transitioner last, so any fade animations draw on top.
            CreateScreenTransitioner();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            BackgroundManager.Draw(gameTime);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///     Creates the map information sprite.
        /// </summary>
        private void CreateMapInformation() => MapInformation = new MapInformation((ResultsScreen) Screen)
        {
            Parent = Container,
            Transformations =
            {
                new Transformation(TransformationProperty.Y, Easing.EaseOutBounce, -125, 60, 500)
            }
        };

        /// <summary>
        ///     Creates the score results sprite.
        /// </summary>
        private void CreateScoreResults()
        {
            var resultsScreen = (ResultsScreen) Screen;

            var columns = new List<ScoreResultsInfoItem>()
            {
                new ScoreResultsInfoItem("Mods", ModHelper.GetModsString(resultsScreen.ScoreProcessor.Mods)),
                new ScoreResultsInfoItem("Score", resultsScreen.ScoreProcessor.Score.ToString("N0")),
                new ScoreResultsInfoItem("Accuracy", StringHelper.AccuracyToString(resultsScreen.ScoreProcessor.Accuracy)),
                new ScoreResultsInfoItem("Max Combo", resultsScreen.ScoreProcessor.MaxCombo.ToString("N0") + "x")
            };

            ScoreResults = new ScoreResultsTable(resultsScreen, "Score Results", columns)
            {
                Parent = Container,
                Y = 60 + MapInformation.Height + 20,
                Alignment = Alignment.TopCenter,
                X = -WindowManager.Width,
            };

            var transformation = new Transformation(TransformationProperty.X, Easing.EaseInOutElastic, ScoreResults.X, -ScoreResults.Width / 2f - 10, 800);
            ScoreResults.Transformations.Add(transformation);
        }

        /// <summary>
        ///     Creates the table for online results.
        /// </summary>
        private void CreateOnlineResultsInfo()
        {
            var resultsScreen = (ResultsScreen)Screen;

            var columns = new List<ScoreResultsInfoItem>()
            {
                new ScoreResultsInfoItem("Score Rating"),
                new ScoreResultsInfoItem("Map Rank"),
                new ScoreResultsInfoItem("Ranks Gained")
            };

            OnlineResults = new ScoreResultsTable(resultsScreen, "Online Results", columns)
            {
                Parent = Container,
                Y = 60 + MapInformation.Height + 20,
                Alignment = Alignment.TopCenter,
                X = WindowManager.Width
            };

            var transformation = new Transformation(TransformationProperty.X, Easing.EaseInOutElastic, OnlineResults.X, OnlineResults.Width / 2f + 10, 800);
            OnlineResults.Transformations.Add(transformation);
        }

        /// <summary>
        ///     Creates the judgement breakdown sprite.
        /// </summary>
        private void CreateJudgementBreakdown()
        {
            var resultsScreen = (ResultsScreen)Screen;

            JudgementBreakdown = new JudgementBreakdown(resultsScreen.ScoreProcessor)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = ScoreResults.Y + ScoreResults.Height + 20,
                X = -WindowManager.Width
            };

            var transformation = new Transformation(TransformationProperty.X, Easing.EaseOutElastic, JudgementBreakdown.X,
                                                                                    -JudgementBreakdown.Width / 2f - 10, 1200);

            JudgementBreakdown.Transformations.Add(transformation);
        }

        /// <summary>
        ///     Creates the button container sprite.
        /// </summary>
        private void CreateButtonContainer()
        {
            var resultsScreen = (ResultsScreen)Screen;

            ButtonContainer = new ResultsButtonContainer(resultsScreen)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = WindowManager.Height,
                X = 0
            };

            var transformation = new Transformation(TransformationProperty.Y, Easing.EaseOutBounce, ButtonContainer.Y,
                                                        JudgementBreakdown.Y + JudgementBreakdown.Height + 20, 1200);

            ButtonContainer.Transformations.Add(transformation);
        }

        /// <summary>
        ///     Creates the score data container sprite.
        /// </summary>
        private void CreateScoreStatistics()
        {
            var stats = new List<StatisticContainer>();

            var resultsScreen = (ResultsScreen)Screen;

            // Only add data if we're coming originally from gameplay.
            if (resultsScreen.ResultsScreenType == ResultsScreenType.FromGameplay)
            {
                stats.Add(new StatisticContainer("Hit Offset", new HitOffsetGraph(resultsScreen.ScoreProcessor)));
                stats.Add(new StatisticContainer("Health", new HealthGraph(resultsScreen.ScoreProcessor)));
            }

            // Create the score statistics sprite along with all of the stats screens we have.
            ScoreStatistics = new ResultsScoreStatistics(resultsScreen, JudgementBreakdown, stats)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = JudgementBreakdown.Y,
                X = WindowManager.Width
            };

            var transformation = new Transformation(TransformationProperty.X, Easing.EaseOutElastic, ScoreStatistics.X,
                                                    ScoreStatistics.Width / 2f + 10, 1200);

            ScoreStatistics.Transformations.Add(transformation);
        }

        /// <summary>
        ///     Creates the screen transitioner sprite.
        /// </summary>
        private void CreateScreenTransitioner() => ScreenTransitioner = new Sprite()
        {
            Parent = Container,
            Alignment = Alignment.TopLeft,
            Width = WindowManager.Width,
            Height = WindowManager.Height,
            Tint = Color.Black,
            Alpha = 1,
            Transformations =
            {
                new Transformation(TransformationProperty.Alpha, Easing.Linear, 1, 0, 300)
            }
        };

        /// <summary>
        ///     Handles exit screen animations.
        /// </summary>
        public void PerformExitAnimations()
        {
            ScreenTransitioner.Transformations.Clear();
            ScreenTransitioner.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 500));

            MapInformation.Transformations.Clear();
            MapInformation.Transformations.Add(new Transformation(TransformationProperty.Y, Easing.EaseInOutQuad, MapInformation.Y,
                                                                        -WindowManager.Height, 300));

            ScoreResults.Transformations.Clear();
            ScoreResults.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseInOutQuad, ScoreResults.X,
                                                                        -WindowManager.Width, 300));

            OnlineResults.Transformations.Clear();
            OnlineResults.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseInOutQuad, OnlineResults.X, WindowManager.Width, 300));

            JudgementBreakdown.Transformations.Clear();
            JudgementBreakdown.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseInOutQuad,
                                                                        JudgementBreakdown.X, -WindowManager.Width, 300));

            ScoreStatistics.Transformations.Clear();
            ScoreStatistics.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseInOutQuad,
                                                                        ScoreStatistics.X, WindowManager.Width, 300));

            ButtonContainer.Transformations.Clear();
            ButtonContainer.Transformations.Add(new Transformation(TransformationProperty.Y, Easing.EaseInOutQuad, ButtonContainer.Y,
                                                                        WindowManager.Height, 300));
        }
    }
}
