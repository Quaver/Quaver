using Quaver.API.Enums;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Deviance
{
    public struct HitDifferenceGraphLineData
    {
        public Judgement Judgement { get; }

        public Sprite Line { get; }

        public float Deviance { get; }

        public HitDifferenceGraphLineData(Judgement j, Sprite line, float deviance)
        {
            Judgement = j;
            Line = line;
            Deviance = deviance;
        }
    }
}