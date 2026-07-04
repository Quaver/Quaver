using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    public class DrawableEditorLineTimingPoint : DrawableEditorLine
    {
        public TimingPointInfo TimingPoint { get; }

        public DrawableEditorLineTimingPoint(EditorPlayfield playfield, TimingPointInfo timingPoint) : base(playfield)
        {
            TimingPoint = timingPoint;
        }

        public override Color GetColor() => ColorHelper.HexToColor("#FE5656");

        public override string GetValue() => "";

        public override float StartTime => TimingPoint.StartTime;

        public override void SetSize()
        {
            const int height = 4;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Height != height)
                Height = height;
        }
    }
}