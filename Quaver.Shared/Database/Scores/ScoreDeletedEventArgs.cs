using System;

namespace Quaver.Shared.Database.Scores
{
    public class ScoreDeletedEventArgs : EventArgs
    {
        public Score Score { get; }

        public ScoreDeletedEventArgs(Score score) => Score = score;
    }
}