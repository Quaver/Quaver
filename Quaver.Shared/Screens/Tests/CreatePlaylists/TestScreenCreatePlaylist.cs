using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.CreatePlaylists
{
    public sealed class TestScreenCreatePlaylist : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestScreenCreatePlaylist() => View = new TestScreenCreatePlaylistView(this);
    }
}