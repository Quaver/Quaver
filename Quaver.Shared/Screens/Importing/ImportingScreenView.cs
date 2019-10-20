/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Visualizer;
using Quaver.Shared.Screens.Settings;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Importing
{
    public class ImportingScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Banner { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus PleaseWaitText { get; set; }

        /// <summary>
        /// </summary>
        public SpriteTextPlus Status { get; private set; }

        /// <summary>
        /// </summary>
        private LoadingWheel LoadingWheel { get; set; }

        /// <summary>
        /// </summary>
        private MenuAudioVisualizer VisualizerTop { get; set; }

        /// <summary>
        /// </summary>
        private MenuAudioVisualizer VisualizerBottom { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ImportingScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateBanner();
            CreatePleaseWaitText();
            CreateStatusText();
            CreateLoadingWheel();
            CreateVisualizers();

            MapsetImporter.ImportingMapset += OnImportingMapset;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor($"#2F2F2F"));
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            MapsetImporter.ImportingMapset -= OnImportingMapset;
            Container?.Destroy();
        }

        /// <summary>
        ///     This method will update the screen when a mapset is finished being imported.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnImportingMapset(object sender, ImportingMapsetEventArgs e)
            => Status.ScheduleUpdate(() => Status.Text = $"[{e.Index + 1}/{e.Queue.Count}] IMPORTING: \"{e.FileName}\"");

        /// <summary>
        ///     Creates <see cref="Background"/>
        /// </summary>
        private void CreateBackground()
            => Background = new BackgroundImage(UserInterface.Triangles, 0, false) { Parent = Container };

        /// <summary>
        ///     Creates <see cref="Banner"/>
        /// </summary>
        private void CreateBanner()
        {
            Banner = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(WindowManager.Width + 4, 180),
                Tint = ColorHelper.HexToColor($"#242424")
            };

            Banner.AddBorder(ColorHelper.HexToColor("#45D6F5"), 2);
        }

        /// <summary>
        /// </summary>
        private void CreatePleaseWaitText()
        {
            PleaseWaitText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "Please wait while your maps are being processed!".ToUpper(), 26)
            {
                Parent = Banner,
                Alignment = Alignment.TopCenter,
                Y = 24,
                Tint = ColorHelper.HexToColor("#F2C94C")
            };
        }

        /// <summary>
        /// </summary>
        private void CreateStatusText()
        {
            Status = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "Performing Initial Processing".ToUpper(), 24)
            {
                Parent = Banner,
                Alignment = Alignment.TopCenter,
                Y = PleaseWaitText.Y + PleaseWaitText.Height + 16
            };
        }

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel()
        {
            LoadingWheel = new LoadingWheel
            {
                Parent = Banner,
                Alignment = Alignment.TopCenter,
                Y = Status.Y + Status.Height + 18,
                Size = new ScalableVector2(50, 50)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateVisualizers()
        {
            VisualizerTop = new MenuAudioVisualizer((int) WindowManager.Width, 750, 220, 3, 8)
            {
                Parent = Banner,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(WindowManager.Width, 2)
            };

            VisualizerTop.Bars.ForEach(x =>
            {
                x.Alpha = 1;
                x.Tint = Banner.Border.Tint;
            });

            VisualizerBottom = new MenuAudioVisualizer((int) WindowManager.Width, 750, 220, 3, 8)
            {
                Parent = Banner,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(WindowManager.Width, 2),
                Rotation = 180
            };

            VisualizerBottom.Bars.ForEach(x =>
            {
                x.Alignment = Alignment.TopLeft;
                x.Alpha = 1;
                x.Tint = Banner.Border.Tint;
                x.Rotation = 180;
            });
        }
    }
}
