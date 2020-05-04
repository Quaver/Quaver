using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs
{
    public class ResultsOverviewGraphContainer : Sprite
    {
        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        /// </summary>
        private Container FooterContainer { get; set; }

        /// <summary>
        /// </summary>
        private Container ContentContainer { get; set; }

        /// <summary>
        /// </summary>
        private Container LeftContainer { get; set; }

        /// <summary>
        /// </summary>
        private Container RightContainer { get; set; }

        /// <summary>
        /// </summary>
        private Sprite DividerLine { get; set; }

        /// <summary>
        /// </summary>
        private ResultsJudgementGraph JudgementGraph { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        public ResultsOverviewGraphContainer(Map map, Bindable<ScoreProcessor> processor)
        {
            Map = map;
            Processor = processor;

            Image = UserInterface.ResultsGraphContainerPanel;
            Size = new ScalableVector2(ResultsScreenView.CONTENT_WIDTH - ResultsTabContainer.PADDING_X, Image.Height);

            CreateFooterContainer();
            CreateContentContainer();
            CreateDividerLine();
            CreateLeftAndRightContainers();
            CreateJudgementGraph();
        }

        /// <summary>
        /// </summary>
        private void CreateFooterContainer() => FooterContainer = new Container
        {
            Parent = this,
            Alignment = Alignment.BotLeft,
            Size = new ScalableVector2(Width, 69),
        };

        /// <summary>
        /// </summary>
        private void CreateContentContainer() => ContentContainer = new Container
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            Size = new ScalableVector2(Width, Height - FooterContainer.Height),
        };

        /// <summary>
        /// </summary>
        private void CreateDividerLine() => DividerLine = new Sprite
        {
            Parent = ContentContainer,
            Alignment = Alignment.TopCenter,
            Size = new ScalableVector2(2, ContentContainer.Height),
            Tint = ColorHelper.HexToColor("#363636")
        };

        /// <summary>
        /// </summary>
        private void CreateLeftAndRightContainers()
        {
            LeftContainer = new Container
            {
                Parent = ContentContainer,
                Size = new ScalableVector2(ContentContainer.Width / 2 - DividerLine.Width, ContentContainer.Height)
            };

            RightContainer = new Container
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopRight,
                Size = LeftContainer.Size
            };
        }

        /// <summary>
        /// </summary>
        private void CreateJudgementGraph() => JudgementGraph = new ResultsJudgementGraph(Processor,
            new ScalableVector2(LeftContainer.Width - 50, LeftContainer.Height))
        {
            Parent = LeftContainer,
            Alignment = Alignment.MidCenter,
        };
    }
}