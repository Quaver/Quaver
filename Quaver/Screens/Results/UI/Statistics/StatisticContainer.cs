using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.Results.UI.Statistics
{
    public class StatisticContainer
    {
        /// <summary>
        ///    The name of the tab, used for the tab button.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The actual content to be placed in this individual tab.
        /// </summary>
        public Sprite Content { get; }

        /// <summary>
        ///     The tab button that gets created to access this statistic.
        /// </summary>
        public TextButton Button { get; set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        public StatisticContainer(string name, Sprite content = null)
        {
            Name = name;
            Content = content;
        }
    }
}
