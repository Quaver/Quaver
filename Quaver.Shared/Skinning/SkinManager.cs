namespace Quaver.Shared.Skinning
{
    public static class SkinManager
    {
        /// <summary>
        ///     The currently selected skin
        /// </summary>
        public static SkinStore Skin { get; private set; }

        /// <summary>
        ///     Loads the currently selected skin
        /// </summary>
        public static void Load() => Skin = new SkinStore();
    }
}