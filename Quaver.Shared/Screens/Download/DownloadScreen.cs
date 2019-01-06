/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using Quaver.API.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Screens.Download.UI.Drawable;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.Select.UI.Mapsets;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Download
{
    public class DownloadScreen : QuaverScreen
    {
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Download;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     The game mode that we're going to search for.
        /// </summary>
        public GameMode CurrentGameMode { get; set; } = GameMode.Keys4;

        /// <summary>
        ///     The ranked status that we're going to search for
        /// </summary>
        public RankedStatus CurrentRankedStatus { get; set; } = RankedStatus.Ranked;

        /// <summary>
        ///     The current page we are on in the search.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        ///     The amount of mapsets that were last searched.
        ///     If 0, none will be searched for in infinite scroll.
        /// </summary>
        public int LastSearchedCount { get; set; }

        /// <summary>
        ///     The currently selected mapset that the user has clicked on to download
        /// </summary>
        public Bindable<DownloadableMapset> SelectedMapset { get; } = new Bindable<DownloadableMapset>(null);

        /// <summary>
        ///     Temporary cache of mapset banners throughout the game's life.
        /// </summary>
        public static Dictionary<int, Texture2D> MapsetBanners { get; } = new Dictionary<int, Texture2D>();

        /// <summary>
        /// </summary>
        public DownloadScreen() => View = new DownloadScreenView(this);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            // Search automatically upon entering the screen
            var view = View as DownloadScreenView;
            view?.SearchBox.SearchForMapsets("", CurrentGameMode, CurrentRankedStatus, false);

            base.OnFirstUpdate();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleInput(GameTime gameTime)
        {
            if (Exiting || DialogManager.Dialogs.Count != 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Exit(() => new MenuScreen());
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;
    }
}