namespace Quaver.Shared
{
    public enum QuaverScreenChangeType
    {
        /// <summary>
        ///     This will completely change the screen and not add it to the stack
        /// </summary>
        CompleteChange,

        /// <summary>
        ///     Adds the screen to the stack and doesn't destroy the prevous screen
        /// </summary>
        AddToStack,

        /// <summary>
        ///     Removes the top screen in the stack
        /// </summary>
        RemoveTopScreen
    }
}