using Microsoft.Xna.Framework;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     A rounded panel with a gradient. It is the accent color at one edge, fades to the dark color
    ///     over <see cref="TransitionFraction"/> of the width, and stays dark after that.
    /// </summary>
    internal sealed class GradientRoundedPanel : BakedRoundedPanel<(Color Accent, Color Dark, bool AccentOnRight)>
    {
        /// <summary>
        ///     How much of the width (0 to 1) the fade takes.
        /// </summary>
        private float TransitionFraction { get; }

        private Color AccentColor { get; set; }

        private Color DarkColor { get; set; }

        /// <summary>
        ///     Whether the accent color is at the right edge instead of the left edge.
        /// </summary>
        private bool AccentOnRight { get; set; }

        internal GradientRoundedPanel(float cornerRadius, Color accentColor, Color darkColor, bool accentOnRight,
            float transitionFraction = 1f) : base(cornerRadius)
        {
            TransitionFraction = MathHelper.Clamp(transitionFraction, 0.0001f, 1f);
            AccentColor = accentColor;
            DarkColor = darkColor;
            AccentOnRight = accentOnRight;
        }

        internal void SetColors(Color accentColor, Color darkColor, bool accentOnRight)
        {
            if (AccentColor == accentColor && DarkColor == darkColor && AccentOnRight == accentOnRight)
                return;

            AccentColor = accentColor;
            DarkColor = darkColor;
            AccentOnRight = accentOnRight;
            RefreshTexture();
        }

        protected override (Color Accent, Color Dark, bool AccentOnRight) Fill =>
            (AccentColor, DarkColor, AccentOnRight);

        protected override Color GetColor(float x, int textureWidth)
        {
            var distanceFromAccentEdge = x / textureWidth;

            if (AccentOnRight)
                distanceFromAccentEdge = 1 - distanceFromAccentEdge;

            var amount = MathHelper.Clamp(distanceFromAccentEdge / TransitionFraction, 0, 1);
            return Color.Lerp(AccentColor, DarkColor, amount);
        }
    }
}
