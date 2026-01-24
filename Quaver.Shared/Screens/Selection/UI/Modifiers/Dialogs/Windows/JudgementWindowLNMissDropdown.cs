using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Dialogs.Windows
{
    public class JudgementWindowLNMissDropdown : JudgementWindowDropdown
    {
        protected override int Target
        {
            get => (int)SelectedWindow.Value.LNMissJudgement;
            set => SelectedWindow.Value.LNMissJudgement = (Judgement)value;
        }

        public JudgementWindowLNMissDropdown(Bindable<JudgementWindows> selectedWindow) : base(selectedWindow, "LN Miss Judgement: ", GetDropdownItems()) { }

        private static List<string> GetDropdownItems() => new()
            {
                "Marvelous",
                "Perfect",
                "Great",
                "Good",
                "Okay",
                "Miss"
            };
    }
}