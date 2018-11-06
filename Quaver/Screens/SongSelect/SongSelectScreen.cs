using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Modifiers;
using Quaver.Screens.Loading;
using Quaver.Screens.Menu;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Wobble.Graphics;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Screens.SongSelect
{
    public class SongSelectScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Select;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     The previous search term the user searched for.
        ///     Used to persist through screen changes.
        /// </summary>
        public static string PreviousSearchTerm { get; set; } = "";

        /// <summary>
        ///     The mapsets that are currently available to be displayed.
        /// </summary>
        public List<Mapset> AvailableMapsets { get; set; }

        /// <summary>
        /// </summary>
        public SongSelectScreen()
        {
            // Grab the mapsets available to the user according to their previous search term.
            AvailableMapsets = MapsetHelper.SearchMapsets(MapManager.Mapsets, PreviousSearchTerm);

            // If no mapsets were found, just default to all of them.
            if (AvailableMapsets.Count == 0)
                AvailableMapsets = MapManager.Mapsets;

            AvailableMapsets = MapsetHelper.OrderMapsetByConfigValue(AvailableMapsets);

            Logger.Debug($"There are currently: {AvailableMapsets.Count} available mapsets to play in select.", LogType.Runtime);

            View = new SongSelectScreenView(this);
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
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Selecting,
            -1, "", (byte) ConfigManager.SelectedGameMode.Value, "", (long) ModManager.Mods);

        /// <summary>
        ///     Handles all input for the screen.
        /// </summary>
        private void HandleInput()
        {
            var view = View as SongSelectScreenView;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
            {
                switch (view.ActiveContainer)
                {
                    case SelectContainerStatus.Mapsets:
                        QuaverScreenManager.ChangeScreen(new MenuScreen());
                        break;
                    case SelectContainerStatus.Difficulty:
                        view.SwitchToContainer(SelectContainerStatus.Mapsets);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Enter))
            {
                switch (view.ActiveContainer)
                {
                    case SelectContainerStatus.Mapsets:
                        view.SwitchToContainer(SelectContainerStatus.Difficulty);
                        break;
                    case SelectContainerStatus.Difficulty:
                        QuaverScreenManager.ChangeScreen(new MapLoadingScreen(new List<LocalScore>()));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
            {
                switch (view.ActiveContainer)
                {
                    case SelectContainerStatus.Mapsets:
                        view?.MapsetScrollContainer.SelectNextMapset(Direction.Forward);
                        break;
                    case SelectContainerStatus.Difficulty:
                        view.DifficultyScrollContainer.SelectNextDifficulty(Direction.Forward);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
            {
                switch (view.ActiveContainer)
                {
                    case SelectContainerStatus.Mapsets:
                        view?.MapsetScrollContainer.SelectNextMapset(Direction.Backward);
                        break;
                    case SelectContainerStatus.Difficulty:
                        view.DifficultyScrollContainer.SelectNextDifficulty(Direction.Backward);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}