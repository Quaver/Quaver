/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Scheduling;
using Wobble;

namespace Quaver.Shared.Skinning
{
    public static class SkinManager
    {
        /// <summary>
        ///     The time that the user has requested their skin be reloaded.
        /// </summary>
        public static long TimeSkinReloadRequested { get; set; }

        /// <summary>
        ///     If non-null, we require a skin reload.
        /// </summary>
        public static string NewQueuedSkin { get; set; }

        /// <summary>
        ///     The currently selected skin
        /// </summary>
        public static SkinStore Skin { get; private set; }

        /// <summary>
        ///     Loads the currently selected skin
        /// </summary>
        public static void Load() => Skin = new SkinStore();

        /// <summary>
        ///     Called every frame. Waits for a skin reload to be queued up.
        /// </summary>
        public static void HandleSkinReloading()
        {
            // Reload skin when applicable
            if (TimeSkinReloadRequested != 0 && GameBase.Game.TimeRunning - TimeSkinReloadRequested >= 400)
            {
                Load();
                TimeSkinReloadRequested = 0;

                ThreadScheduler.RunAfter(() =>
                {
                    Transitioner.FadeOut();
                    NotificationManager.Show(NotificationLevel.Success, "Skin has been successfully loaded!");
                }, 200);
            }
        }
    }
}
