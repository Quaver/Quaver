using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState
{
    /// <summary>
    ///     Enum for all of our different game states
    /// </summary>
    internal enum State
    {
        MainMenu,
        SongSelect,
        SongLoading,
        PlayScreen,
        PlayPause,
        GameOver,
        ScoreScreen,
        LoadingScreen
    }
}
