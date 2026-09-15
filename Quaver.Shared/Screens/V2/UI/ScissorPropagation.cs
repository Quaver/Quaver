using Wobble.Graphics;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     A scroll container clips its content through its sprite batch. A child that starts its own
    ///     batch turns the clipping off for itself and for everything drawn after it.
    ///     AddContainedDrawable only fixes the first level of children, which is not enough for rows
    ///     that contain controls, so rows call this on themselves.
    /// </summary>
    internal static class ScissorPropagation
    {
        /// <summary>
        ///     Makes a drawable and all of its children draw with their parent's sprite batch.
        /// </summary>
        internal static void Apply(Drawable root)
        {
            root.UsePreviousSpriteBatchOptions = true;

            foreach (var child in root.Children)
                Apply(child);
        }
    }
}
