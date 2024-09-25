using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Plugins;
using Quaver.Shared.Screens.Edit.Plugins.Timing;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    public class DrawableEditorLineScrollVelocity : DrawableEditorLine
    {
        public TimingGroup TimingGroup { get; }
        public SliderVelocityInfo ScrollVelocity { get; }

        public DrawableEditorLineScrollVelocity(EditorPlayfield playfield, SliderVelocityInfo sv, TimingGroup timingGroup) : base(playfield)
        {
            ScrollVelocity = sv;
            TimingGroup = timingGroup;
            IsClickable = false;
        }

        public override Color GetColor() => ColorHelper.HexToColor("#56FE6E");

        public override string GetValue() => "";

        public override float StartTime => ScrollVelocity.StartTime;

        public override void SetSize()
        {
            var svPlugin = (EditorScrollVelocityPanel)Playfield.ActionManager.EditScreen.BuiltInPlugins[EditorBuiltInPlugin.ScrollVelocityEditor];
            if (svPlugin.CurrentScrollGroup != TimingGroup)
            {
                Width = 0;
                return;
            }

            var multiplier = Math.Abs(ScrollVelocity.Multiplier);
            var size = MathHelper.Clamp(DefaultSize.X.Value * multiplier, 10, 150);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Width != size)
                Width = size;
        }
    }
}