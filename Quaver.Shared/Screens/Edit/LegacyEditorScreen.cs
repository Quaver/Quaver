/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.API.Maps;
using Quaver.Server.Common.Objects;

namespace Quaver.Shared.Screens.Editor
{
    public sealed class EditorScreen : QuaverScreen
    {
        /// <summary>
        /// </summary>
        public EditorScreen(Qua map)
        {
            // legacy screen
        }

        public override QuaverScreenType Type { get; }
        public override UserClientStatus GetClientStatus() => throw new NotImplementedException();
    }
}