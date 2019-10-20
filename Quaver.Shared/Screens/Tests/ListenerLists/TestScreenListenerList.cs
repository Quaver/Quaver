using Quaver.Server.Common.Objects;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.ListenerLists
{
    public sealed class TestScreenListenerList : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestScreenListenerList() => View = new TestScreenListenerListView(this);
    }
}