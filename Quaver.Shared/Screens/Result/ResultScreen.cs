/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.API.Replays.Virtual;
using Quaver.Server.Client;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Helpers;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.UI.Scoreboard;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.Multiplayer;
using Quaver.Shared.Screens.Result.UI;
using Quaver.Shared.Screens.Select;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Tournament.Gameplay;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Audio.Samples;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Platform;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Result
{
    public class ResultScreen : QuaverScreen
    {
        /// <summary>
        /// </summary>
        /// <param name="score"></param>
        public ResultScreen(Score score)
        {
            //
        }

        public override QuaverScreenType Type { get; }
        public override UserClientStatus GetClientStatus() => throw new NotImplementedException();
    }
}