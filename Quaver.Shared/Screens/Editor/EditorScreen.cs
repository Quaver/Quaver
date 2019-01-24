/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Helpers;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Editor.UI.Dialogs;
using Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata;
using Quaver.Shared.Screens.Editor.UI.Rulesets;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.Select;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using YamlDotNet.Serialization;

namespace Quaver.Shared.Screens.Editor
{
    public sealed class EditorScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Editor;

        /// <summary>
        ///    The original map that the user wants to edit.
        /// </summary>
        public Qua OriginalMap { get; }

        /// <summary>
        ///    The version of the map that is currently being worked on.
        /// </summary>
        public Qua WorkingMap { get; }

        /// <summary>
        ///     The game mode/ruleset used for the editor.
        /// </summary>
        public EditorRuleset Ruleset { get; private set; }

        /// <summary>
        /// </summary>
        public BindableInt BeatSnap { get; } = new BindableInt(4, 1, 16);

        /// <summary>
        ///     All of the available beat snaps to use in the editor.
        /// </summary>
        private List<int> AvailableBeatSnaps { get; } = new List<int> {1, 2, 3, 4, 6, 8, 12, 16};

        /// <summary>
        /// </summary>
        private int BeatSnapIndex => AvailableBeatSnaps.FindIndex(x => x == BeatSnap.Value);

        /// <summary>
        ///     The index of the object who had its hitsounds played.
        /// </summary>
        private int HitSoundObjectIndex { get; set; }

        /// <summary>
        ///     If we're currently in a background change dialog.
        ///     Prevents the user from dragging in multiple files.
        /// </summary>
        public bool InBackgroundConfirmationDialog { get; set; }

        /// <summary>
        ///     Watches the .qua file to detect any outside changes made to it.
        /// </summary>
        private FileSystemWatcher FileWatcher { get; set; }

        /// <summary>
        ///     Detects if a save is currently happening.
        /// </summary>
        private bool SaveInProgress { get; set; }

        /// <summary>
        ///     The time the file was last saved.
        /// </summary>
        private long LastSaveTime { get; set; }

        /// <summary>
        /// </summary>
        public EditorScreen(Qua map)
        {
            OriginalMap = map;
            WorkingMap = ObjectHelper.DeepClone(OriginalMap);

            MapManager.Selected.Value.Qua = WorkingMap;
            DiscordHelper.Presence.Details = WorkingMap.ToString();
            DiscordHelper.Presence.State = "Editing";
            DiscordHelper.Presence.StartTimestamp = (long) (TimeHelper.GetUnixTimestampMilliseconds() / 1000);
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);

            if (!LoadAudioTrack())
                return;

            SetHitSoundObjectIndex();
            CreateRuleset();

            GameBase.Game.IsMouseVisible = true;
            GameBase.Game.GlobalUserInterface.Cursor.Visible = false;

            GameBase.Game.Window.FileDropped += OnFileDropped;
            BeginWatchingFiles();

            View = new EditorScreenView(this);
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PlayHitsounds();

            if (AudioEngine.Track.IsDisposed)
                AudioEngine.LoadCurrentTrack();

