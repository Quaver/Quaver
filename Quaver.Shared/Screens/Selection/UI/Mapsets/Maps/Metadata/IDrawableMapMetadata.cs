namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Metadata
{
    public interface IDrawableMapMetadata
    {
        /// <summary>
        ///     Called when the mapset has opened
        /// </summary>
        void Open();

        /// <summary>
        ///     Called when the mapset has closed
        /// </summary>
        void Close();
    }
}