/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Linq;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Listening;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Settings;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.Multiplayer;
using Quaver.Shared.Screens.Music;
using Quaver.Shared.Screens.Select;
using Quaver.Shared.Screens.Selection;
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

        /// <summary>
        /// </summary>
        private MultiplayerScreen MultiplayerScreen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     The selected map when the screen was initialied
        /// </summary>
        private Map PreviouslySelectedMap { get; set; }

        /// <summary>
        ///     IF the screen was called when coming from select in a multiplayer game.
        ///     This will bring the user back to song select after importing
        /// </summary>
        private bool ComingFromSelect { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;

        /// <summary>
        /// </summary>
        public ImportingScreen(MultiplayerScreen multiplayerScreen = null, bool fromSelect = false)
        {
            ComingFromSelect = fromSelect;
            MultiplayerScreen = multiplayerScreen;

            PreviouslySelectedMap = MapManager.Selected.Value;
            View = new ImportingScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            AudioEngine.Track?.Fade(100, 300);

            ThreadScheduler.Run(() =>
            {
                MapsetImporter.ImportMapsetsInQueue();

                if (MapDatabaseCache.MapsToUpdate.Count != 0)
                    MapDatabaseCache.ForceUpdateMaps();

                if (QuaverSettingsDatabaseCache.OutdatedMaps.Count != 0)
                {
                    var view = View as ImportingScreenView;
                    view.Status.Text = "Performing difficulty calculation for outdated maps".ToUpper();
                    QuaverSettingsDatabaseCache.RecalculateDifficultiesForOutdatedMaps();
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

            if (OnlineManager.CurrentGame != null)
            {
                MapManager.Selected.Value = PreviouslySelectedMap;

                Exit(() =>
                {
                    if (ComingFromSelect)
                        return new SelectionScreen();

                    return new MultiplayerGameScreen();
                });
            }
            else if (OnlineManager.ListeningParty != null)
            {
                Exit(() =>
                {
                    AudioEngine.LoadCurrentTrack();
                    AudioEngine.Track.Play();

                    OnlineManager.UpdateListeningPartyState(ListeningPartyAction.ChangeSong);

                    return new MusicPlayerScreen();
                });
            }
            else if (OnlineManager.IsSpectatingSomeone)
            {
                // TODO: Whenever handling multiple spectatee's, this should be reworked, but it's fine for now.
                var spectatee = OnlineManager.SpectatorClients.First();
                spectatee.Value.WatchUserImmediately();
            }
            else
            {
                Exit(() => new SelectionScreen());
            }
        }
    }
}
