using System;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Shared V2 button: a rounded box with a label that fades on hover. The caller picks the size,
    ///     so the button can match the controls next to it.
    /// </summary>
    public sealed class V2Button : RoundedButton
    {
        public V2Button(WobbleFontStore font, SkinV2ButtonConfig config, ScalableVector2 size, string label,
            EventHandler clickAction = null) : base(clickAction)
        {
            Size = size;
            CornerRadius = config.CornerRadius;
            Tint = SkinV2Color.Parse(config.BackgroundColor);
            PerformHoverFade = true;
            SetLabel(font, label, config.FontSize, SkinV2Color.Parse(config.TextColor));
        }
    }
}