            if (DialogManager.Dialogs.Count == 0)
                HandleInput(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            // In the event that the map the user created is new, show them the metadata dialog.
            if (MapManager.Selected.Value.NewlyCreated)
            {
                DialogManager.Show(new EditorMetadataDialog(this));
                MapManager.Selected.Value.NewlyCreated = false;
            }

            // User created a new difficulty, so prompt them if they would like to delete all objects.
            if (MapManager.Selected.Value.AskToRemoveHitObjects)
            {
                DialogManager.Show(new ConfirmCancelDialog("Do you want to remove all HitObjects?", (o, e) =>
                {
                    WorkingMap.HitObjects.Clear();

                    var ruleset = Ruleset as EditorRulesetKeys;
                    ruleset?.ScrollContainer.HitObjects.ForEach(x => x.Destroy());
                    ruleset?.ScrollContainer.HitObjects.Clear();
                    DialogManager.Dismiss();
                }));

                MapManager.Selected.Value.AskToRemoveHitObjects = false;

            }

            base.OnFirstUpdate();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            GameBase.Game.Window.FileDropped -= OnFileDropped;
            FileWatcher.Dispose();
            BeatSnap.Dispose();
            base.Destroy();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleInput(GameTime gameTime)
        {
            if (Exiting || DialogManager.Dialogs.Count != 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                HandleKeyPressEscape();

            if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyEditorPausePlay.Value))
                HandleKeyPressSpace();

            if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyEditorDecreaseAudioRate.Value))
                ChangeAudioPlaybackRate(Direction.Backward);

            if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyEditorIncreaseAudioRate.Value))
                ChangeAudioPlaybackRate(Direction.Forward);

            if (KeyboardManager.IsUniqueKeyPress(Keys.F1))
                DialogManager.Show(new EditorMetadataDialog(this));

            // Timing Setup
            if (KeyboardManager.IsUniqueKeyPress(Keys.F2))
                NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet.");

            // Scroll Velocities
            if (KeyboardManager.IsUniqueKeyPress(Keys.F3))
                NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet.");

            if (KeyboardManager.IsUniqueKeyPress(Keys.F4))
                ChangePreviewTime((int) AudioEngine.Track.Time);

            HandleAudioSeeking();
            HandleCtrlInput(gameTime);
            HandleBeatSnapChanges();
        }

        /// <summary>
        ///     Changes the audio playback rate either up or down.
        /// </summary>
        /// <param name="direction"></param>
        private void ChangeAudioPlaybackRate(Direction direction)
        {
            float targetRate;

            switch (direction)
            {
                case Direction.Forward:
                    targetRate = AudioEngine.Track.Rate + 0.25f;
                    break;
                case Direction.Backward:
                    targetRate = AudioEngine.Track.Rate - 0.25f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (targetRate <= 0 || targetRate > 2.0f)
            {
                NotificationManager.Show(NotificationLevel.Error, "You cannot change the audio rate this way any further!");
                return;
            }

            var playAfterRateChange = false;

            if (AudioEngine.Track.IsPlaying)
            {
                AudioEngine.Track.Pause();
                playAfterRateChange = true;
            }

            AudioEngine.Track.Rate = targetRate;

            if (Ruleset is EditorRulesetKeys ruleset)
                ruleset.ScrollContainer.ResetObjectPositions();

            if (AudioEngine.Track.IsPaused && playAfterRateChange)
                AudioEngine.Track.Play();

            NotificationManager.Show(NotificationLevel.Info, $"Audio playback rate changed to: {targetRate * 100}%");
        }

        /// <summary>
        /// </summary>
        private void CreateRuleset()
        {
            switch (WorkingMap.Mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    Ruleset = new EditorRulesetKeys(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Attempts to load the audio track for the current map.
        ///     If it can't, it'll send the user back to the menu screen.
        /// </summary>
        /// <returns></returns>
        private bool LoadAudioTrack()
        {
            try
            {
                if (AudioEngine.Track != null && AudioEngine.Track.IsPaused)
                    return true;

                AudioEngine.LoadCurrentTrack();
                return true;
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, "Audio track was unable to be loaded for this map.");
                Exit(() => new MenuScreen());
                return false;
            }
        }

        /// <summary>
        /// </summary>
        public void HandleKeyPressEscape()
        {
            if (SaveInProgress)
            {
                NotificationManager.Show(NotificationLevel.Error, "Please wait until your map has finished saving before exiting!");
                return;
            }

            Exit(() =>
            {
                GameBase.Game.IsMouseVisible = false;
                GameBase.Game.GlobalUserInterface.Cursor.Visible = true;

                DiscordHelper.Presence.StartTimestamp = 0;
                DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);

                if (AudioEngine.Track != null)
                    AudioEngine.Track.Rate = 1.0f;

                AudioEngine.Track?.Fade(0, 100);

                return new SelectScreen();
            });
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressSpace() => PlayPauseTrack();

        /// <summary>
        /// </summary>
        /// <param name="direction"></param>
        public void ChangeBeatSnap(Direction direction)
        {
            var index = BeatSnapIndex;

            switch (direction)
            {
                case Direction.Forward:
                    BeatSnap.Value = index + 1 < AvailableBeatSnaps.Count ? AvailableBeatSnaps[index + 1] : AvailableBeatSnaps.First();
                    break;
                case Direction.Backward:
                    BeatSnap.Value = index - 1 >= 0 ? AvailableBeatSnaps[index - 1] : AvailableBeatSnaps.Last();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>
        ///     Handles seeking through the audio whether with the scroll wheel or
        ///     arrow keys
        /// </summary>
        private void HandleAudioSeeking()
        {
            if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsDisposed || KeyboardManager.CurrentState.IsKeyDown(Keys.LeftShift)
                || KeyboardManager.CurrentState.IsKeyDown(Keys.RightShift))
                return;

            // Seek backwards
            if (KeyboardManager.IsUniqueKeyPress(Keys.Left) || MouseManager.CurrentState.ScrollWheelValue >
                MouseManager.PreviousState.ScrollWheelValue)
            {
                AudioEngine.SeekTrackToNearestSnap(WorkingMap, Direction.Backward, BeatSnap.Value);
                SetHitSoundObjectIndex();
            }
            // Seek Forwards
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Right) || MouseManager.CurrentState.ScrollWheelValue <
                MouseManager.PreviousState.ScrollWheelValue)
            {
                AudioEngine.SeekTrackToNearestSnap(WorkingMap, Direction.Forward, BeatSnap.Value);
                SetHitSoundObjectIndex();
            }
        }

        /// <summary>
        ///     Handles all input when the user is holding down CTRL
        /// </summary>
        private void HandleCtrlInput(GameTime gameTime)
        {
            if (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) &&
                !KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.S))
                Save();

            if (KeyboardManager.IsUniqueKeyPress(Keys.Z))
                Ruleset.ActionManager.Undo();

            if (KeyboardManager.IsUniqueKeyPress(Keys.Y))
                Ruleset.ActionManager.Redo();

            if (KeyboardManager.IsUniqueKeyPress(Keys.T))
                MapManager.Selected.Value.VisitMapsetPage();

            if (KeyboardManager.IsUniqueKeyPress(Keys.U))
                NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet");

            if (KeyboardManager.IsUniqueKeyPress(Keys.E))
                ExportToZip();

            if (KeyboardManager.IsUniqueKeyPress(Keys.Q))
                MapManager.Selected.Value.OpenFile();

            if (KeyboardManager.IsUniqueKeyPress(Keys.W))
                MapManager.Selected.Value.OpenFolder();
        }

        /// <summary>
        ///     Handles changing the beat snap with the scroll wheel + shift
        ///     and arrow keys + shift.
        /// </summary>
        private void HandleBeatSnapChanges()
        {
            if (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftShift) && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightShift))
                return;

            if (MouseManager.CurrentState.ScrollWheelValue > MouseManager.PreviousState.ScrollWheelValue || KeyboardManager.IsUniqueKeyPress(Keys.Up))
                ChangeBeatSnap(Direction.Forward);

            if (MouseManager.CurrentState.ScrollWheelValue < MouseManager.PreviousState.ScrollWheelValue || KeyboardManager.IsUniqueKeyPress(Keys.Down))
                ChangeBeatSnap(Direction.Backward);
        }

        /// <summary>
        ///     Completely stops the AudioTrack.
        /// </summary>
        public static void StopTrack()
        {
            if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsDisposed)
                return;

            if (AudioEngine.Track.IsPlaying)
                AudioEngine.Track.Pause();

            AudioEngine.Track.Seek(0);
            AudioEngine.Track.Stop();
        }

        /// <summary>
        ///     Pauses/Plays the AudioTrack.
        /// </summary>
        public void PlayPauseTrack()
        {
            if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsDisposed)
            {
                AudioEngine.LoadCurrentTrack();
                SetHitSoundObjectIndex();

                AudioEngine.Track.Play();
            }
            else if (AudioEngine.Track.IsPlaying)
                AudioEngine.Track.Pause();
            else if (AudioEngine.Track.IsPaused)
                AudioEngine.Track.Play();
        }

        /// <summary>
        ///     Restarts the audio track from the beginning
        /// </summary>
        public void RestartTrack()
        {
            if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsDisposed)
            {
                AudioEngine.LoadCurrentTrack();
                SetHitSoundObjectIndex();

                AudioEngine.Track.Play();
            }
            else if (AudioEngine.Track.IsPlaying)
            {
                AudioEngine.Track.Pause();
                AudioEngine.Track.Seek(0);
                SetHitSoundObjectIndex();

                AudioEngine.Track.Play();
            }
            else if (AudioEngine.Track.IsPaused)
            {
                AudioEngine.Track.Seek(0);
                SetHitSoundObjectIndex();

                AudioEngine.Track.Play();
            }
        }

        /// <summary>
        ///     Keeps track of and plays object hitsounds.
        /// </summary>
        private void PlayHitsounds()
        {
            for (var i = HitSoundObjectIndex; i < WorkingMap.HitObjects.Count; i++)
            {
                if (Exiting)
                    return;

                var obj = WorkingMap.HitObjects[i];

                if (AudioEngine.Track.Time >= obj.StartTime)
                {
                    if (ConfigManager.EditorEnableHitsounds.Value)
                        HitObjectManager.PlayObjectHitSounds(obj);

                    HitSoundObjectIndex = i + 1;
                }
                else
                    break;
            }
        }

        /// <summary>
        ///     Sets the hitsounds object index, so we know which object to play sounds for.
        ///     This is generally used when seeking through the map.
        /// </summary>
        public void SetHitSoundObjectIndex()
        {
            HitSoundObjectIndex = WorkingMap.HitObjects.FindLastIndex(x => x.StartTime <= AudioEngine.Track.Time);
            HitSoundObjectIndex++;
        }

        /// <summary>
        ///     Saves the map
        /// </summary>
        public void Save()
        {
            if (MapManager.Selected.Value.Game != MapGame.Quaver)
            {
                NotificationManager.Show(NotificationLevel.Error, "You cannot save a map loaded from another game.");
                return;
            }

            if (SaveInProgress)
            {
                NotificationManager.Show(NotificationLevel.Error, "Slow down! We're already saving your map.");
                return;
            }

            if (!MapDatabaseCache.MapsToUpdate.Contains(MapManager.Selected.Value))
                MapDatabaseCache.MapsToUpdate.Add(MapManager.Selected.Value);

            ThreadScheduler.Run(() =>
            {
                var path = $"{ConfigManager.SongDirectory}/{MapManager.Selected.Value.Directory}/{MapManager.Selected.Value.Path}";

                SaveInProgress = true;
                WorkingMap.Save(path);
                SaveInProgress = false;
                LastSaveTime = GameBase.Game.TimeRunning;

                NotificationManager.Show(NotificationLevel.Success, "Successfully saved the map.");
            });
        }

        /// <summary>
        ///    Changes the audio preview time of the map.
        /// </summary>
        public void ChangePreviewTime(int time) => Ruleset.ActionManager.SetPreviewTime(WorkingMap, time);

        /// <summary>
        ///     Called when a file is dropped into the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="file"></param>
        private void OnFileDropped(object sender, string file)
        {
            if (MapManager.Selected.Value.Game != MapGame.Quaver)
            {
                NotificationManager.Show(NotificationLevel.Error, "You cannot change the background for a map loaded from another game.");
                return;
            }

            if (InBackgroundConfirmationDialog)
            {
                NotificationManager.Show(NotificationLevel.Error, "Finish what you're doing before importing another background!");
                return;
            }

            var fileLower = file.ToLower();

            if (!fileLower.EndsWith(".png") && !fileLower.EndsWith(".jpg") && !fileLower.EndsWith(".jpeg"))
                return;

            DialogManager.Show(new BackgroundConfirmationDialog(this, file));
        }

        /// <summary>
        /// </summary>
        private void BeginWatchingFiles()
        {
            FileWatcher = new FileSystemWatcher($"{ConfigManager.SongDirectory.Value}/{MapManager.Selected.Value.Directory}")
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.qua"
            };

            var lastRead = DateTime.MinValue;

            FileWatcher.Changed += async (sender, args) =>
            {
                var path = $"{ConfigManager.SongDirectory.Value}/{MapManager.Selected.Value.Directory}/{MapManager.Selected.Value.Path}";

                var lastWriteTime = File.GetLastWriteTime(path);

                if (!ConfigManager.IsFileReady(path) || lastWriteTime == lastRead)
                    return;

                if (args.FullPath.Replace("\\", "/") != path.Replace("\\", "/"))
                    return;

                if (lastWriteTime == lastRead)
                    return;

                lastRead = lastWriteTime;

                if (SaveInProgress)
                    return;

                await Task.Delay(500);

                if (GameBase.Game.TimeRunning - LastSaveTime < 600)
                    return;

                // Only make a new dialog if one isn't already up.
                if (DialogManager.Dialogs.Count == 0 || DialogManager.Dialogs.First().GetType() != typeof(ChangesDetectedConfirmationDialog))
                    DialogManager.Show(new ChangesDetectedConfirmationDialog(this, args.FullPath));
            };

            FileWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        ///     Asks the user if they'd like to create a new difficulty for the mapset,
        ///     and does so.
        /// </summary>
        public void CreateNewDifficulty() => ThreadScheduler.Run(() =>
        {
            Button.IsGloballyClickable = false;

            // Save the already existing map.
            Save();

            var qua = ObjectHelper.DeepClone(WorkingMap);
            qua.DifficultyName = "";
            qua.Description = $"Created at {TimeHelper.GetUnixTimestampMilliseconds()}";

            var dir = $"{ConfigManager.SongDirectory.Value}/{MapManager.Selected.Value.Directory}";
            var path = $"{dir}/{qua.Artist} - {qua.Title} [{qua.DifficultyName}] - {TimeHelper.GetUnixTimestampMilliseconds()}.qua";
            qua.Save(path);

            // Add the new map to the db.
            var map = Map.FromQua(qua, path);
            map.Id = MapDatabaseCache.InsertMap(map, path);

            // Reload the mapsets
            MapDatabaseCache.OrderAndSetMapsets();

            // Set the selected one to the new one.
            MapManager.Selected.Value = map;
            MapManager.Selected.Value.Qua = qua;

            // Find the mapset and get the *new* object w/ the selected map.
            var selectedMapset = MapManager.Mapsets.Find(x => x.Maps.Any(y => y.Id == MapManager.Selected.Value.Id));
            MapManager.Selected.Value = selectedMapset.Maps.Find(x => x.Id == MapManager.Selected.Value.Id);
            MapManager.Selected.Value.Qua = qua;
            MapManager.Selected.Value.NewlyCreated = true;
            MapManager.Selected.Value.AskToRemoveHitObjects = true;

            // Reload editor w/ new one.
            Exit(() => new EditorScreen(qua));
        });

        /// <summary>
        ///     Creates a new mapset with an audio file.
        /// </summary>
        /// <param name="audioFile"></param>
        public static void HandleNewMapsetCreation(string audioFile)
        {
            try
            {
                var game = GameBase.Game as QuaverGame;

                // Add a fade effect and make butotns not clickable
                // so the user can't perform any actions during this time.
                Transitioner.FadeIn();
                Button.IsGloballyClickable = false;

                var tagFile = TagLib.File.Create(audioFile);

                // Create a fresh .qua with the available metadata from the file
                var qua = new Qua()
                {
                    AudioFile = Path.GetFileName(audioFile),
                    Artist = tagFile.Tag.FirstPerformer ?? "",
                    Title = tagFile.Tag.Title ?? "",
                    Source = tagFile.Tag.Album ?? "",
                    Tags = string.Join(" ", tagFile.Tag.Genres) ?? "",
                    Creator = ConfigManager.Username.Value,
                    DifficultyName = "",
                    // Makes the file different to prevent exception thrown in the DB for same md5 checksum
                    Description = $"Created at {TimeHelper.GetUnixTimestampMilliseconds()}",
                    BackgroundFile = "",
                    Mode = GameMode.Keys4,
                    TimingPoints =
                    {
                        new TimingPointInfo()
                        {
                            Bpm = 0,
                            StartTime = 0
                        }
                    },
                    HitObjects =
                    {
                        new HitObjectInfo()
                        {
                            StartTime = 0,
                            Lane = 1
                        }
                    }
                };

                // Create a new directory to house the map.
                var dir = $"{ConfigManager.SongDirectory.Value}/{TimeHelper.GetUnixTimestampMilliseconds()}";
                Directory.CreateDirectory(dir);

                // Copy over the audio file into the directory
                File.Copy(audioFile, $"{dir}/{Path.GetFileName(audioFile)}");

                // Save the new .qua file into the directory
                var path = $"{dir}/{qua.Artist} - {qua.Title} [{qua.DifficultyName}] - {TimeHelper.GetUnixTimestampMilliseconds()}.qua";
                qua.Save(path);

                // Place the new map inside of the database and make sure all the loaded maps are correct
                var map = Map.FromQua(qua, path);
                map.Id = MapDatabaseCache.InsertMap(map, path);
                MapDatabaseCache.OrderAndSetMapsets();

                MapManager.Selected.Value = map;
                MapManager.Selected.Value.Qua = qua;

                var selectedMapset = MapManager.Mapsets.Find(x => x.Maps.Any(y => y.Id == MapManager.Selected.Value.Id));

                // Find the new object from the loaded maps that contains the same id.
                MapManager.Selected.Value = selectedMapset.Maps.Find(x => x.Id == MapManager.Selected.Value.Id);
                MapManager.Selected.Value.Qua = qua;
                MapManager.Selected.Value.NewlyCreated = true;

                game?.CurrentScreen.Exit(() => new EditorScreen(qua));
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);

                var game = GameBase.Game as QuaverGame;

                game?.CurrentScreen.Exit(() =>
                {
                    NotificationManager.Show(NotificationLevel.Error, "Could not create new mapset with that audio file.");
                    return new SelectScreen();
                });
            }
        }

        /// <summary>
        ///     Goes to the gameplay screen to play test.
        /// </summary>
        public void GoPlayTest()
        {
            if (WorkingMap.HitObjects.Count(x => x.StartTime >= AudioEngine.Track.Time) == 0)
            {
                NotificationManager.Show(NotificationLevel.Error, "There aren't any hitobjects to play test past this point!");
                return;
            }

            AudioEngine.Track.Rate = 1.0f;

            Exit(() =>
            {
                Save();
                return new GameplayScreen(WorkingMap, "", new List<Score>(), null, true, AudioEngine.Track.Time);
            });
        }

        /// <summary>
        /// </summary>
        public static void ExportToZip()
        {
            MapManager.Selected.Value.Mapset.ExportToZip();
            ThreadScheduler.RunAfter(() => NotificationManager.Show(NotificationLevel.Success, "Successfully exported mapset!"), 100);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Editing, -1, "", (byte) GameMode.Keys4, WorkingMap.ToString(), 0);
    }
}