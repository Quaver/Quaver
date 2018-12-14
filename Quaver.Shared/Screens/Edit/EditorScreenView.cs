/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Edit.UI;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Form;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Edit
{
    public class EditorScreenView : ScreenView
    {
        /// <summary>
        ///     The background image for this screen.
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        ///     The current time in the map.
        /// </summary>
        private EditorSongTimeDisplay TimeDisplay { get; set; }

        /// <summary>
        ///     The bar that controls where in the song to seek to.
        /// </summary>
        private EditorSeekBar SeekBar { get; set; }

        /// <summary>
        ///     Allows the user to select the current beat snap.
        /// </summary>
        private HorizontalSelector BeatSnapSelector { get; set; }

        /// <summary>
        ///     We want to show a notif at the very beginning, this keeps track of if we've already added to the queue
        /// </summary>
        private bool NotificationShown { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorScreenView(Screen screen) : base(screen)
        {
            Background = new BackgroundImage(MapManager.CurrentBackground ?? UserInterface.MenuBackground, 60) { Parent = Container };

            CreateSongTimeDisplay();
            CreateSeekBar();
            CreateBeatSnapSelector();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var screen = (EditorScreen) Screen;
            screen.Ruleset.Update(gameTime);

            Container?.Update(gameTime);

            if (!NotificationShown)
            {
                NotificationManager.Show(NotificationLevel.Warning, "The editor is experimental and does not work!");
                NotificationShown = true;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);

            var screen = (EditorScreen)Screen;
            screen.Ruleset.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            var screen = (EditorScreen)Screen;
            screen.Ruleset.Destroy();

            Container?.Destroy();
        }

        /// <summary>
        ///
        /// </summary>
        private void CreateSongTimeDisplay() => TimeDisplay = new EditorSongTimeDisplay(NumberDisplayType.SongTime, "00:00", new Vector2(0.5f, 0.5f))
        {
            Parent = Container,
            Alignment = Alignment.BotCenter,
            Y = -70,
            X = 0
        };

        /// <summary>
        ///    Creates the bar to seek through the song's progress.
        /// </summary>
        private void CreateSeekBar() => SeekBar = new EditorSeekBar(SeekBarAxis.Horizontal, new Vector2(WindowManager.Width, 30))
        {
            Parent = Container,
            Alignment = Alignment.BotCenter,
        };

        /// <summary>
        ///     Creates the element where users are able to select the current beat snap.
        /// </summary>
        private void CreateBeatSnapSelector()
        {
            var snaps = new List<string> { "1/1", "1/2", "1/3", "1/4", "1/6", "1/8", "1/12", "1/16", "1/32", "1/48" };

            BeatSnapSelector = new HorizontalSelector(snaps, new ScalableVector2(200, 30), BitmapFonts.Exo2Regular, 18,
                                                    FontAwesome.Get(FontAwesomeIcon.fa_chevron_sign_left), FontAwesome.Get(FontAwesomeIcon.fa_right_chevron),
                                                    new ScalableVector2(30, 30), 10, (item, index) =>
                {
                    var screen = (EditorScreen) Screen;
                    screen.BeatSnap.Value = int.Parse(item.Split('/')[1]);
                }, 3)
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                X = 200
            };
        }
    }
}
