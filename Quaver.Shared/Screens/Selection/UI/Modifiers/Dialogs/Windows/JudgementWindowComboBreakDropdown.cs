using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Dialogs.Windows
{
    public class JudgementWindowComboBreakDropdown : LabelledDropdown
    {
        private Bindable<JudgementWindows> SelectedWindow { get; }

        public JudgementWindowComboBreakDropdown(Bindable<JudgementWindows> selectedWindow) : base("Combo Break Judgement: ", 24,
            new Dropdown(GetDropdownItems(), new ScalableVector2(150, 40), 24, ColorHelper.HexToColor("#10C8F6")))
        {
            SelectedWindow = selectedWindow;
            SelectedWindow.ValueChanged += OnSelectedWindowChanged;
            SetSelectedItem();

            Dropdown.ItemSelected += OnItemSelected;
        }

        public override void Update(GameTime gameTime)
        {
            if (SelectedWindow.Value.IsDefault && Dropdown.Opened)
                Dropdown.Close();

            Dropdown.IsClickable = !SelectedWindow.Value.IsDefault;
            base.Update(gameTime);
        }

        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SelectedWindow.ValueChanged -= OnSelectedWindowChanged;
            base.Destroy();
        }

        private static List<string> GetDropdownItems() => new List<string>
        {
            "Perfect",
            "Great",
            "Good",
            "Okay",
            "Miss"
        };

        private void SetSelectedItem() => ScheduleUpdate(() =>
        {
            Dropdown.SelectItem(Dropdown.Items[(int) SelectedWindow.Value.ComboBreakJudgement - 1]);
        });

        private void OnSelectedWindowChanged(object sender, BindableValueChangedEventArgs<JudgementWindows> e) => SetSelectedItem();

        private void OnItemSelected(object sender, DropdownClickedEventArgs e)
            => SelectedWindow.Value.ComboBreakJudgement = (Judgement) (e.Index + 1);
    }
}