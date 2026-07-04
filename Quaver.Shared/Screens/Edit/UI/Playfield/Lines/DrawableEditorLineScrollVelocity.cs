using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;

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
        }

        public override Color GetColor() =>
            ConfigManager.EditorColorSvLineByTimingGroup.Value && TimingGroup != null
                ? ColorHelper.ToXnaColor(TimingGroup.GetColor())
                : ColorHelper.HexToColor("#56FE6E");

        public override string GetValue() => "";

        public override float StartTime => ScrollVelocity.StartTime;

        public override void SetSize()
        {
            Tint = GetColor();

            var selectedScrollGroup = Playfield.ActionManager.EditScreen.SelectedScrollGroup;
            if (selectedScrollGroup != TimingGroup 
                // Global scroll group is always visible
                && TimingGroup != Playfield.ActionManager.EditScreen.WorkingMap.GlobalScrollGroup)
            {
                Width = 0;
                return;
            }

            var multiplier = Math.Abs(ScrollVelocity.Multiplier);
            var desiredWidth = MathHelper.Clamp(DefaultSize.X.Value * multiplier, 10, 150);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Width != desiredWidth)
                Width = desiredWidth;
        }
    }
}