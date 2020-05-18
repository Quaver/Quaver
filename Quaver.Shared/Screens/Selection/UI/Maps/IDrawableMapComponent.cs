namespace Quaver.Shared.Screens.Selection.UI.Maps
{
    public interface IDrawableMapComponent
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