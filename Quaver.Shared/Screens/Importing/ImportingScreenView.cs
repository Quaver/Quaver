/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Visualizer;
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
        private ProgressBar ImportProgressBar { get; set; }

        /// <summary>
        /// </summary>
        public SpriteTextPlus Status { get; private set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Details { get; set; }

        /// <summary>
        /// </summary>
        private string CurrentAction { get; set; } = "Performing Initial Processing";

        /// <summary>
        /// </summary>
        private string CurrentDetails { get; set; } = "Gathering import tasks";

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
            CreateDetailsText();
            CreateProgressBar();
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
            => SetProgress(new ImportProgressEventArgs("Importing Queued Mapsets", $"Importing: {e.FileName}", e.Index + 1, e.Queue.Count));

        /// <summary>
        ///     Updates the progress display from the import worker thread.
        /// </summary>
        /// <param name="progress"></param>
        public void SetProgress(ImportProgressEventArgs progress)
        {
            Status.ScheduleUpdate(() =>
            {
                CurrentAction = GetActionText(progress);
                CurrentDetails = progress.Details;

                SetTextAndTruncate(Status, CurrentAction.ToUpper(), (int) ImportProgressBar.Width - 24);
                SetTextAndTruncate(Details, CurrentDetails, (int) Banner.Width - 120);

                ImportProgressBar.Bindable.Value = progress.Percentage;
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        private static string GetActionText(ImportProgressEventArgs progress)
        {
            if (!progress.HasProgress)
                return progress.Status;

            return $"[{progress.Percentage:0}%] {progress.Status}";
        }

        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <param name="maxWidth"></param>
        private static void SetTextAndTruncate(SpriteTextPlus text, string value, int maxWidth)
        {
            text.Text = value;

            if (!string.IsNullOrEmpty(text.Text) && text.Width > maxWidth)
                text.TruncateWithEllipsis(maxWidth);
        }

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
                Size = new ScalableVector2(WindowManager.Width + 4, 240),
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
            Status = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), CurrentAction.ToUpper(), 22, false)
            {
                Parent = ImportProgressBar,
                Alignment = Alignment.MidCenter,
                Tint = Color.White
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDetailsText()
        {
            Details = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBold), CurrentDetails, 21, false)
            {
                Parent = Banner,
                Alignment = Alignment.TopCenter,
                Y = PleaseWaitText.Y + PleaseWaitText.Height + 18,
                Tint = Color.LightGray
            };
        }

        /// <summary>
        /// </summary>
        private void CreateProgressBar()
        {
            var width = Math.Min(WindowManager.Width * 0.58f, 760);

            ImportProgressBar = new ProgressBar(new Vector2(width, 36), 0, 100, 0, ColorHelper.HexToColor("#151515"), ColorHelper.HexToColor("#128AA4"))
            {
                Parent = Banner,
                Alignment = Alignment.TopCenter,
                Y = Details.Y + Details.Height + 14
            };

            ImportProgressBar.ActiveBar.Alpha = 0.8f;
            ImportProgressBar.AddBorder(ColorHelper.HexToColor("#45D6F5"), 2);
        }

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel()
        {
            LoadingWheel = new LoadingWheel
            {
                Parent = Banner,
                Alignment = Alignment.TopCenter,
                Y = ImportProgressBar.Y + ImportProgressBar.Height + 22,
                Size = new ScalableVector2(42, 42)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateVisualizers()
        {
            VisualizerTop = new MenuAudioVisualizer((int)WindowManager.Width, 750, 220, 3, 8)
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

            VisualizerBottom = new MenuAudioVisualizer((int)WindowManager.Width, 750, 220, 3, 8)
            {
                Parent = Banner,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(WindowManager.Width, 2),
                SpriteRotation = MathF.PI
            };

            VisualizerBottom.Bars.ForEach(x =>
            {
                x.Alignment = Alignment.TopLeft;
                x.Alpha = 1;
                x.Tint = Banner.Border.Tint;
                x.Rotation = MathF.PI;
            });
        }
    }
}
