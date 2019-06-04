using System.Collections.Generic;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsRuleset : MultiplayerSettingsText
    {
        public MultiplayerSettingsRuleset(string name, string value) : base(name, value,
            () => new MenuDialog("Change Game Mode", new List<IMenuDialogOption>
            {
                new MenuDialogOption("Free For All", () => NotificationManager.Show(NotificationLevel.Info, "FFA")),
                new MenuDialogOption("Team", () => NotificationManager.Show(NotificationLevel.Info, "Team")),
                new MenuDialogOption("Battle Royale", () => NotificationManager.Show(NotificationLevel.Info, "BR"))
            }))
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