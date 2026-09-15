using System;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     One option row: a label bar with a control on the right.
    ///     A toggle, slider or key field sits on top of the label bar. A dropdown or button sits next to
    ///     a shorter label bar instead, with a gap between them.
    /// </summary>
    internal sealed class OptionsRowV2 : Container, IViewportCullExempt
    {
        private SkinV2OptionsRowConfig Config { get; }

        private RoundedPanel LabelBackground { get; }

        private Drawable Control { get; }

        private bool ControlSitsOnLabelBar { get; }

        /// <summary>
        ///     Only the control can be busy, for example while waiting for a key.
        /// </summary>
        public bool IsCullExempt => Control is IViewportCullExempt exempt && exempt.IsCullExempt;

        internal OptionsRowV2(WobbleFontStore font, SkinV2OptionsRowConfig config, string label, Drawable control,
            bool controlSitsOnLabelBar)
        {
            Config = config;
            Control = control;
            ControlSitsOnLabelBar = controlSitsOnLabelBar;

            Height = config.RowHeight;

            LabelBackground = new RoundedPanel(config.CornerRadius)
            {
                Parent = this,
                Tint = SkinV2Color.Parse(config.BackgroundColor)
            };

            _ = new SpriteTextPlus(font, label, config.LabelFontSize)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = config.HorizontalPadding,
                Tint = SkinV2Color.Parse(config.LabelColor),
                UsePreviousSpriteBatchOptions = true
            };

            Control.Parent = this;
            Control.Alignment = Alignment.MidRight;

            Control.X = ControlSitsOnLabelBar
                ? -(config.HorizontalPadding + config.ControlGap)
                : -config.HorizontalPadding;

            ScissorPropagation.Apply(this);
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();

            if (LabelBackground == null)
                return;

            var width = ControlSitsOnLabelBar
                ? Width - Config.HorizontalPadding
                : Width - Config.HorizontalPadding - Control.Width - Config.ControlGap;

            LabelBackground.SetSizeIfChanged(Math.Max(1, width), Height);
        }
    }
}
