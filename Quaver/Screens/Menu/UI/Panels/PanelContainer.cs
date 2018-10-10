using System.Collections.Generic;
using Wobble.Graphics;

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
        public static int StartingX => 65;

        /// <summary>
        ///
        /// </summary>
        /// <param name="panels"></param>
        public PanelContainer(List<Panel> panels)
        {
            Panels = panels;

            for (var i = 0; i < Panels.Count; i++)
            {
                var panel = Panels[i];

                panel.Parent = this;
                panel.Alignment = Alignment.MidLeft;
                panel.X = StartingX + i * panel.Width + (i * 10);
            }
        }
    }
}