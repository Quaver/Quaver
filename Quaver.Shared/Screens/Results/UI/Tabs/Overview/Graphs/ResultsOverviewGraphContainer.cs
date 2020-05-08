using System.Diagnostics;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Deviance;
using Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Footer;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

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
        private Bindable<bool> IsSubmittingScore { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreSubmissionResponse> ScoreSubmissionStats { get; }

        /// <summary>
        /// </summary>
        private Container FooterContainer { get; set; }

        /// <summary>
        /// </summary>
        private ResultsOverviewFooter Footer { get; set; }

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
        private GraphSelectionDropdown GraphDropdown { get; set; }

        /// <summary>
        /// </summary>
        private CachedHitDifferenceGraph DevianceGraph { get; set; }

        /// <summary>
        /// </summary>
        private TextKeyValue Mean { get; set; }

        /// <summary>
        /// </summary>
        private TextKeyValue StandardDeviation { get; set; }

        /// <summary>
        /// </summary>
        private TextKeyValue Ratio { get; set; }

        /// <summary>
        /// </summary>
        private const int STATISTICS_SPACING_X = 65;

        /// <summary>
        /// </summary>
        private HitStatistics Statistics { get; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="isSubmittingScore"></param>
        /// <param name="scoreSubmissionStats"></param>
        public ResultsOverviewGraphContainer(Map map, Bindable<ScoreProcessor> processor, Bindable<bool> isSubmittingScore,
            Bindable<ScoreSubmissionResponse> scoreSubmissionStats)
        {
            Map = map;
            Processor = processor;
            IsSubmittingScore = isSubmittingScore;
            ScoreSubmissionStats = scoreSubmissionStats;

            Image = UserInterface.ResultsGraphContainerPanel;
            Size = new ScalableVector2(ResultsScreenView.CONTENT_WIDTH - ResultsTabContainer.PADDING_X, Image.Height);

            Statistics = Processor.Value.Stats != null ? Processor.Value.GetHitStatistics() : new HitStatistics();

            CreateFooterContainer();
            CreateContentContainer();
            CreateDividerLine();
            CreateLeftAndRightContainers();
            CreateJudgementGraph();
            CreateSelectionDropdown();
            CreateDevianceGraph();
            CreateMean();
            CreateStandardDeviation();
            CreateRatio();

            GraphDropdown.Parent = this;
        }

        /// <summary>
        /// </summary>
        private void CreateFooterContainer()
        {
            FooterContainer = new Container
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Width, 69),
            };

            Footer = new ResultsOverviewFooter(Map, Processor, IsSubmittingScore, ScoreSubmissionStats, FooterContainer.Size)
            {
                Parent = FooterContainer
            };
        }

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

        /// <summary>
        /// </summary>
        private void CreateMean()
        {
            Mean = new TextKeyValue("Mean:", $"{Statistics.Mean:0.00} ms", 22, Color.White)
            {
                Parent = RightContainer,
                X = -GraphDropdown.X,
                Y = GraphDropdown.Y + GraphDropdown.Dropdown.Height / 2f,
                Key = { Tint = ColorHelper.HexToColor("#808080")  },
                Value = { Tint = ColorHelper.HexToColor("#45D6F5") }
            };


            Mean.Y -= Mean.Height / 2f;
        }

        /// <summary>
        /// </summary>
        private void CreateStandardDeviation()
        {
            StandardDeviation = new TextKeyValue("Std. Dev:", $"{Statistics.StandardDeviation:0.00} ms", Mean.Key.FontSize, Color.White)
            {
                Parent = RightContainer,
                X = Mean.X + Mean.Width + STATISTICS_SPACING_X,
                Y = Mean.Y,
                Key = { Tint = Mean.Key.Tint },
                Value = { Tint = Mean.Value.Tint }
            };
        }

        /// <summary>
        /// </summary>
        private void CreateRatio()
        {
            var judgements = Processor.Value.CurrentJudgements;

            var ratio = "0:0";

            if (judgements[Judgement.Perf] != 0)
                ratio = $"{(float) judgements[Judgement.Marv] / judgements[Judgement.Perf]:0.0}:1";

            Ratio = new TextKeyValue("Ratio:", ratio, Mean.Key.FontSize, Color.White)
            {
                Parent = RightContainer,
                X = StandardDeviation.X + StandardDeviation.Width + STATISTICS_SPACING_X,
                Y = Mean.Y,
                Key = { Tint = Mean.Key.Tint },
                Value = { Tint = Mean.Value.Tint }
            };
        }

        /// <summary>
        /// </summary>
        private void CreateSelectionDropdown() => GraphDropdown = new GraphSelectionDropdown()
        {
            Parent = RightContainer,
            Alignment = Alignment.TopRight,
            X = -26,
            Y = 16,
        };

        /// <summary>
        /// </summary>
        private void CreateDevianceGraph()
        {
            if (Processor.Value.Stats != null && Processor.Value.Stats.Count > 0)
            {
                DevianceGraph = new CachedHitDifferenceGraph(Map, Processor,
                    new ScalableVector2(RightContainer.Width + GraphDropdown.X * 2 + 12, 300))
                {
                    Parent = RightContainer,
                    Alignment = Alignment.TopLeft,
                    X = -GraphDropdown.X - 12,
                    Y = GraphDropdown.Y + GraphDropdown.Height + 40,
                    Visible = true
                };

                return;
            }

            var _ = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Statistics Not Available", 22)
            {
                Parent = RightContainer,
                Alignment = Alignment.MidCenter
            };
        }
    }
}