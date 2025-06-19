using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Dialogs.Windows
{
    public class JudgementWindowComboBreakDropdown : JudgementWindowDropdown
    {
        protected override int Target
        {
            get => (int)SelectedWindow.Value.ComboBreakJudgement - 1;
            set => SelectedWindow.Value.ComboBreakJudgement = (Judgement)(value + 1);
        }

        public JudgementWindowComboBreakDropdown(Bindable<JudgementWindows> selectedWindow) : base(selectedWindow, "Combo Break Judgement: ") { }
    }
}