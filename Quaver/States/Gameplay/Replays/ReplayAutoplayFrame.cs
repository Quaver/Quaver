using System.Windows.Forms.VisualStyles;
using Quaver.API.Maps;

namespace Quaver.States.Gameplay.Replays
{
    public struct ReplayAutoplayFrame
    {
        public ReplayAutoplayFrameType Type { get; }

        public int Time { get; }

        public ReplayKeyPressState Keys { get; set; }

        public HitObjectInfo HitObject { get; }

        public ReplayAutoplayFrame(HitObjectInfo hitObject, ReplayAutoplayFrameType type, int time, ReplayKeyPressState keys)
        {
            HitObject = hitObject;
            Type = type;
            Time = time;
            Keys = keys;
        }
    }

    public enum ReplayAutoplayFrameType
    {
        Press,
        Release
    }
}