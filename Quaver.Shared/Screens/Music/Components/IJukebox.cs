using Wobble.Graphics;

namespace Quaver.Shared.Screens.Music.Components
{
    public interface IJukebox
    {
        int TrackListQueuePosition { get; set; }

        void SelectNextTrack(Direction direction);
    }
}