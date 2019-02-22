/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.Menu.UI.Panels
{
    public class PanelContainer : Container
    {
        /// <summary>
        ///     The menu panels.
        /// </summary>
        public List<Panel> Panels { get; }

        /// <summary>
        ///     The X position where
        /// </summary>
        public static int StartingX => 44;

        /// <summary>
        ///
        /// </summary>
        /// <param name="panels"></param>
        public PanelContainer(List<Panel> panels)
        {
            Panels = panels;
            InitializePanels();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            FadePanels(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        ///     Aligns the panels and adds fade in Animations.
        /// </summary>
        private void InitializePanels()
        {
            for (var i = 0; i < Panels.Count; i++)
            {
                var panel = Panels[i];

                panel.Parent = this;
                panel.Alignment = Alignment.MidLeft;
                panel.Y = 30;

                panel.Animations.Add(new Animation(AnimationProperty.X, Easing.OutQuint, 0,
                    StartingX + i * panel.Width + i * 15, 600 + 100 * i));

                panel.Thumbnail.Animations.Add(new Animation(AnimationProperty.Alpha,
                    Easing.OutQuint, 0, 1, 500 + 100 * i));
            }
        }

        /// <summary>
        ///     Performs the fade animation for the panels when one is hovered.
        /// </summary>
        /// <param name="gameTime"></param>
        private void FadePanels(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            Panel hoveredPanel = null;

            // Try to find the hovered panel.
            foreach (var panel in Panels)
            {
                if (!panel.IsHovered)
                    continue;

                hoveredPanel = panel;
                break;
            }

            // If there is a hovered panel, then dim all of the other panels.
            if (hoveredPanel != null)
            {
                foreach (var panel in Panels)
                    panel.Thumbnail.FadeToColor(panel == hoveredPanel ? Color.White : Color.Gray, dt, 60);
            }
            // Otherwise allow all panels to be displayed at full brightness.
            else
            {
                foreach (var panel in Panels)
                    panel.Thumbnail.FadeToColor(Color.White, dt, 60);
            }
        }
    }
}
