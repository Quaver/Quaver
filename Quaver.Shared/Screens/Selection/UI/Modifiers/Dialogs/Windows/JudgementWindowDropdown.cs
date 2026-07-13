using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Dialogs.Windows
{
    public abstract class JudgementWindowDropdown : LabelledDropdown
    {
        protected abstract int Target { get; set; }

        protected Bindable<JudgementWindows> SelectedWindow { get; }

        public JudgementWindowDropdown(Bindable<JudgementWindows> selectedWindow, string label, List<string> options) : base(label, 18,
            new Dropdown(options, new ScalableVector2(150, 40), 18, ColorHelper.HexToColor("#10C8F6")))
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

        private void SetSelectedItem() =>
            ScheduleUpdate(() => Dropdown.SelectItem(Dropdown.Items[Target]));

        private void OnSelectedWindowChanged(object sender, BindableValueChangedEventArgs<JudgementWindows> e) =>
            SetSelectedItem();

        private void OnItemSelected(object sender, DropdownClickedEventArgs e) =>
            Target = e.Index;
    }
}