using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using System.Collections.Generic;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Dialogs.Windows
{
    public class JudgementWindowComboBreakDropdown : JudgementWindowDropdown
    {
        // This dropdown excludes Marvelous (`(Judgement)0`), since that would entirely disable combo.
        // This means we need to offset the dropdown value by one.
        protected override int Target
        {
            get => (int)SelectedWindow.Value.ComboBreakJudgement - 1;
            set => SelectedWindow.Value.ComboBreakJudgement = (Judgement)(value + 1);
        }

        public JudgementWindowComboBreakDropdown(Bindable<JudgementWindows> selectedWindow) : base(selectedWindow, "Combo Break Judgement: ", GetDropdownItems()) { }

        private static List<string> GetDropdownItems() => new()
            {
                "Perfect",
                "Great",
                "Good",
                "Okay",
                "Miss"
            };
    }
}