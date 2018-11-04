using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Parsers;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics.Notifications;
using Quaver.Helpers;
using Quaver.Modifiers;
using Quaver.Screens.Edit.Input;
using Quaver.Screens.Edit.Rulesets;
using Quaver.Screens.Edit.Rulesets.Keys;
using Quaver.Screens.Edit.UI;
using Quaver.Screens.Menu;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Wobble;
using Wobble.Audio;
using Wobble.Bindables;
using Wobble.Discord;
using Wobble.Screens;

namespace Quaver.Screens.Edit
{
    public class EditorScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Edit;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     Reference to the map that is currently being edited.
        /// </summary>
        public Qua Map { get; private set; }

        /// <summary>
        ///     The last saved version of the map.
        /// </summary>
        public Qua LastSavedMap { get; private set; }

        /// <summary>
        ///     The currently selected beat snap.
        /// </summary>
        public BindableInt BeatSnap { get; private set; }

        /// <summary>
        ///     The ruleset the editor is for.
        /// </summary>
        public EditorRuleset Ruleset { get; }

        /// <summary>
        ///     Handles input for the editor.
        /// </summary>
        private EditorInputManager InputManager { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="map"></param>
        public EditorScreen(Qua map)
        {
            ModManager.RemoveAllMods();
            LoadAudioTrack();

            Map = map;
            LastSavedMap = ObjectHelper.DeepClone(Map);

            BeatSnap = new BindableInt(4, 1, 48);
            InputManager = new EditorInputManager(this);

            // Select the editor's game mode based on the map's mode.
            switch (Map.Mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    Ruleset = new EditorRulesetKeys(this, Map.Mode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DiscordManager.Client.CurrentPresence.State = "Editing Map";
            DiscordManager.Client.CurrentPresence.Details = Map.ToString();
            DiscordManager.Client.SetPresence(DiscordManager.Client.CurrentPresence);

            View = new EditorScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputManager.HandleInput(gameTime.ElapsedGameTime.TotalMilliseconds);
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BeatSnap.Dispose();
            base.Destroy();
        }

        /// <summary>
        ///     Makes absolutely sure that we actually have an AudioTrack loaded up.
        /// </summary>
        private static void LoadAudioTrack()
        {
            // Load up the track if we don't have it.
            if (AudioEngine.Track == null || AudioEngine.Track.IsDisposed || AudioEngine.Track.IsStopped)
            {
                try
                {
                    AudioEngine.LoadCurrentTrack();
                }
                catch (Exception)
                {
                    QuaverScreenManager.ChangeScreen(new MenuScreen());
                    NotificationManager.Show(NotificationLevel.Error, "A track needs to be loaded in order to use the editor!");
                }
            }

            if (AudioEngine.Track == null)
                throw new AudioEngineException("Cannot use editor if a track isn't loaded.");

            if (AudioEngine.Track.IsPlaying)
                AudioEngine.Track.Pause();
        }

        /// <summary>
        ///     Saves the map.
        /// </summary>
        public void SaveMap()
        {
            var map = MapManager.Selected.Value;

            var path = $"{ConfigManager.SongDirectory}/{map.Directory}/{map.Path}";
            Map.Save(path);

            LastSavedMap = ObjectHelper.DeepClone(Map);

            MapCache.LoadAndSetMapsets();
            NotificationManager.Show(NotificationLevel.Success, "Map has successfully been saved!", (sender, e) => Process.Start(path));
        }

        /// <summary>
        ///     Goes to the editor screen.
        /// </summary>
        public static void Go()
        {
            var map = MapManager.Selected.Value;

            if (map == null)
            {
                NotificationManager.Show(NotificationLevel.Error, "There is currently no selected map to edit!");
                return;
            }

            Qua qua;

            switch (map.Game)
            {
                case MapGame.Quaver:
                    var path = $"{ConfigManager.SongDirectory}/{map.Directory}/{map.Path}";
                    qua = Qua.Parse(path);
                    break;
                case MapGame.Osu:
                    qua = new OsuBeatmap($"{MapManager.OsuSongsFolder}/{map.Directory}/{map.Path}").ToQua();
                    break;
                case MapGame.Etterna:
                    NotificationManager.Show(NotificationLevel.Error, "Etterna maps aren't eligible to be edited");
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (qua != null)
            {
                QuaverScreenManager.ChangeScreen(new EditorScreen(qua));
            }
            else
                NotificationManager.Show(NotificationLevel.Error, "An error ocurred when trying to edit this map.");
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Editing, Map.MapId, "",
            (byte) Map.Mode, Map.ToString(), (long) ModManager.Mods);
    }
}
