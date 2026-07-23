using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.V2.Main;
using Quaver.Shared.Screens.V2.UI;
using Wobble.Configuration;

namespace Quaver.Shared.Skinning.V2
{
    /// <summary>
    ///     Root of the skin.yml document. Component-specific configuration lives beside its owning screen or UI.
    /// </summary>
    public sealed class SkinV2Config
    {
        [ConfigRequired]
        [Range(2, 2)]
        public int FormatVersion { get; set; } = 2;

        [Required]
        public SkinV2SharedConfig Shared { get; set; } = new SkinV2SharedConfig();

        [Required]
        public SkinV2ScreensConfig Screens { get; set; } = new SkinV2ScreensConfig();
    }

    public sealed class SkinV2SharedConfig
    {
        [Required]
        public SkinV2NavigationConfig Navigation { get; set; } = new SkinV2NavigationConfig();
    }

    public static class SkinV2FontSizesConfig
    {
        public const int Text3Xl = 24;
        public const int Text2Xl = 22;
        public const int TextXl = 20;
        public const int TextLg = 18;
        public const int TextBase = 16;
        public const int TextSm = 14;
        public const int TextXs = 12;
        public const int Text2Xs = 10;
    }

    public static class SkinV2FontWeightsConfig
    {
        public static readonly string Bold = Fonts.InterBold;
        public static readonly string SemiBold = Fonts.InterSemiBold;
        public static readonly string Medium = Fonts.InterMedium;
    }

    public static class SkinV2MarginsConfig
    {
        public const float Lg = 20;
        public const float Md = 10;
        public const float Sm = 5;
    }
    
    public static class SkinV2Spacing
    {
        public const int Spacing3Xl = 24;
        public const int Spacing2Xl = 22;
        public const int SpacingXl = 20;
        public const int SpacingLg = 18;
        public const int SpacingBase = 16;
        public const int SpacingSm = 14;
        public const int SpacingXs = 12;
        public const int Spacing2Xs = 10;
    }

    public static class SkinV2BorderRadiusConfig
    {
        public const int Normal = 6;
    }

    public sealed class SkinV2ScreensConfig
    {
        [Required]
        public SkinV2MainConfig Main { get; set; } = new SkinV2MainConfig();
    }
}
