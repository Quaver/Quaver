using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
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
        internal ResultsScreen Screen { get; }

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
        private ResultsButtonContainer ButtonContainer { get; set; }

        /// <summary>
        ///     The sprite that contains all of the score data.
        /// </summary>
        private ResultsScoreStatistics ScoreStatistics { get; set; }

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
            CreateScoreData();
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
            MapInformation.PosX = GraphicsHelper.Tween(0, MapInformation.PosX, Math.Min(dt / 120, 1));

            if (Math.Abs(MapInformation.PosX) < 50)
                ScoreResultsTable.PosX = GraphicsHelper.Tween(-ScoreResultsTable.SizeX / 2f - 10, ScoreResultsTable.PosX, Math.Min(dt / 120, 1));

            if (Math.Abs(MapInformation.PosX) < 50)
                OnlineResultsTable.PosX = GraphicsHelper.Tween(OnlineResultsTable.SizeX / 2f + 10, OnlineResultsTable.PosX, Math.Min(dt / 120, 1));

            if (MapInformation.PosX > -25)
                JudgementBreakdown.PosX = GraphicsHelper.Tween(-JudgementBreakdown.SizeX / 2f - 10, JudgementBreakdown.PosX, Math.Min(dt / 120, 1));

            if (Math.Abs(JudgementBreakdown.PosX) > -10)
                ScoreStatistics.PosX = GraphicsHelper.Tween(ScoreStatistics.SizeX / 2f + 10, ScoreStatistics.PosX, Math.Min(dt / 120, 1));

            if (Math.Abs(MapInformation.PosX) < 20)
                ButtonContainer.PosX = GraphicsHelper.Tween(0, ButtonContainer.PosX, Math.Min(dt / 120, 1));

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
            PosY =  30,
            PosX = -GameBase.WindowRectangle.Width
        };

        /// <summary>
        ///     Creates the score results sprite.
        /// </summary>
        private void CreateScoreResultsInfo()
        {
            var mods = Screen.ScoreProcessor.Mods.ToString();

            ScoreResultsTable = new ScoreResultsTable(Screen, "Score Results", new List<ScoreResultsInfoItem>()
            {
                new ScoreResultsInfoItem("Mods", mods == "0" ? "None" : mods),
                new ScoreResultsInfoItem("Score", Screen.ScoreProcessor.Score.ToString("N0")),
                new ScoreResultsInfoItem("Accuracy", StringHelper.AccuracyToString(Screen.ScoreProcessor.Accuracy)),
                new ScoreResultsInfoItem("Max Combo", Screen.ScoreProcessor.MaxCombo.ToString("N0") + "x")
            })
            {
                Parent = Container,
                PosY = MapInformation.PosY + MapInformation.SizeY + 20,
                Alignment = Alignment.TopCenter,
                PosX = GameBase.WindowRectangle.Width
            };
        }

        /// <summary>
        ///     Creates the table for online results.
        /// </summary>
        private void CreateOnlineResultsInfo()
        {
            OnlineResultsTable = new ScoreResultsTable(Screen, "Online Results", new List<ScoreResultsInfoItem>()
            {
                new ScoreResultsInfoItem("Score Rating"),
                new ScoreResultsInfoItem("Map Rank"),
                new ScoreResultsInfoItem("Ranks Gained")
            })
            {
                Parent = Container,
                PosY = MapInformation.PosY + MapInformation.SizeY + 20,
                Alignment = Alignment.TopCenter,
                PosX = -GameBase.WindowRectangle.Width
            };
        }

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
            PosY = JudgementBreakdown.PosY + JudgementBreakdown.SizeY + 20,
            PosX = GameBase.WindowRectangle.Width
        };

        /// <summary>
        ///     Creates the score data container sprite.
        /// </summary>
        private void CreateScoreData() => ScoreStatistics = new ResultsScoreStatistics(Screen)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            PosY = JudgementBreakdown.PosY,
            PosX = GameBase.WindowRectangle.Width
        };
    }
}