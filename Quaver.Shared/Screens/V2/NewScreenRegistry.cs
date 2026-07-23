namespace Quaver.Shared.Screens.V2
{
    /// <summary>
    ///     Explicit registration point for completed replacement screens.
    /// </summary>
    internal static class NewScreenRegistry
    {
        internal static ScreenFactorySet CreateFactorySet() => new()
        {
            MainMenu = () => new Main.MainMenuScreen()
        };
    }
}
