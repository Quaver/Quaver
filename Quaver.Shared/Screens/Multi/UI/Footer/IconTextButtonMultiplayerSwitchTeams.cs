using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multi.UI.Footer
{
    public class IconTextButtonMultiplayerSwitchTeams : IconTextButton
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public IconTextButtonMultiplayerSwitchTeams(Bindable<MultiplayerGame> game)
            : base(FontAwesome.Get(FontAwesomeIcon.fa_refresh_arrow),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Switch Teams", (sender, args) =>
            {
                if (game.Value.Ruleset == MultiplayerGameRuleset.Team)
                {
                    MultiplayerTeam team;

                    if (OnlineManager.GetTeam(OnlineManager.Self.OnlineUser.Id, game.Value) == MultiplayerTeam.Red)
                        team = MultiplayerTeam.Blue;
                    else
                        team = MultiplayerTeam.Red;

                    OnlineManager.Client?.ChangeGameTeam(team);
                }
            })
        {
        }
    }
}