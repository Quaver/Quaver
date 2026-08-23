namespace Quaver.Shared.Screens.V2.Music
{
    internal sealed class MusicPlayerScreenView : PlaceholderScreenView
    {
        public MusicPlayerScreenView(MusicPlayerScreen screen) : base(screen,
            config => config.Screens.Music, "Screens.Music",
            "Screen_V2_MusicPlayerScreen")
        {
        }
    }
}
