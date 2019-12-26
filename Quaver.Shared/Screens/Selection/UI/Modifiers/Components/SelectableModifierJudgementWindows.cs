using Quaver.Shared.Assets;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Modifiers.Mods;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Selection.UI.Modifiers.Dialogs.Windows;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Components
{
    public class SelectableModifierJudgementWindows : SelectableModifier
    {
        public SelectableModifierJudgementWindows(int width) : base(width, new ModJudgementWindows())
        {
            // ReSharper disable once ObjectCreationAsStatement
            new IconButton(UserInterface.CustomizeButton, (sender, args) => DialogManager.Show(new JudgementWindowDialog()))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Icon.X,
                Size = new ScalableVector2(102, 22),
            };

            Clicked += (sender, args) => DialogManager.Show(new JudgementWindowDialog());
        }
    }
}