using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI;
using Wobble.Bindables;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multi.UI.Footer
{
    public class IconTextButtonMultiplayerModifiers : IconTextButton
    {
        public IconTextButtonMultiplayerModifiers(MultiplayerGameScreen screen)
            : base(FontAwesome.Get(FontAwesomeIcon.fa_open_wrench_tool_silhouette),
                FontManager.GetWobbleFont(Fonts.LatoBlack),"Modifiers", (sender, args) =>
                {
                    if (screen.ActiveLeftPanel.Value == SelectContainerPanel.Modifiers)
                        screen.ActiveLeftPanel.Value = SelectContainerPanel.MatchSettings;
                    else
                        screen.ActiveLeftPanel.Value = SelectContainerPanel.Modifiers;
                })
        {
        }
    }
}