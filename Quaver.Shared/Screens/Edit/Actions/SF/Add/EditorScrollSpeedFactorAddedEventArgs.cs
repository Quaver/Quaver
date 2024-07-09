using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.SF.Add
{
    public class EditorScrollSpeedFactorAddedEventArgs : EventArgs
    {
        public ScrollSpeedFactorInfo ScrollSpeedFactor { get; }

        public EditorScrollSpeedFactorAddedEventArgs(ScrollSpeedFactorInfo SF) => ScrollSpeedFactor = SF;
    }
}