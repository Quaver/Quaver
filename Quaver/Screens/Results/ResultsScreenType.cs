using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Screens.Results
{
    /// <summary>
    ///     Determines where we are coming from when loading the results screen.
    ///     Dictates how we should display it.
    /// </summary>
    public enum ResultsScreenType
    {
        FromGameplay,
        FromReplayFile,
        FromLocalScore
    }
}
