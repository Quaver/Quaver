using System.Linq;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    internal static class OptionsDrawableCleanup
    {
        /// <summary>
        ///     Destroys a drawable and all of its children. Wobble's Destroy() does not destroy children.
        /// </summary>
        internal static void DestroyTree(Drawable root)
        {
            if (root == null)
                return;

            foreach (var child in root.Children.ToArray())
                DestroyTree(child);

            root.Destroy();
        }
    }
}
