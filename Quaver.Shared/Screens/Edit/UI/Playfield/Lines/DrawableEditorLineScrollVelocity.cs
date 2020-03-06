using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    public class DrawableEditorLineScrollVelocity : DrawableEditorLine
    {
        private SliderVelocityInfo ScrollVelocity { get; }

        public DrawableEditorLineScrollVelocity(EditorPlayfield playfield, SliderVelocityInfo sv) : base(playfield)
        {
            ScrollVelocity = sv;
        }

        public override Color GetColor() => ColorHelper.HexToColor("#56FE6E");

        public override string GetValue() => "";

        public override int GetTime() => (int) Math.Round(ScrollVelocity.StartTime, MidpointRounding.AwayFromZero);
    }
}