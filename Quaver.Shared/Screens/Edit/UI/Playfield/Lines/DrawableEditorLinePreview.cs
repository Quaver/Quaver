using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    public class DrawableEditorLinePreview : DrawableEditorLine
    {
        public int Time { get; set; }

        public DrawableEditorLinePreview(EditorPlayfield playfield, int time) : base(playfield)
        {
            Time = time;
            IsClickable = false;
        }

        public override Color GetColor() => Colors.SecondaryAccent;

        public override string GetValue() => "";

        public override int GetTime() => Time;

        public override void SetSize()
        {
            const int height = 4;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Height != height)
                Height = height;
        }
    }
}