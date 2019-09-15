using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Helpers;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.Search;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Selection
{
    public sealed class SelectionScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Select;

        /// <summary>
        ///     Stores the currently available mapsets to play in the screen
        /// </summary>
        public Bindable<List<Mapset>> AvailableMapsets { get; private set; }

        /// <summary>
        ///    The user's search term/query
        /// </summary>
        public Bindable<string> CurrentSearchQuery { get; private set; }

        /// <summary>
        ///     The currently active panel on the left side of the screen
        /// </summary>
        public Bindable<SelectContainerPanel> ActiveLeftPanel { get; private set; }

        /// <summary>
        ///     The currently active scroll container on the right-side of the screen
        /// </summary>
        public Bindable<SelectScrollContainerType> ActiveScrollContainer { get; private set; }

        /// <summary>
        /// </summary>
        private Random Rng { get; } = new Random();

        /// <summary>
        ///     Invoked when a random mapset has been selected
        /// </summary>
        public static event EventHandler<RandomMapsetSelectedEventArgs> RandomMapsetSelected;

        /// <summary>
        ///     If the user is currently exporting a mapset
        /// </summary>
        private bool IsExportingMapset { get; set; }

        /// <summary>
        /// </summary>
        public SelectionScreen()
        {
            InitializeSearchQueryBindable();
            InitializeAvailableMapsetsBindable();
            InitializeActiveLeftPanelBindable();
            InitializeActiveScrollContainerBindable();

            View = new SelectionScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            FadeAudioTrackIn();
            base.OnFirstUpdate();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            CurrentSearchQuery?.Dispose();
            AvailableMapsets?.Dispose();
            ActiveLeftPanel?.Dispose();
            ActiveScrollContainer?.Dispose();
            RandomMapsetSelected = null;

            base.Destroy();
        }

        /// <summary>
        ///     Initializes the bindable which stores the user's search query <see cref="CurrentSearchQuery"/>
        /// </summary>
        private void InitializeSearchQueryBindable()
            => CurrentSearchQuery = new Bindable<string>(null) { Value = FilterPanelSearchBox.PreviousSearchTerm };

        /// <summary>
        ///     Initializes the bindable which stores the available mapsets for the screen <see cref="AvailableMapsets"/>
        /// </summary>
        private void InitializeAvailableMapsetsBindable() => AvailableMapsets = new Bindable<List<Mapset>>(null)
        {
            Value = MapsetHelper.OrderMapsetsByConfigValue(MapsetHelper.SearchMapsets(MapManager.Mapsets, CurrentSearchQuery.Value))
        };

        /// <summary>
        ///     Initializes the bindable which keeps track of which panel on the left side of the screen is active
        /// </summary>
        private void InitializeActiveLeftPanelBindable()
        {
            ActiveLeftPanel = new Bindable<SelectContainerPanel>(SelectContainerPanel.Leaderboard)
            {
                Value = SelectContainerPanel.Leaderboard
            };
        }

        /// <summary>
        ///     Initializes the bindable which keeps track of which scroll container is active
        /// </summary>
        private void InitializeActiveScrollContainerBindable()
        {
            ActiveScrollContainer = new Bindable<SelectScrollContainerType>(SelectScrollContainerType.Mapsets)
            {
                Value = SelectScrollContainerType.Mapsets
            };
        }

        /// <summary>
        ///     Handles all input for the screen
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleInput(GameTime gameTime)
        {
            if (DialogManager.Dialogs.Count != 0)
                return;

            HandleKeyPressEscape();
            HandleKeyPressF1();
            HandleKeyPressF2();
            HandleKeyPressF3();
            HandleKeyPressEnter();
            HandleKeyPressControlInput();
            HandleRightMouseButtonClick();
        }

        /// <summary>
        ///     Handles when the user presses escape
        /// </summary>
        private void HandleKeyPressEscape()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                return;

            switch (ActiveLeftPanel.Value)
            {
                case SelectContainerPanel.Leaderboard:
                    if (ActiveScrollContainer.Value == SelectScrollContainerType.Maps)
                    {
                        ActiveScrollContainer.Value = SelectScrollContainerType.Mapsets;
                        return;
                    }

                    // TODO: Handle for multiplayer
                    Exit(() => new MenuScreen());
                    break;
                case SelectContainerPanel.Modifiers:
                    ActiveLeftPanel.Value = SelectContainerPanel.Leaderboard;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     Handles when the user presses F1
        /// </summary>
        private void HandleKeyPressF1()
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.F1) && ActiveLeftPanel.Value != SelectContainerPanel.Modifiers)
                ActiveLeftPanel.Value = SelectContainerPanel.Modifiers;
        }

        /// <summary>
        ///     Handles random map selection through key press
        /// </summary>
        private void HandleKeyPressF2()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.F2))
                return;

            SelectRandomMap();
        }

        /// <summary>
        ///    Handles exporting mapsets through F3 key press
        /// </summary>
        private void HandleKeyPressF3()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.F3))
                return;

            ExportSelectedMapset();
        }

        /// <summary>
        ///     Handles when the user presses the enter key
        /// </summary>
        private void HandleKeyPressEnter()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Enter))
                return;

            switch (ActiveScrollContainer.Value)
            {
                case SelectScrollContainerType.Mapsets:
                    ActiveScrollContainer.Value = SelectScrollContainerType.Maps;
                    break;
                case SelectScrollContainerType.Maps:
                    ExitToGameplay();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Handles when the user holds control down and performs input actions
        /// </summary>
        private void HandleKeyPressControlInput()
        {
            if (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) &&
                !KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                return;

            if (OnlineManager.CurrentGame != null)
                return;

            // Increase rate.
            if (KeyboardManager.IsUniqueKeyPress(Keys.OemPlus) || KeyboardManager.IsUniqueKeyPress(Keys.Add))
                ModManager.AddSpeedMods(GetNextRate(true));

            // Decrease Rate
            if (KeyboardManager.IsUniqueKeyPress(Keys.OemMinus) || KeyboardManager.IsUniqueKeyPress(Keys.Subtract))
                ModManager.AddSpeedMods(GetNextRate(false));

            // Change from pitched to non-pitched
            if (KeyboardManager.IsUniqueKeyPress(Keys.D0))
                ConfigManager.Pitched.Value = !ConfigManager.Pitched.Value;
        }

        /// <summary>
        ///     Handles when the user clicks the right mouse button.
        ///     If the user has the maps container open, this'll bring back the mapset container
        /// </summary>
        private void HandleRightMouseButtonClick()
        {
            if (!MouseManager.IsUniqueClick(MouseButton.Right))
                return;

            if (ActiveScrollContainer.Value != SelectScrollContainerType.Maps)
                return;

            var view = (SelectionScreenView) View;

            if (!view.MapContainer.IsHovered())
                return;

            ActiveScrollContainer.Value = SelectScrollContainerType.Mapsets;
        }

        /// <summary>
        ///     Gets the adjacent rate value.
        ///
        ///     For example, if the current rate is 1.0x, the adjacent value would be either 0.95x or 1.1x,
        ///     depending on the argument.
        /// </summary>
        /// <param name="faster">If true, returns the higher rate, otherwise the lower rate.</param>
        /// <returns></returns>
        private static float GetNextRate(bool faster)
        {
            var current = ModHelper.GetRateFromMods(ModManager.Mods);
            var adjustment = 0.1f;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (current < 1.0f || (current == 1.0f && !faster))
                adjustment = 0.05f;

            var next = current + adjustment * (faster ? 1f : -1f);
            return (float) Math.Round(next, 2);
        }

        /// <summary>
        ///     Fades the track back to the config setting
        /// </summary>
        private void FadeAudioTrackIn()
        {
            if (ConfigManager.VolumeMusic == null)
                return;

            if (AudioEngine.Track != null && AudioEngine.Track.IsPlaying)
                AudioEngine.Track?.Fade(ConfigManager.VolumeMusic.Value, 500);
        }

        /// <summary>
        ///     Selects a random map
        /// </summary>
        public void SelectRandomMap()
        {
            if (AvailableMapsets.Value.Count == 0)
                return;

            ActiveScrollContainer.Value = SelectScrollContainerType.Mapsets;

            var index = Rng.Next(AvailableMapsets.Value.Count);
            var mapIndex = Rng.Next(AvailableMapsets.Value[index].Maps.Count);

            MapManager.Selected.Value = AvailableMapsets.Value[index].Maps[mapIndex];
            RandomMapsetSelected?.Invoke(this, new RandomMapsetSelectedEventArgs(AvailableMapsets.Value[index], index));
        }

        /// <summary>
        ///     Exits the screen to gameplay
        /// </summary>
        public void ExitToGameplay()
        {
            if (MapManager.Selected.Value == null)
                return;

            Exit(() => new MapLoadingScreen(new List<Score>()));
        }

        /// <summary>
        ///     Exports the current mapset to a zip file and opens it in the file manager
        /// </summary>
        public void ExportSelectedMapset()
        {
            if (MapManager.Selected.Value == null)
                return;

            if (IsExportingMapset)
            {
                NotificationManager.Show(NotificationLevel.Warning, "Slow down! You must wait for your previous mapset to export");
                return;
            }

            IsExportingMapset = true;

            ThreadScheduler.Run(() =>
            {
                NotificationManager.Show(NotificationLevel.Info, "Exporting mapset to zip archive. Please wait!");

                MapManager.Selected.Value.Mapset.ExportToZip();
                IsExportingMapset = false;

                NotificationManager.Show(NotificationLevel.Success,
                    $"Successfully exported {MapManager.Selected.Value.Mapset.Artist} - {MapManager.Selected.Value.Mapset.Title}!");
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Selecting, -1, "", 0, "", 0);
    }
}