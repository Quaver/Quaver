namespace Quaver.Shared.Graphics.Form.Checkboxes
{
    public interface ICheckboxContainerItem
    {
        /// <summary>
        ///     If the item is currently selected based on user click actions
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        ///     Retrieve the name of the checkbox item
        /// </summary>
        /// <returns></returns>
        string GetName();

        /// <summary>
        ///     Determines if the item is selected or not
        /// </summary>
        /// <returns></returns>
        bool GetSelectedState();

        /// <summary>
        ///     Called when toggling the
        /// </summary>
        void OnToggle();
    }
}