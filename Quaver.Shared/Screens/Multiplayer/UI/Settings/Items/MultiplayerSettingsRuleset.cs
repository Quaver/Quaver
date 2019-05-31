using Quaver.Server.Client.Handlers;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsRuleset : MultiplayerSettingsText
    {
        public MultiplayerSettingsRuleset(string name, string value) : base(name, value)
        {
            OnlineManager.Client.OnGameRulesetChanged += OnGameRulesetChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnGameRulesetChanged -= OnGameRulesetChanged;
            base.Destroy();
        }

        private void OnGameRulesetChanged(object sender, RulesetChangedEventArgs e)
            => Value.Text = e.Ruleset.ToString().Replace("_", " ");
    }
}