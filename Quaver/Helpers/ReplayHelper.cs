using System;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Replays;
using Quaver.Main;

namespace Quaver.Helpers
{
    internal static class ReplayHelper
    {
        /// <summary>
        ///     Generates a perfect replay given the game mode.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static Replay GeneratePerfectReplay(Qua map, string md5)
        {        
            var replay = new Replay(map.Mode, "Autoplay", GameBase.CurrentMods, md5);

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