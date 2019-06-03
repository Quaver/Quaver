using System;

namespace Quaver.Shared.Screens.Multiplayer.UI.Dialogs
{
    public class MultiplayerPlayerOption : IMultiplayerPlayerOption
    {
        /// <summary>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// </summary>
        public Action ClickAction { get; }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="clickAction"></param>
        public MultiplayerPlayerOption(string name, Action clickAction)
        {
            Name = name;
            ClickAction = clickAction;
        }
    }
}