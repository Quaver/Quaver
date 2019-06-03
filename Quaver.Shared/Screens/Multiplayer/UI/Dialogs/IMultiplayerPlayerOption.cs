using System;

namespace Quaver.Shared.Screens.Multiplayer.UI.Dialogs
{
    public interface IMultiplayerPlayerOption
    {
        /// <summary>
        ///     The name of the option
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The action that gets performed when clicking on it
        /// </summary>
        Action ClickAction { get; }
    }
}