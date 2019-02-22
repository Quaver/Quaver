using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.SV
{
    public class EditorScrollVelocityChangeInfo
    {
        /// <summary>
        /// </summary>
        public SliderVelocityInfo Info { get; }

        /// <summary>
        /// </summary>
        public float OriginalTime { get; }

        /// <summary>
        /// </summary>
        public float OriginalMultiplier { get; }

        /// <summary>
        /// </summary>
        public float NewTime { get; }

        /// <summary>
        /// </summary>
        public float NewMultiplier { get; }

        /// <summary>
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="newTime"></param>
        /// <param name="newMultiplier"></param>
        public EditorScrollVelocityChangeInfo(SliderVelocityInfo sv, float newTime, float newMultiplier)
        {
            Info = sv;
            OriginalTime = Info.StartTime;
            OriginalMultiplier = Info.Multiplier;
            NewTime = newTime;
            NewMultiplier = newMultiplier;
        }
    }
}