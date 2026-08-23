/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.UserGroupBadges
{
    public sealed class UserGroupsTestScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public UserGroupsTestScreen() => View = new UserGroupsTestScreenView(this);
    }
}
