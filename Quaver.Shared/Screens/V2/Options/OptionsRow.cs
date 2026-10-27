using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.V2.Options.Model;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options
{
    internal class OptionsRow : FlexContainer
    {

        private OptionsDefinition Definition { get; }

        private RoundedButton Label { get; }

        private Drawable Control { get; set; }

        public OptionsRow(OptionsDefinition definition)
        {
            Definition = definition;

            Size = new ScalableVector2(1, 54);
            Direction = FlexDirection.Row;
            AlignItems = FlexAlignItems.Center;
            ColumnGap = 10;

            Label = new RoundedButton
            {
                Parent = this,
                Size = new ScalableVector2(1, Height),
                CornerRadius = 6,
                Tint = ColorHelper.HexToColor("#181E25"),
                PerformHoverFade = false,
                IsClickable = false
            };

            var labelText = Definition.LabelKeyCount.HasValue ? LocalizationManager.Get(Definition.LabelName, Definition.LabelKeyCount.Value) : LocalizationManager.Get(Definition.LabelName);
            Label.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), labelText, 18, Color.White);
            Label.Label.Alignment = Alignment.MidLeft;
            Label.Label.X = 15;

            SetItemOptions(Label, new FlexItemOptions { Basis = 0, Grow = 1, Shrink = 0 });
        }

        public void SetControl(Drawable control)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            if (Control != null)
                throw new InvalidOperationException("This option row already has a control.");

            Control = control;

            // Dropdowns should be placed next to the option row, while others option types shoulld be included inside the row
            if (control is V2DropdownBase)
            {
                Control.Parent = this;
                Control.Height = Height;
                SetItemOptions(Control, new FlexItemOptions { Shrink = 0 });
            }
            else
            {
                Control.Parent = Label;
                Control.Alignment = Alignment.MidRight;
                Control.Position = new ScalableVector2(-15, 0);
            }

            RefreshLayout();
        }
    }
}
