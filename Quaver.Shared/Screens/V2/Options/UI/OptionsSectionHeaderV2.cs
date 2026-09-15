using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     The two-color bar above a group of rows, reading "{Category} Settings | {Subcategory}".
    ///     It is only as wide as its text.
    /// </summary>
    internal sealed class OptionsSectionHeaderV2 : Container
    {
        private SplitRoundedPanel Background { get; }

        internal OptionsSectionHeaderV2(WobbleFontStore font, SkinV2OptionsRowConfig config, string categoryText,
            string subcategoryText)
        {
            var categoryLabel = new SpriteTextPlus(font, categoryText, config.SectionHeaderFontSize)
            {
                Tint = SkinV2Color.Parse(config.SectionHeaderTextColor),
                UsePreviousSpriteBatchOptions = true
            };
            var subcategoryLabel = new SpriteTextPlus(font, subcategoryText, config.SectionHeaderFontSize)
            {
                Tint = SkinV2Color.Parse(config.SectionHeaderSubcategoryTextColor),
                UsePreviousSpriteBatchOptions = true
            };

            // Each label has padding on both sides.
            var padding = config.HorizontalPadding;
            var splitX = padding * 2 + categoryLabel.Width;
            Size = new ScalableVector2(splitX + padding * 2 + subcategoryLabel.Width, config.RowHeight);

            Background = new SplitRoundedPanel(config.CornerRadius,
                SkinV2Color.Parse(config.SectionHeaderCategoryColor),
                SkinV2Color.Parse(config.SectionHeaderSubcategoryColor))
            {
                Parent = this,
                SplitPosition = splitX
            };

            categoryLabel.Parent = this;
            categoryLabel.Alignment = Alignment.MidLeft;
            categoryLabel.X = padding;

            subcategoryLabel.Parent = this;
            subcategoryLabel.Alignment = Alignment.MidLeft;
            subcategoryLabel.X = splitX + padding;

            ScissorPropagation.Apply(this);
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();

            Background?.SetSizeIfChanged(Size);
        }
    }
}
