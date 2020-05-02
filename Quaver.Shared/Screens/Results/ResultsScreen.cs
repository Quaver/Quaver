using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Screens.Results.UI;
using Quaver.Shared.Screens.Results.UI.Header.Contents.Tabs;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Results
{
    public sealed class ResultsScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Results;

        /// <summary>
        /// </summary>
        public Map Map { get; }

        /// <summary>
        /// </summary>
        public Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        /// </summary>
        public Bindable<ResultsScreenTabType> ActiveTab { get; } = new Bindable<ResultsScreenTabType>(ResultsScreenTabType.Overview);

        /// <summary>
        /// </summary>
        public ResultsScreen(Map map, Score score)
        {
            Map = map;
            Processor = new Bindable<ScoreProcessor>(new ScoreProcessorKeys(score.ToReplay()));

            View = new ResultsScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Processor.Dispose();
            ActiveTab.Dispose();
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;
    }
}