/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata;
using Quaver.Shared.Screens.Settings;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Platform;

namespace Quaver.Shared.Screens.Editor.UI.Navigation
{
    public class MainEditorNavigationBar : EditorNavigationBar
    {
        /// <summary>
        /// </summary>
        private EditorScreen Screen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MainEditorNavigationBar(EditorScreen screen) : base(new List<EditorControlButton>
        {
            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_arrow_pointing_to_left), "Back To Menu (ESC)", -48, Alignment.BotLeft,
                (o, e) => screen.HandleKeyPressEscape()),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_plus_black_symbol), "New", -48, Alignment.BotLeft,
                (o, e) => screen.CreateNewDifficulty()),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_undo_arrow), "Undo (CTRL+Z)", -48, Alignment.BotLeft,
                (o, e) => screen.Ruleset.ActionManager.Undo()),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_refresh_arrow), "Redo (CTRL+Y)", -48, Alignment.BotLeft,
                (o, e) => screen.Ruleset.ActionManager.Redo()),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_text_file), "Edit Metadata (F1)", -48, Alignment.BotLeft,
                (o, e) => screen.OpenMetadataDialog()),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_time), "Timing Setup (F2)", -48, Alignment.BotLeft,
                (o, e) => NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet")),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_dashboard), "Edit Scroll Velocities (F3)", -48, Alignment.BotLeft,
                (o, e) => screen.OpenScrollVelocityDialog()),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_music_note_black_symbol), "Set Audio Preview Time (F4)", -48, Alignment.BotLeft,
                (o, e) => screen.ChangePreviewTime((int) AudioEngine.Track.Time)),

        }, new List<EditorControlButton>
        {
            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_settings), "Open Options Menu (CTRL+O)", -48, Alignment.BotRight,
                (o, e) =>
                {
                    if (DialogManager.Dialogs.Count > 0)
                        return;

                    DialogManager.Show(new SettingsDialog());
                }),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_earth_globe), "Visit Mapset Page (CTRL+T)", -48, Alignment.BotRight,
                (o, e) => MapManager.Selected.Value.VisitMapsetPage()),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_cloud_storage_uploading_option), "Upload Mapset (CTRL+U)", -48, Alignment.BotRight,
                (o, e) => NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet")),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_archive_black_box), "Export Mapset (CTRL+E)", -48, Alignment.BotRight,
                (o, e) => EditorScreen.ExportToZip()),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_save_file_option), "Save File (CTRL+S)", -48, Alignment.BotRight,
                (o, e) => screen.Save()),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_file), "Edit .qua File (CTRL+Q)", -48, Alignment.BotRight,
                (o, e) => MapManager.Selected.Value.OpenFile()),

            new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_open_folder), "Open Mapset Folder (CTRL+W)", -48, Alignment.BotRight,
                (o, e) => MapManager.Selected.Value.OpenFolder()),
        }) => Screen = screen;
    }
}
