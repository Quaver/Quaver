using System;
using System.Collections.Generic;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Multiplayer.UI.Dialogs;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsFreeModType : MultiplayerSettingsText
    {
        private MultiplayerFreeModType Type { get; }

        public MultiplayerSettingsFreeModType(string name, string value, MultiplayerFreeModType type) : base(name, value)
        {
            Type = type;

            CreateDialog = () =>
            {
                if (OnlineManager.CurrentGame.FreeModType.HasFlag(Type))
                    OnlineManager.Client?.ChangeGameFreeModType((MultiplayerFreeModType) (OnlineManager.CurrentGame.FreeModType - Type));
                else
                    OnlineManager.Client?.ChangeGameFreeModType(OnlineManager.CurrentGame.FreeModType | Type);

                return null;
            };

            OnlineManager.Client.OnFreeModTypeChanged += OnFreeModTypeChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnFreeModTypeChanged -= OnFreeModTypeChanged;
            base.Destroy();
        }

        private void OnFreeModTypeChanged(object sender, FreeModTypeChangedEventArgs e)
        {
            Value.Text = FreeModTypeToString(Type);
        }

        public static string FreeModTypeToString(MultiplayerFreeModType type)
        {
            if (OnlineManager.CurrentGame.FreeModType.HasFlag(type))
                return "Yes";

            return "No";
        }
    }
}