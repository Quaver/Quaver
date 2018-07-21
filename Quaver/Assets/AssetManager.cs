using Quaver.Main;

namespace Quaver.Assets
{
    internal static class AssetManager
    {
        /// <summary>
        ///     Loads all game assets.
        /// </summary>
        internal static void Load()
        {
            FontAwesome.Load();
            Fonts.Load();
            UserInterface.Load();
            SFX.Load();
            Titles.Load();
        }
    }
}