using System.Collections.Generic;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Multiplayer.UI.Dialogs;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsRuleset : MultiplayerSettingsText
    {
        public MultiplayerSettingsRuleset(string name, string value) : base(name, value,
            () => new MenuDialogMultiplayer("Change Game Mode", new List<IMenuDialogOption>
            {
                new MenuDialogOption("Free For All", () => OnlineManager.Client?.ChangeGameRuleset(MultiplayerGameRuleset.Free_For_All)),
                new MenuDialogOption("Team", () => OnlineManager.Client?.ChangeGameRuleset(MultiplayerGameRuleset.Team)),
                new MenuDialogOption("Battle Royale", () => OnlineManager.Client?.ChangeGameRuleset(MultiplayerGameRuleset.Battle_Royale)),
                new MenuDialogOption("Close", () => {})
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