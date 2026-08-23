namespace Quaver.Shared
{
    /// <summary>
    ///     Controls which parts of the interface are covered by a screen transition.
    /// </summary>
    public enum ScreenTransitionMode
    {
        /// <summary>
        ///     Keeps elements retained by both screens visible and otherwise performs a full-screen transition.
        /// </summary>
        Auto,

        /// <summary>
        ///     Covers the entire interface, including retained elements.
        /// </summary>
        FullScreen,

        /// <summary>
        ///     Keeps elements retained by both screens visible above the transition.
        /// </summary>
        KeepPersistentElementsVisible
    }
}
