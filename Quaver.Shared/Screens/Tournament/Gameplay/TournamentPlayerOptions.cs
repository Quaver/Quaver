namespace Quaver.Shared.Screens.Tournament.Gameplay
{
    public class TournamentPlayerOptions
    {
        /// <summary>
        ///     The index of the player (player 1, player 2, player 3, etc)
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// </summary>
        /// <param name="index"></param>
        public TournamentPlayerOptions(int index)
        {
            Index = index;
        }
    }
}