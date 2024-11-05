using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    public class DrawableEditorLineScrollSpeedFactor : DrawableEditorLine
    {
        public TimingGroup TimingGroup { get; }
        public ScrollSpeedFactorInfo ScrollSpeedFactor { get; }

        public DrawableEditorLineScrollSpeedFactor(EditorPlayfield playfield, ScrollSpeedFactorInfo sv,
            TimingGroup timingGroup) : base(playfield)
        {
            ScrollSpeedFactor = sv;
            TimingGroup = timingGroup;
            IsClickable = false;
            Rotation = 180;
        }

        public override Color GetColor() =>
            ConfigManager.EditorColorSvLineByTimingGroup.Value && TimingGroup != null
                ? ColorHelper.ToXnaColor(TimingGroup.GetColor())
                : ColorHelper.HexToColor("#56FE6E");

        public override string GetValue() => "";

        public override float StartTime => ScrollSpeedFactor.StartTime;

        /// <summary>
        ///     Sets the position of the line
        /// </summary>
        public override void SetPosition()
        {
            var x = Playfield.AbsolutePosition.X - DesiredWidth;
            var y = Playfield.HitPositionY - StartTime * Playfield.TrackSpeed - Height;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (X != x || Y != y)
                Position = new ScalableVector2(x, y);
        }

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

            var desiredWidth = DesiredWidth;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Width != desiredWidth)
                Width = desiredWidth;
        }

        private float DesiredWidth
        {
            get
            {
                var multiplier = Math.Abs(ScrollSpeedFactor.Multiplier);
                var desiredWidth = MathHelper.Clamp(DefaultSize.X.Value * multiplier, 10, 150);
                return desiredWidth;
            }
        }
    }
}