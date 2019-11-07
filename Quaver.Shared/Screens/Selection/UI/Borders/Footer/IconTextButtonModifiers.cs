using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Wobble.Bindables;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Borders.Footer
{
    public class IconTextButtonModifiers : IconTextButton
    {
        public IconTextButtonModifiers(Bindable<SelectContainerPanel> activeLeftPanel)
            : base(FontAwesome.Get(FontAwesomeIcon.fa_open_wrench_tool_silhouette), FontManager.GetWobbleFont(Fonts.LatoBlack),
            "Modifiers", (sender, args) =>
            {
                if (activeLeftPanel == null)
                    return;

                if (activeLeftPanel.Value == SelectContainerPanel.Modifiers)
                    activeLeftPanel.Value = SelectContainerPanel.Leaderboard;
                else
                    activeLeftPanel.Value = SelectContainerPanel.Modifiers;
            })
        {
        }
    }
}