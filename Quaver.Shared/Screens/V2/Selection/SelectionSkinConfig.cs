using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;

namespace Quaver.Shared.Screens.V2.Selection
{
    public sealed class SkinV2SelectionConfig : ISkinV2PlaceholderConfig
    {
        [Required]
        [ConfigEditable]
        public SkinV2BackgroundConfig Background { get; set; } =
            new SkinV2BackgroundConfig { SolidColor = "#080D13FF" };

        [Required]
        [ConfigEditable]
        public SkinV2PlaceholderTitleConfig Title { get; set; } = new SkinV2PlaceholderTitleConfig();
    }
}
