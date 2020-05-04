using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Results.UI.Header.Contents.Tabs;
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
        private static Color TextDarkGray { get; } = ColorHelper.HexToColor("#808080");

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="activeTab"></param>
        public ResultsOverviewTab(Map map, Bindable<ScoreProcessor> processor, Bindable<ResultsScreenTabType> activeTab)
            : base(map, processor, activeTab)
        {
            CreateContentContainer();
            CreateJudgementWindowPreset();
            CreateDateAndTime();
            CreatePlayedBy();
            CreateScoreContainer();
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
            Parent = this,
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
                Parent = this,
                Alignment = Alignment.TopRight,
                Tint = TextDarkGray
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePlayedBy() => PlayedBy = new TextKeyValue("Played by", $"{Processor.Value.PlayerName}", 22, Color.White)
        {
            Parent = this,
            Alignment = Alignment.TopRight,
            X = -DateAndTime.Width - 4,
            Key = { Tint =  TextDarkGray },
            Value = { Tint = ColorHelper.HexToColor("#00D1FF")}
        };

        /// <summary>
        /// </summary>
        private void CreateScoreContainer() => ScoreContainer = new ResultsOverviewScoreContainer(Map, Processor)
        {
            Parent = this,
            Y = JudgementWindows.Y + JudgementWindows.Height + 10,
            X = PADDING_X
        };
    }
}