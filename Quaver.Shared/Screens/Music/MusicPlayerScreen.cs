using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Settings;
using Quaver.Shared.Discord;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.Search;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Music
{
    public sealed class MusicPlayerScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Music;

        /// <summary>
        /// </summary>
        public Bindable<string> CurrentSearchQuery { get; set; }

        /// <summary>
        /// </summary>
        public Bindable<List<Mapset>> AvailableSongs { get; set; }

        /// <summary>
        /// </summary>
        public MusicPlayerScreen()
        {
            if (OnlineManager.IsSpectatingSomeone)
                OnlineManager.Client?.StopSpectating();

            // Import any maps in the queue
            if (MapsetImporter.Queue.Count > 0 || QuaverSettingsDatabaseCache.OutdatedMaps.Count != 0
                                               || MapDatabaseCache.MapsToUpdate.Count != 0)
            {
                Exit(() => new ImportingScreen());
                return;
            }

            ModManager.RemoveSpeedMods();

            InitializeSearchQueryBindable();
            InitializeAvailableSongsBindable();

            MapManager.Selected.ValueChanged += OnMapChanged;

            if (ConfigManager.AutoLoadOsuBeatmaps != null)
                ConfigManager.AutoLoadOsuBeatmaps.ValueChanged += OnAutoLoadOsuBeatmapsChanged;

            View = new MusicPlayerScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            if (ConfigManager.AutoLoadOsuBeatmaps != null)
                ConfigManager.AutoLoadOsuBeatmaps.ValueChanged -= OnAutoLoadOsuBeatmapsChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void HandleInput()
        {
            if (Exiting)
                return;

            if (DialogManager.Dialogs.Count != 0)
                return;

            HandleKeyPressEscape();
        }

        /// <summary>
        /// </summary>
        public void ExitToMenu()
        {
            DiscordHelper.Presence.PartySize = 0;
            DiscordHelper.Presence.PartyMax = 0;
            DiscordHelper.UpdatePresence();

            Exit(() => new MainMenuScreen());
        }

        /// <summary>
        ///     Initializes the bindable which stores the user's search query <see cref="CurrentSearchQuery"/>
        /// </summary>
        private void InitializeSearchQueryBindable() => CurrentSearchQuery = new Bindable<string>(null)
        {
            Value = FilterPanelSearchBox.PreviousSearchTerm
        };

        /// <summary>
        ///     Initializes the bindable which stores the available mapsets for the screen <see cref="AvailableSongs"/>
        /// </summary>
        private void InitializeAvailableSongsBindable()
            => AvailableSongs = new Bindable<List<Mapset>>(null) { Value = new List<Mapset>() };

        /// <summary>
        /// </summary>
        private void HandleKeyPressEscape()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                return;

            ExitToMenu();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus()
        {
            if (MapManager.Selected.Value == null)
                return new UserClientStatus(ClientStatus.Listening, -1, "-1", 1, "", 0);

            return new UserClientStatus(ClientStatus.Listening, MapManager.Selected.Value.MapId, MapManager.Selected.Value.Md5Checksum,
                (byte)MapManager.Selected.Value.Mode, $"{MapManager.Selected.Value.Artist} - {MapManager.Selected.Value.Title}", 0);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            OnlineManager.Client?.UpdateClientStatus(GetClientStatus());
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAutoLoadOsuBeatmapsChanged(object sender, BindableValueChangedEventArgs<bool> e)
            => Exit(() => new ImportingScreen());
    }
}