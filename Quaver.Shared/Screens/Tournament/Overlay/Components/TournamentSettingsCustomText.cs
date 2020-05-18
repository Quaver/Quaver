using IniFileParser.Model;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public class TournamentSettingsCustomText : TournamentDrawableSettings
    {
        public Bindable<string> CustomText { get; } = new Bindable<string>("");

        public TournamentSettingsCustomText(string name) : base(name)
        {
        }

        public override void Load(KeyDataCollection ini)
        {
            CustomText.Value = ini[$"{Name}CustomText"];
            base.Load(ini);
        }

        public override void Dispose()
        {
            CustomText.Dispose();
            base.Dispose();
        }
    }
}