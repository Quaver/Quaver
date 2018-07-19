using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UI;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States.Results.UI.Breakdown;
using Quaver.States.Results.UI.Buttons;
using Quaver.States.Results.UI.Data;
using Quaver.States.Results.UI.ScoreResults;

namespace Quaver.States.Results.UI
{
    internal class ResultsInterface : IGameStateComponent
    {
        /// <summary>
        ///     Reference to the parent screen.
        /// </summary>
        private ResultsScreen Screen { get; }

        /// <summary>
        ///     Sprite container.
        /// </summary>
        private Container Container { get; set; }

        /// <summary>
        ///     The background of the map.
        /// </summary>
        private Background Background { get; set; }

        /// <summary>
        ///     Information about the map in the result sscreen.
        /// </summary>
        private MapInformation MapInformation { get; set; }

        /// <summary>
        ///     Information regarding the score's results.
        /// </summary>
        private ScoreResultsTable ScoreResultsTable { get; set; }

        /// <summary>
        ///     Information regarding online results.
        /// </summary>
        private ScoreResultsTable OnlineResultsTable { get; set; }

        /// <summary>
        ///     Sprite that gives the breakdown of judgements.
        /// </summary>
        internal JudgementBreakdown JudgementBreakdown { get; private set; }

        /// <summary>
        ///     The container for the buttons.
        /// </summary>
        internal ResultsButtonContainer ButtonContainer { get; private set; }

        /// <summary>
        ///     The sprite that contains all of the score data.
        /// </summary>
        private ResultsScoreStatistics ScoreStatistics { get; set; }

        /// <summary>
        ///     Screen transitioner for fade effects.
        /// </summary>
        private Sprite ScreenTransitioner { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        internal ResultsInterface(ResultsScreen screen) => Screen = screen;

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="state"></param>
        public void Initialize(IGameState state)
        {
            Container = new Container();

            CreateBackground();
            CreateMapInformation();
            CreateScoreResultsInfo();
            CreateOnlineResultsInfo();
            CreateJudgementBreakdown();
            CreateButtonContainer();
            CreateScoreStatistics();

            // Create transitioner last, so any fade animations draw on top.
            CreateScreenTransitioner();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public void UnloadContent() => Container.Destroy();

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="dt"></param>
        public void Update(double dt)
        {
            PerformPanelAnimations(dt);
            PerformScreenTransitions(dt);

            Container.Update(dt);
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            Container.Draw();
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Creates the map background.
        /// </summary>
        private void CreateBackground() => Background = new Background(GameBase.QuaverUserInterface.MenuBackground, 0)
        {
            Parent = Container
        };

        /// <summary>
        ///     Creates the map information sprite.
        /// </summary>
        private void CreateMapInformation() => MapInformation = new MapInformation(Screen)
        {
            Parent = Container,
            PosY = -GameBase.WindowRectangle.Height,
            PosX = 0
        };

        /// <summary>
        ///     Creates the score results sprite.
        /// </summary>
        private void CreateScoreResultsInfo() => ScoreResultsTable = new ScoreResultsTable(Screen, "Score Results",
            new List<ScoreResultsInfoItem>()
        {
            new ScoreResultsInfoItem("Mods", ModHelper.GetModsString(Screen.ScoreProcessor.Mods)),
            new ScoreResultsInfoItem("Score", Screen.ScoreProcessor.Score.ToString("N0")),
            new ScoreResultsInfoItem("Accuracy", StringHelper.AccuracyToString(Screen.ScoreProcessor.Accuracy)),
            new ScoreResultsInfoItem("Max Combo", Screen.ScoreProcessor.MaxCombo.ToString("N0") + "x")
        })
        {
            Parent = Container,
            PosY = 60 + MapInformation.SizeY + 20,
            Alignment = Alignment.TopCenter,
            PosX = -GameBase.WindowRectangle.Width
        };

        /// <summary>
        ///     Creates the table for online results.
        /// </summary>
        private void CreateOnlineResultsInfo() => OnlineResultsTable = new ScoreResultsTable(Screen, "Online Results",
            new List<ScoreResultsInfoItem>()
        {
            new ScoreResultsInfoItem("Score Rating"),
            new ScoreResultsInfoItem("Map Rank"),
            new ScoreResultsInfoItem("Ranks Gained")
        })
        {
            Parent = Container,
            PosY = 60 + MapInformation.SizeY + 20,
            Alignment = Alignment.TopCenter,
            PosX = GameBase.WindowRectangle.Width
        };

        /// <summary>
        ///     Creates the judgement breakdown sprite.
        /// </summary>
        private void CreateJudgementBreakdown() => JudgementBreakdown = new JudgementBreakdown(Screen.ScoreProcessor)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            PosY = ScoreResultsTable.PosY + ScoreResultsTable.SizeY + 20,
            PosX = -GameBase.WindowRectangle.Width
        };

        /// <summary>
        ///     Creates the button container sprite.
        /// </summary>
        private void CreateButtonContainer() => ButtonContainer = new ResultsButtonContainer(Screen)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            PosY = GameBase.WindowRectangle.Height,
            PosX = 0
        };

        /// <summary>
        ///     Creates the score data container sprite.
        /// </summary>
        private void CreateScoreStatistics()
        {
            var stats = new List<StatisticContainer>();

            // Only add data if we're coming originally from gameplay.
            if (Screen.Type == ResultsScreenType.FromGameplay)
            {
                stats.Add(new StatisticContainer("Hit Offset", new HitOffsetGraph(Screen.ScoreProcessor)));
                stats.Add(new StatisticContainer("Debug", new Sprite() { Image = GameBase.QuaverUserInterface.MenuBackground, Size = new UDim2D(500, 200), Alignment = Alignment.TopCenter}));
                stats.Add(new StatisticContainer("Debug2", new Sprite() { Image = FontAwesome.Archive, Size = new UDim2D(500, 200), Alignment = Alignment.TopCenter}));
            }

            // Create the score statistics sprite along with all of the stats screens we have.
            ScoreStatistics = new ResultsScoreStatistics(Screen, stats)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                PosY = JudgementBreakdown.PosY,
                PosX = GameBase.WindowRectangle.Width
            };
        }

