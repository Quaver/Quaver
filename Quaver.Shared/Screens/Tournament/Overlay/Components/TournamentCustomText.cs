namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentCustomText : TournamentOverlaySpriteText
    {
        public TournamentCustomText(TournamentSettingsCustomText settings) : base(settings)
        {
            SetText();
        }

        public override void UpdateState()
        {
            var settings = (TournamentSettingsCustomText) Settings;
            Text = settings.CustomText.Value;
            base.UpdateState();
        }
    }
}