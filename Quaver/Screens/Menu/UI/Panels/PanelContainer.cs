using System.Collections.Generic;
using System.Data;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Transformations;

namespace Quaver.Screens.Menu.UI.Panels
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
        public static int StartingX => 64;

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
        ///     Aligns the panels and adds fade in transformations.
        /// </summary>
        private void InitializePanels()
        {
            for (var i = 0; i < Panels.Count; i++)
            {
                var panel = Panels[i];

                panel.Parent = this;
                panel.Alignment = Alignment.MidLeft;

                panel.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutQuint, 0,
                    StartingX + i * panel.Width + i * 10, 600 + 100 * i));

                panel.Thumbnail.Transformations.Add(new Transformation(TransformationProperty.Alpha,
                    Easing.EaseOutQuint, 0, 1, 500 + 100 * i));
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