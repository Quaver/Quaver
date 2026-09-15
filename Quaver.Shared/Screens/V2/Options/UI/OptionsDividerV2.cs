using System;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     A thin line between groups of rows. It is as wide as a row's label bar.
    /// </summary>
    internal sealed class OptionsDividerV2 : Container
    {
        private SkinV2OptionsRowConfig Config { get; }

        private RoundedPanel Line { get; }

        internal OptionsDividerV2(SkinV2OptionsRowConfig config)
        {
            Config = config;
            Height = config.DividerThickness;

            Line = new RoundedPanel(0)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Tint = SkinV2Color.Parse(config.DividerColor)
            };

            ScissorPropagation.Apply(this);
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();

            Line?.SetSizeIfChanged(Math.Max(1, Width - Config.HorizontalPadding), Height);
        }
    }
}
