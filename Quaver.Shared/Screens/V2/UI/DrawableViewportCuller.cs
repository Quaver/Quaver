using System.Collections.Generic;
using MonoGame.Extended;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     A list entry that has to keep updating while it is scrolled out of view, because it is busy.
    ///     For example a key field waiting for a key, a text box being typed in, or a dropdown with its
    ///     menu open.
    /// </summary>
    internal interface IViewportCullExempt
    {
        bool IsCullExempt { get; }
    }

    /// <summary>
    ///     Hides the entries of a scrolling list that are outside the visible area. A scroll container only
    ///     clips what is drawn, so without this every row would still update and draw every frame.
    ///     Call <see cref="Prepare"/> once for each entry when the list is built, then <see cref="Apply"/>
    ///     every frame.
    /// </summary>
    internal static class DrawableViewportCuller
    {
        /// <summary>
        ///     Makes the entry skip its update, and the layout work for its children, while it is hidden.
        ///     By default Wobble still updates hidden drawables.
        /// </summary>
        internal static void Prepare(Drawable entry)
        {
            if (entry == null)
                return;

            entry.UpdateWhenInvisible = false;
            entry.DeferChildRectangleRecalculationWhileHidden = true;
        }

        /// <summary>
        ///     Shows the entries that overlap the viewport and hides the rest. Entries within
        ///     <paramref name="margin"/> above or below the viewport stay visible, so they are ready
        ///     before they scroll into view.
        /// </summary>
        internal static void Apply(IReadOnlyList<Drawable> entries, RectangleF viewport, float margin)
        {
            if (entries == null || entries.Count == 0)
                return;

            var top = viewport.Top - margin;
            var bottom = viewport.Bottom + margin;

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || entry.IsDisposed)
                    continue;

                var rectangle = entry.ScreenRectangle;
                var shouldBeVisible = rectangle.Bottom >= top && rectangle.Top <= bottom ||
                                      entry is IViewportCullExempt exempt && exempt.IsCullExempt;

                if (entry.Visible == shouldBeVisible)
                    continue;

                entry.Visible = shouldBeVisible;

                if (!shouldBeVisible)
                    ResetInteractionState(entry);
            }
        }

        private static void ResetInteractionState(Drawable drawable)
        {
            if (drawable is Button button)
                button.ResetInteractionState();

            for (var i = 0; i < drawable.Children.Count; i++)
                ResetInteractionState(drawable.Children[i]);
        }
    }
}