        /// <summary>
        ///     Creates the screen transitioner sprite.
        /// </summary>
        private void CreateScreenTransitioner() => ScreenTransitioner = new Sprite()
        {
            Parent = Container,
            Alignment = Alignment.TopLeft,
            SizeX = GameBase.WindowRectangle.Width,
            SizeY = GameBase.WindowRectangle.Height,
            Tint = Color.Black,
            Alpha = 1
        };

        /// <summary>
        ///     Performs the animations for the panels entering and exiting the screen.
        /// </summary>
        /// <param name="dt"></param>
        private void PerformPanelAnimations(double dt)
        {
            var animSpeed = Math.Min(dt / 120, 1);

            if (!Screen.IsExiting)
            {
                MapInformation.PosY = GraphicsHelper.Tween(60, MapInformation.PosY, animSpeed);

                if (!(MapInformation.PosY >= 0))
                    return;

                ScoreResultsTable.PosX = GraphicsHelper.Tween(-ScoreResultsTable.SizeX / 2f - 10, ScoreResultsTable.PosX, animSpeed);
                OnlineResultsTable.PosX = GraphicsHelper.Tween(OnlineResultsTable.SizeX / 2f + 10, OnlineResultsTable.PosX, animSpeed);
                JudgementBreakdown.PosX = GraphicsHelper.Tween(-JudgementBreakdown.SizeX / 2f - 10, JudgementBreakdown.PosX, animSpeed);
                ScoreStatistics.PosX = GraphicsHelper.Tween(ScoreStatistics.SizeX / 2f + 10, ScoreStatistics.PosX, animSpeed);
                ButtonContainer.PosY = GraphicsHelper.Tween(JudgementBreakdown.PosY + JudgementBreakdown.SizeY + 20, ButtonContainer.PosY, animSpeed);
            }
            else
            {
                MapInformation.PosY = GraphicsHelper.Tween(-GameBase.WindowRectangle.Height, MapInformation.PosY, animSpeed);
                ScoreResultsTable.PosX = GraphicsHelper.Tween(-GameBase.WindowRectangle.Width, ScoreResultsTable.PosX, animSpeed);
                OnlineResultsTable.PosX = GraphicsHelper.Tween(GameBase.WindowRectangle.Width, OnlineResultsTable.PosX, animSpeed);
                JudgementBreakdown.PosX = GraphicsHelper.Tween(-GameBase.WindowRectangle.Width, JudgementBreakdown.PosX, animSpeed);
                ScoreStatistics.PosX = GraphicsHelper.Tween(GameBase.WindowRectangle.Width, ScoreStatistics.PosX, animSpeed);
                ButtonContainer.PosY = GraphicsHelper.Tween(GameBase.WindowRectangle.Height, ButtonContainer.PosY, animSpeed);
            }
        }

        /// <summary>
        ///     Performs animations for the screen transitioner.
        /// </summary>
        /// <param name="dt"></param>
        private void PerformScreenTransitions(double dt)
        {
            if (Screen.IsExiting)
                ScreenTransitioner.FadeIn(dt, 120);
            else
                ScreenTransitioner.FadeOut(dt, 480);
        }
    }
}