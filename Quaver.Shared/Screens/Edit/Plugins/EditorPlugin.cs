using Quaver.Shared.Scripting;

namespace Quaver.Shared.Screens.Edit.Plugins
{
    public class EditorPlugin : LuaImGui
    {
        /// <summary>
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// </summary>
        public string Description { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="author"></param>
        /// <param name="description"></param>
        /// <param name="filePath"></param>
        /// <param name="isResource"></param>
        public EditorPlugin(string name, string author, string description, string filePath, bool isResource = false) : base(filePath, isResource)
        {
            Name = name;
            Author = author;
            Description = description;
        }
    }
}