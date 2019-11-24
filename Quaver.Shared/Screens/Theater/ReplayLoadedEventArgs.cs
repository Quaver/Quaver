using System;
using Quaver.API.Replays;

namespace Quaver.Shared.Screens.Theater
{
    public class ReplayLoadedEventArgs : EventArgs
    {
        public Replay Replay { get; }

        public ReplayLoadedEventArgs(Replay replay) => Replay = replay;
    }
}