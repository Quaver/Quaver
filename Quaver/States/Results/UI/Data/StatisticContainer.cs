using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;

namespace Quaver.States.Results.UI.Data
{
    /// <summary>
    ///     Contains an individual statistic "screen" inside of the
    /// </summary>
    internal class StatisticContainer
    {
        internal string Name { get; }

        internal Sprite Content { get; }

        internal TextButton Button { get; set; }

        internal StatisticContainer(string name, Sprite content = null)
        {
            Name = name;
            Content = content;
        }
    }
}