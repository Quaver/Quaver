namespace Quaver.Shared.Screens.V2
{
    /// <summary>
    ///     Explicit registration point for completed v2 screens.
    /// </summary>
    internal static class V2ScreenRegistry
    {
        internal static ScreenFactorySet CreateFactorySet() => new()
        {
            MainMenu = () => new Main.MainMenuScreen()
        };
    }
}
