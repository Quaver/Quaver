using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Replays;
using Quaver.Modifiers;
using Wobble;

namespace Quaver.Helpers
{
    public static class ReplayHelper
    {
        /// <summary>
        ///     Generates a perfect replay given the game mode.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static Replay GeneratePerfectReplay(Qua map, string md5)
        {
            var replay = new Replay(map.Mode, "Autoplay", ModManager.Mods, md5);

            switch (map.Mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    replay = Replay.GeneratePerfectReplayKeys(replay, map);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return replay;
        }
    }
}
