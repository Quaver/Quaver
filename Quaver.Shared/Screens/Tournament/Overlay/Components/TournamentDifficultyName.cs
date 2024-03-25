using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentDifficultyName : TournamentOverlaySpriteText
    {
        private Qua Qua { get; }

        private TournamentPlayer Player { get; }

        public TournamentDifficultyName(Qua qua, TournamentSettingsDifficultyRating settings, List<TournamentPlayer> players) : base(settings)
        {
            Qua = qua;
            Player = players.First();
            SetText();
        }

        public override void UpdateState()
        {
            base.UpdateState();

            var settings = (TournamentSettingsDifficultyRating) Settings;

            if (settings.UseDefaultColor.Value)
                Tint = ColorHelper.DifficultyToColor((float) Player.Rating.DifficultyRating);

            Text = Qua.DifficultyName;
            TruncateWithEllipsis(Settings.MaxWidth.Value);
        }
    }
}