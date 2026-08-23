using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Users
{
    /// <summary>
    ///     Shared lifecycle contract for account dropdowns used by legacy and replacement screens.
    /// </summary>
    public abstract class LoggedInUserDropdownBase : ScrollContainer
    {
        protected LoggedInUserDropdownBase(ScalableVector2 size, ScalableVector2 contentSize)
            : base(size, contentSize)
        {
        }

        public abstract ImageButton Button { get; }

        public abstract void Open();

        public abstract void Close();
    }
}
