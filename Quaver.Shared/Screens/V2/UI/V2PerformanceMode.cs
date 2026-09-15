using System;
using Quaver.Shared.Config;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Lets V2 screens skip animations and decorative effects while the performance preset is active.
    ///     Skipping an animation never skips its result: a menu still opens, it just opens at once.
    ///     This matters because animating a size re-lays out everything inside the drawable every frame.
    ///     Gameplay is not affected.
    /// </summary>
    internal static class V2PerformanceMode
    {
        /// <summary>
        ///     Whether the performance preset is active. It is read from the config every time,
        ///     so switching presets takes effect right away.
        /// </summary>
        internal static bool IsActive => string.Equals(QuaverYamlConfigManager.ActivePresetId,
            QuaverPresetCatalog.PerformanceId, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        ///     Whether V2 UI may animate between two states instead of jumping to the end state.
        /// </summary>
        internal static bool TransitionsEnabled => !IsActive;

        /// <summary>
        ///     Whether V2 UI may run decorative effects that never end, like the main menu particles.
        ///     These are turned off completely, because there is no end state to jump to.
        ///     Separate from <see cref="TransitionsEnabled"/> so the two can differ in the future.
        /// </summary>
        internal static bool AmbientEffectsEnabled => !IsActive;

        /// <summary>
        ///     Returns 0 while the performance preset is active, otherwise the given duration.
        /// </summary>
        internal static int Duration(int milliseconds) => IsActive ? 0 : milliseconds;
    }

    /// <summary>
    ///     Animation helpers that set the final value at once while the performance preset is active.
    ///     They are named differently from Wobble's own methods on purpose: an extension method with the
    ///     same name would never be called, because instance methods always win.
    /// </summary>
    internal static class V2PerformanceModeAnimationExtensions
    {
        internal static void FadeToOrSnap(this Sprite sprite, float alpha, Easing easing, int time)
        {
            if (sprite == null)
                return;

            if (V2PerformanceMode.TransitionsEnabled && time > 0)
            {
                sprite.FadeTo(alpha, easing, time);
                return;
            }

            sprite.ClearAnimations(AnimationProperty.Alpha);
            sprite.Alpha = alpha;
        }

        internal static void MoveToXOrSnap(this Drawable drawable, float x, Easing easing, int time)
        {
            if (drawable == null)
                return;

            if (V2PerformanceMode.TransitionsEnabled && time > 0)
            {
                drawable.MoveToX(x, easing, time);
                return;
            }

            drawable.ClearAnimations(AnimationProperty.X);
            drawable.X = x;
        }

        internal static void ChangeHeightToOrSnap(this Drawable drawable, int height, Easing easing, int time)
        {
            if (drawable == null)
                return;

            if (V2PerformanceMode.TransitionsEnabled && time > 0)
            {
                drawable.ChangeHeightTo(height, easing, time);
                return;
            }

            drawable.ClearAnimations(AnimationProperty.Height);
            drawable.Height = height;
        }
    }
}
