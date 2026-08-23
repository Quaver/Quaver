using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;

namespace Quaver.Shared.Screens.V2
{
    /// <summary>
    ///     Shared contract used by temporary V2 screen scaffolds.
    /// </summary>
    internal interface ISkinV2PlaceholderConfig
    {
        SkinV2BackgroundConfig Background { get; }

        SkinV2PlaceholderTitleConfig Title { get; }
    }

    public sealed class SkinV2PlaceholderTitleConfig
    {
        [SkinFont]
        public string Font { get; set; } = SkinV2FontWeightsConfig.Bold;

        [Range(1, 256)]
        public int FontSize { get; set; } = SkinV2FontSizesConfig.Text3Xl;

        [SkinColor]
        [ConfigEditable]
        public string Color { get; set; } = "#FFFFFFFF";
    }
}
