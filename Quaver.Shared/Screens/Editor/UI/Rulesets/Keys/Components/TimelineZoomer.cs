/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Components
{
    public class TimelineZoomer : Sprite
    {
        /// <summary>
        /// </summary>
        private EditorControlButton ZoomIn { get; }

        /// <summary>
        /// </summary>
        private EditorControlButton ZoomOut { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public TimelineZoomer()
        {
            Size = new ScalableVector2(44, 70);
            Tint = Color.Black;
            Alpha = 0.75f;

            ZoomIn = new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_zoom_in), "Zoom Timeline In", 50, Alignment.TopLeft)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 8,
                Size = new ScalableVector2(Width / 2, Width / 2)
            };

            ZoomIn.Clicked += (o, e) => ConfigManager.EditorScrollSpeedKeys.Value++;

            ZoomOut = new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_zoom_out), "Zoom Timeline Out", -50, Alignment.BotLeft)
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Y = -ZoomIn.Y,
                Size = ZoomIn.Size
            };

            ZoomOut.Clicked += (o, e) => ConfigManager.EditorScrollSpeedKeys.Value--;

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Width, 2),
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(2, Height),
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Width, 2),
            };
        }
    }
}