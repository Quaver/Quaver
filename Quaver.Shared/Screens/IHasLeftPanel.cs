using Quaver.Shared.Screens.Selection.UI;
using Wobble.Bindables;

namespace Quaver.Shared.Screens
{
    public interface IHasLeftPanel
    {
        Bindable<SelectContainerPanel> ActiveLeftPanel { get; set; }
    }
}