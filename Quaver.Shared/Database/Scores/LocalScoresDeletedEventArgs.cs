using System;
using Quaver.Shared.Database.Maps;

namespace Quaver.Shared.Database.Scores
{
    public class LocalScoresDeletedEventArgs : EventArgs
    {
        public Map Map { get; }

        public LocalScoresDeletedEventArgs(Map map) => Map = map;
    }
}