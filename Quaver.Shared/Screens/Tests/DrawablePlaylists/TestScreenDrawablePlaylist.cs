using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.DrawablePlaylists
{
    public sealed class TestScreenDrawablePlaylist : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestScreenDrawablePlaylist() => View = new TestScreenDrawablePlaylistView(this);
    }
}