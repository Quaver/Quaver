namespace Quaver.Shared.Screens
{
    /// <summary>
    ///     Replacement-safe state needed when temporarily leaving an active multiplayer game.
    /// </summary>
    internal interface IMultiplayerGameScreenState
    {
        bool DontLeaveGameUponScreenSwitch { get; set; }
    }
}
