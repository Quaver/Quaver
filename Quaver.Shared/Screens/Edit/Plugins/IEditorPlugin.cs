using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Quaver.Shared.Screens.Edit.Plugins
{
    public interface IEditorPlugin
    {
        /// <summary>
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// </summary>
        bool IsWindowHovered { get; }

        /// <summary>
        /// </summary>
        string Name { get; }

        /// <summary>
        /// </summary>
        string Author { get; }

        /// <summary>
        /// </summary>
        string Description { get; set; }

        /// <summary>
        ///     If the plugin is built into the editor
        /// </summary>
        bool IsBuiltIn { get; set; }

        /// <summary>
        ///     The directory of the plugin if it is not built in.
        /// </summary>
        string Directory { get; set; }

        /// <summary>
        ///     If the plugin belongs to the Steam Workshop
        /// </summary>
        bool IsWorkshop { get; set; }

        /// <summary>
        ///     Called when the plugin goes active
        /// </summary>
        void Initialize();

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        void Draw(GameTime gameTime);

        /// <summary>
        /// </summary>
        void Destroy();
    }
}