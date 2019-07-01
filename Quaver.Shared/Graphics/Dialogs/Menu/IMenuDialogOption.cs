using System;
using Microsoft.Xna.Framework;

namespace Quaver.Shared.Graphics.Dialogs.Menu
{
    public interface IMenuDialogOption
    {
        /// <summary>
        ///     The name of the option
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The action that gets performed when clicking on it
        /// </summary>
        Action ClickAction { get; }

        /// <summary>
        ///     The color of the text
        /// </summary>
        Color Color { get; }
    }
}