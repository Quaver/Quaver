using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Buttons;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Modifiers.Mods;
using Quaver.Shared.Screens.Selection.UI.Modifiers.Dialogs.Windows;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Components
{
    public class SelectableModifierJudgementWindows : SelectableModifier
    {
        public SelectableModifierJudgementWindows(int width) : base(width, new ModJudgementWindows())
        {
            var customizeButton = new RoundedButton((sender, args) => DialogManager.Show(new JudgementWindowDialog()))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Icon.X,
                Size = new ScalableVector2(102, 22),
                Tint = ColorHelper.HexToColor("#F2C94C")
            };

            customizeButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "CUSTOMIZE", 20, ColorHelper.HexToColor("#242424"));

            Clicked += (sender, args) => DialogManager.Show(new JudgementWindowDialog());
        }
    }
}