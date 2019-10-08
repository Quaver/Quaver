using Quaver.Server.Common.Objects;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.MenuJukebox
{
    public sealed class TestScreenMenuJukebox : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestScreenMenuJukebox() => View = new TestScreenMenuJukeboxView(this);
    }
}