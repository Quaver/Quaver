using Quaver.Shared.Database.Maps;

namespace Quaver.Shared.Screens.V2.Selection.UI
{
    /// <summary>
    ///     Data and layout information for a mapset or difficulty in the song select list
    /// </summary>
    internal readonly struct SelectionListEntry
    {
        internal Mapset Mapset { get; }
        internal Map Difficulty { get; }
        internal bool IsDifficulty => Difficulty != null;
        internal int DifficultyIndex { get; }

        internal SelectionListEntry(Mapset mapset, int difficultyIndex = -1)
        {
            Mapset = mapset;
            DifficultyIndex = difficultyIndex;
            Difficulty = difficultyIndex < 0 ? null : mapset.Maps[difficultyIndex];
        }

        internal bool HasSameItem(SelectionListEntry other) => Mapset == other.Mapset && Difficulty == other.Difficulty;
    }
}
