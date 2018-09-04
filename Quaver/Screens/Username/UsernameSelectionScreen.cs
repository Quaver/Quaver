using Wobble.Screens;

namespace Quaver.Screens.Username
{
    public class UsernameSelectionScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public UsernameSelectionScreen() => View = new UsernameSelectionScreenView(this);
    }
}