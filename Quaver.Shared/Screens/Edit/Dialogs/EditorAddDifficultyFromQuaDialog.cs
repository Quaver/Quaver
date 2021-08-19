using System;
using System.IO;
using Quaver.API.Maps;
using Quaver.Server.Common.Helpers;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorAddDifficultyFromQuaDialog : YesNoDialog
    {
        private EditScreen Screen { get; }

        public EditorAddDifficultyFromQuaDialog(EditScreen screen) : base("ADD DIFFICULTY FROM FILE",
            "Drag a .qua file into the window to add it to the mapset.")
        {
            Screen = screen;

            YesButton.Visible = false;
            YesButton.IsClickable = false;

            NoButton.Alignment = Alignment.BotCenter;
            NoButton.X = 0;

            GameBase.Game.Window.FileDropped += OnFileDropped;
        }

        public override void Destroy()
        {
            GameBase.Game.Window.FileDropped -= OnFileDropped;
            base.Destroy();
        }

        private void OnFileDropped(object sender, string e)
        {
            var file = e.ToLower();

            if (!file.EndsWith(".qua"))
                return;

            Logger.Important($"Importing file: {e} into the mapset.", LogType.Runtime);

            ThreadScheduler.Run(() =>
            {
                Qua qua;

                try
                {
                    qua = Qua.Parse(e);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, $"The .qua file you have dragged in is not valid!");
                    return;
                }

                try
                {
                    var dir = $"{ConfigManager.SongDirectory.Value}/{Screen.Map.Directory}";

                    var path = $"{dir}/{StringHelper.FileNameSafeString(qua.Artist)} - {StringHelper.FileNameSafeString(qua.Title)} " +
                               $"[{StringHelper.FileNameSafeString(qua.DifficultyName)}].qua";

                    File.Copy(e, path);

                    // Add the new map to the db.
                    var map = Map.FromQua(qua, path);
                    map.DateAdded = DateTime.Now;
                    map.Id = MapDatabaseCache.InsertMap(map);
                    map.Mapset = Screen.Map.Mapset;
                    map.NewlyCreated = true;
                    Screen.Map.Mapset.Maps.Add(map);
                    MapManager.Selected.Value = map;

                    if (!MapDatabaseCache.MapsToUpdate.Contains(map))
                        MapDatabaseCache.MapsToUpdate.Add(map);

                    var track = AudioEngine.LoadMapAudioTrack(map);
                    Screen.Exit(() => new EditScreen(map, track));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error,
                        "There was an issue while creating a new difficulty.");
                }
                finally
                {
                    DialogManager.Dismiss(this);
                }
            });
        }
    }
}