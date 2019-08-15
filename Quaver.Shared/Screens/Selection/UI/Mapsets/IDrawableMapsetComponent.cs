namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public interface IDrawableMapsetComponent
    {
        /// <summary>
        ///     Called when the mapset/map is selected
        /// </summary>
        void Select();

        /// <summary>
        ///     Called when the mapset/map is deselected
        /// </summary>
        void Deselect();
    }
}