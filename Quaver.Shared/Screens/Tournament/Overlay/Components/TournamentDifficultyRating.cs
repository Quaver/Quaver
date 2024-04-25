using System.Collections.Generic;
using IniFileParser.Model;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using System.Linq;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentDifficultyRating : TournamentOverlaySpriteText
    {
        private Qua Qua { get; }

        private TournamentPlayer Player { get; }

        public TournamentDifficultyRating(Qua qua, TournamentSettingsDifficultyRating settings, List<TournamentPlayer> players) : base(settings)
        {
            Qua = qua;
            Player = players.First();
            SetText();
        }

        public override void UpdateState()
        {
            base.UpdateState();

            Text = StringHelper.RatingToString(Player.Rating.DifficultyRating);

            var settings = (TournamentSettingsDifficultyRating) Settings;

            if (settings.UseDefaultColor.Value)
                Tint = ColorHelper.DifficultyToColor((float) Player.Rating.DifficultyRating);
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