using System.Collections.Generic;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsFreeModType : MultiplayerSettingsText
    {
        public MultiplayerSettingsFreeModType(string name, string value) : base(name, value)
        {
            OnlineManager.Client.OnFreeModTypeChanged += OnFreeModTypeChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnFreeModTypeChanged -= OnFreeModTypeChanged;
            base.Destroy();
        }

        private void OnFreeModTypeChanged(object sender, FreeModTypeChangedEventArgs e)
            => Value.Text = FreeModTypeToString(e.Type);

        public static string FreeModTypeToString(MultiplayerFreeModType type)
        {
            if (type == MultiplayerFreeModType.None)
                return "None";

            var list = new List<string>();

            if (type.HasFlag(MultiplayerFreeModType.Regular))
                list.Add("Regular");

            if (type.HasFlag(MultiplayerFreeModType.Rate))
                list.Add("Rate");

            return string.Join(", ", list);
        }
    }
}