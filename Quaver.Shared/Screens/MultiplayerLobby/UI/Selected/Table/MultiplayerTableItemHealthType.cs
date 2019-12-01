using System;
using Quaver.API.Enums;
using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemHealthType : MultiplayerTableItem
    {
        public MultiplayerTableItemHealthType(Bindable<MultiplayerGame> game) : base(game)
        {
        }

        public override string GetName() => "Health System";

        public override string GetValue()
        {
            switch ((MultiplayerHealthType) SelectedGame.Value.HealthType)
            {
                case MultiplayerHealthType.Manual_Regeneration:
                    return "Manual Regeneration";
                case MultiplayerHealthType.Lives:
                    return "Lives";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}