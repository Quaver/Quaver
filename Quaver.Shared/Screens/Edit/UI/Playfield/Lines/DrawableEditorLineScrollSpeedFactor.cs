using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    public class DrawableEditorLineScrollSpeedFactor : DrawableEditorLine
    {
        public ScrollSpeedFactorInfo ScrollSpeedFactor { get; }

        private readonly int _keyCount;

        public DrawableEditorLineScrollSpeedFactor(EditorPlayfield playfield, ScrollSpeedFactorInfo sv) :
            base(playfield)
        {
            ScrollSpeedFactor = sv;
            IsClickable = false;
            _keyCount = Playfield.ActionManager.EditScreen.WorkingMap.GetKeyCount();
        }

        public override Color GetColor() => Colors.MainBlue;

        public override string GetValue() => "";

        public override int GetTime() => (int)Math.Round(ScrollSpeedFactor.StartTime, MidpointRounding.AwayFromZero);

        public override void SetSize()
        {
            var multiplier = Math.Abs(ScrollSpeedFactor.Factor);
            var width = MathHelper.Clamp(DefaultSize.X.Value * multiplier, 10, 150);

            var height = MathHelper.Clamp(ScrollSpeedFactor.GetLaneMaskLanes(_keyCount).Count(), 2, 15);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Width != width)
                Width = width;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Height != height)
                Height = height;
        }
    }
}