using System;
using Microsoft.Xna.Framework;

namespace Quaver.Shared.Graphics.Dialogs.Menu
{
    public class MenuDialogOption : IMenuDialogOption
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public Action ClickAction { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="clickAction"></param>
        /// <param name="color"></param>
        public MenuDialogOption(string name, Action clickAction)
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
        public MenuDialogOption(string name, Action clickAction, Color color)
        {
            Name = name;
            ClickAction = clickAction;
            Color = color;
        }
    }
}