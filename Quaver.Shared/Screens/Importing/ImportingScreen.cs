/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Settings;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Select;
using Quaver.Shared.Screens.Settings;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Importing
{
    public class ImportingScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Importing;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;


        /// <summary>
        /// </summary>
        public ImportingScreen() => View = new ImportingScreenView(this);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            ThreadScheduler.Run(() =>
            {
                MapsetImporter.ImportMapsetsInQueue();

                if (QuaverSettingsDatabaseCache.OutdatedMaps.Count != 0)
                {
                    var view = View as ImportingScreenView;
                    view.Header.Text = "Please wait while we're recalculating map difficulties";
                    QuaverSettingsDatabaseCache.RecalculateDifficultiesForOutdatedMaps();;
                }

                OnImportCompletion();
            });

            base.OnFirstUpdate();
        }

        /// <summary>
        ///     Called after all maps have been imported to the database.
        /// </summary>
        private void OnImportCompletion()
        {
            Logger.Important($"Map import has completed", LogType.Runtime);

            if (SelectScreen.PreviousSearchTerm != "")
                SelectScreen.PreviousSearchTerm = "";

            Exit(() =>
            {
                AudioEngine.Track?.Fade(10, 300);
                return new SelectScreen();
            });
        }
    }
}
