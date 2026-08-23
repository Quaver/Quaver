using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Mapsets;

namespace Quaver.Shared.Screens.Selection
{
    /// <summary>
    ///     State needed by systems outside of a selection screen implementation.
    /// </summary>
    internal interface ISelectionScreenState
    {
        SelectScrollContainerType ActiveScrollContainer { get; }

        SelectContainerPanel ActiveLeftPanel { get; }
    }
}
