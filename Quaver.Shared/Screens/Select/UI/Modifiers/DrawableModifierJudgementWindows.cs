using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Quaver.Shared.Screens.Select.UI.Modifiers.Windows;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Select.UI.Modifiers
{
    public class DrawableModifierJudgementWindows : DrawableModifier
    {
        public DrawableModifierJudgementWindows(ModifiersDialog dialog, IGameplayModifier modifier) : base(dialog, modifier)
        {
            var btn = new BorderedTextButton("Customize", Colors.MainAccent)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -24,
                Text =
                {
                    FontSize = 14,
                    Font = Fonts.Exo2SemiBold,
                    Alignment = Alignment.MidCenter,
                    UsePreviousSpriteBatchOptions = true
                },
                UsePreviousSpriteBatchOptions = true
            };

            btn.Clicked += (sender, args) =>
            {
                DialogManager.Show(new CustomizeJudgementWindowsDialog(0.75f));
                Dialog.Close();
            };
        }

        public override void ChangeSelectedOptionButton()
        {
        }
    }
}