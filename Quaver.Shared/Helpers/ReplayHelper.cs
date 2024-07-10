/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Replays;
using Quaver.Shared.Modifiers;

namespace Quaver.Shared.Helpers
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

            if (ModeHelper.IsKeyMode(map.Mode))
                replay = Replay.GeneratePerfectReplayKeys(replay, map);
            else
                throw new ArgumentOutOfRangeException();

            return replay;
        }
    }
}
