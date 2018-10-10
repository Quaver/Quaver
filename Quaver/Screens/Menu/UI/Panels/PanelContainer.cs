using System.Collections.Generic;
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

                panel.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutQuint, 0,
                    StartingX + i * panel.Width + i * 10, 1000 + 100 * i));

                panel.Thumbnail.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.EaseOutQuint, 0, 1, 500 + 100 * i));
            }
        }
    }
}