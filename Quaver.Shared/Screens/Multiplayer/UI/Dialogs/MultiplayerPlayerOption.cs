using System;
using Microsoft.Xna.Framework;

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
        public Color Color { get; }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="clickAction"></param>
        /// <param name="color"></param>
        public MultiplayerPlayerOption(string name, Action clickAction)
        {
            Name = name;
            ClickAction = clickAction;
            Color = Color.White;
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="clickAction"></param>
        /// <param name="color"></param>
        public MultiplayerPlayerOption(string name, Action clickAction, Color color)
        {
            Name = name;
            ClickAction = clickAction;
            Color = color;
        }
    }
}