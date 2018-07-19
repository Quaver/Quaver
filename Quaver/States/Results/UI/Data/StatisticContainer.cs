using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;

namespace Quaver.States.Results.UI.Data
{
    /// <summary>
    ///     Contains an individual statistic "screen/container" inside of the statistics.
    /// </summary>
    internal class StatisticContainer
    {
        /// <summary>
        ///    The name of the tab, used for the tab button.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        ///     The actual content to be placed in this individual tab.
        /// </summary>
        internal Sprite Content { get; }

        /// <summary>
        ///     The tab button that gets created to access this statistic.
        /// </summary>
        internal TextButton Button { get; set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        internal StatisticContainer(string name, Sprite content = null)
        {
            Name = name;
            Content = content;
        }
    }
}