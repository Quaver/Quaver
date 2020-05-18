using IniFileParser.Model;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentDifficultyRating : TournamentOverlaySpriteText
    {
        private Qua Qua { get; }

        public TournamentDifficultyRating(Qua qua, TournamentSettingsDifficultyRating settings) : base(settings)
        {
            Qua = qua;
            SetText();
        }

        public override void UpdateState()
        {
            base.UpdateState();

            var difficulty = Qua.SolveDifficulty();
            Text = StringHelper.RatingToString(difficulty.OverallDifficulty);

            var settings = (TournamentSettingsDifficultyRating) Settings;

            if (settings.UseDefaultColor.Value)
                Tint = ColorHelper.DifficultyToColor(difficulty.OverallDifficulty);
        }
    }

    public class TournamentSettingsDifficultyRating : TournamentDrawableSettings
    {
        public Bindable<bool> UseDefaultColor { get; } = new Bindable<bool>(true);

        public TournamentSettingsDifficultyRating(string name) : base(name)
        {
        }

        public override void Load(KeyDataCollection ini)
        {
            UseDefaultColor.Value = ConfigHelper.ReadBool(Inverted.Default, ini[$"{Name}UseDefaultColor"]);
            base.Load(ini);
        }

        public override void Dispose()
        {
            UseDefaultColor.Dispose();
            base.Dispose();
        }
    }
}