using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.TimingGroups.Colors
{
    public class EditorTimingGroupColorChangedEventArgs : EventArgs
    {
        public TimingGroup TimingGroup { get; }

        public Color OldColor { get; }

        public Color NewColor { get; }

        public EditorTimingGroupColorChangedEventArgs(TimingGroup timingGroup, Color oldColor, Color newColor)
        {
            TimingGroup = timingGroup;
            OldColor = oldColor;
            NewColor = newColor;
        }
    }
}