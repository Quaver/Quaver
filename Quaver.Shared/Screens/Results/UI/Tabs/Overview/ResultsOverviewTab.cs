using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Results.UI.Header.Contents.Tabs;
using Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs;
using Quaver.Shared.Screens.Results.UI.Tabs.Overview.Heading;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview
{
    public class ResultsOverviewTab : ResultsTabContainer
    {
        /// <summary>
        /// </summary>
        private Container ContentContainer { get; set; }

        /// <summary>
        /// </summary>
        private TextKeyValue JudgementWindows { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus DateAndTime { get; set; }

        /// <summary>
        /// </summary>
        private TextKeyValue PlayedBy { get; set; }

        /// <summary>
        /// </summary>
        private ResultsOverviewScoreContainer ScoreContainer { get; set; }

        /// <summary>
        /// </summary>
        private ResultsOverviewGraphContainer GraphContainer { get; set; }

        /// <summary>
        /// </summary>
        private static Color TextDarkGray { get; } = ColorHelper.HexToColor("#808080");

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="activeTab"></param>
        /// <param name="isSubmittingScore"></param>
        /// <param name="scoreSubmissionStats"></param>
        public ResultsOverviewTab(Map map, Bindable<ScoreProcessor> processor, Bindable<ResultsScreenTabType> activeTab,
            Bindable<bool> isSubmittingScore, Bindable<ScoreSubmissionResponse> scoreSubmissionStats)
            : base(map, processor, activeTab, isSubmittingScore, scoreSubmissionStats)
        {
            CreateContentContainer();
            CreateJudgementWindowPreset();
            CreateDateAndTime();
            CreatePlayedBy();
            CreateScoreContainer();
            CreateGraphContainer();
        }

        /// <summary>
        /// </summary>
        private void CreateContentContainer() => ContentContainer = new Container
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(Width, 645)
        };

        /// <summary>
        /// </summary>
        private void CreateJudgementWindowPreset() => JudgementWindows = new TextKeyValue("Judgement Preset:", Processor.Value.Windows.Name,
            22, Color.White)
        {
            Parent = ContentContainer,
            X = PADDING_X,
            Key = { Tint =  TextDarkGray },
            Value = { Tint = ColorHelper.HexToColor("#FFE76B")}
        };

        /// <summary>
        /// </summary>
        private void CreateDateAndTime()
        {
            var time = $"{Processor.Value.Date:hh:mm:ss tt}";

            DateAndTime = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                $"on {Processor.Value.Date.ToShortDateString()} @ {time}", 22)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopRight,
                Tint = TextDarkGray
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePlayedBy() => PlayedBy = new TextKeyValue("Played by", $"{Processor.Value.PlayerName}", 22, Color.White)
        {
            Parent = ContentContainer,
            Alignment = Alignment.TopRight,
            X = -DateAndTime.Width - 4,
            Key = { Tint =  TextDarkGray },
            Value = { Tint = ColorHelper.HexToColor("#00D1FF")}
        };

        /// <summary>
        /// </summary>
        private void CreateScoreContainer()
        {
            ScoreContainer = new ResultsOverviewScoreContainer(Map, Processor)
            {
                Parent = ContentContainer,
                Y = JudgementWindows.Y + JudgementWindows.Height + 10,
                X = PADDING_X
            };

            ScoreContainer.CreateItems();
        }

        /// <summary>
        /// </summary>
        private void CreateGraphContainer() => GraphContainer = new ResultsOverviewGraphContainer(Map, Processor,
            IsSubmittingScore, ScoreSubmissionStats)
        {
            Parent = ContentContainer,
            Alignment = Alignment.BotLeft,
            X = PADDING_X
        };
    }
}