using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState
{
    /// <summary>
    ///     Enum for all the different game states. (Can only be accessed within Quaver.GameState namespace.)
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
        LoadingScreen,
        TestScreen
    }
}
