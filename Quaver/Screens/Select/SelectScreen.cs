using Wobble.Screens;

namespace Quaver.Screens.Select
{
    public class SelectScreen : Screen
    {
        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public SelectScreen() => View = new SelectScreenView(this);
    }
}