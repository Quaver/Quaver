using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.V2.Downloading;
using Quaver.Shared.Screens.V2.Importing;
using Quaver.Shared.Screens.V2.Initialization;
using Quaver.Shared.Screens.V2.Loading;
using Quaver.Shared.Screens.V2.Main;
using Quaver.Shared.Screens.V2.Multi;
using Quaver.Shared.Screens.V2.Multiplayer;
using Quaver.Shared.Screens.V2.MultiplayerLobby;
using Quaver.Shared.Screens.V2.Music;
using Quaver.Shared.Screens.V2.Results;
using Quaver.Shared.Screens.V2.Selection;
using Quaver.Shared.Screens.V2.Theater;
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

        [ConfigRequired]
        [Required]
        public SkinV2MetadataConfig Metadata { get; set; } = new SkinV2MetadataConfig();

        [Required]
        public SkinV2SharedConfig Shared { get; set; } = new SkinV2SharedConfig();

        [Required]
        public SkinV2ScreensConfig Screens { get; set; } = new SkinV2ScreensConfig();
    }

    public sealed class SkinV2MetadataConfig
    {
        [ConfigRequired]
        [Required]
        public string Name { get; set; } = "Default Quaver Skin";

        [ConfigRequired]
        [Required]
        public string Author { get; set; } = "Quaver Team";

        [ConfigRequired]
        [Required]
        public string Version { get; set; } = "v0.1";
    }

    public sealed class SkinV2SharedConfig
    {
        [Required]
        public SkinV2BrandConfig Brand { get; set; } = new SkinV2BrandConfig();

        [Required]
        public SkinV2NavigationConfig Navigation { get; set; } = new SkinV2NavigationConfig();
    }

    public sealed class SkinV2BrandConfig
    {
        public const string DefaultAccentColor = "#1FBBFFFF";

        [ConfigEditable]
        [SkinColor]
        public string AccentColor { get; set; } = DefaultAccentColor;
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

        [Required]
        public SkinV2InitializationConfig Initialization { get; set; } =
            new SkinV2InitializationConfig();

        [Required]
        public SkinV2ImportingConfig Importing { get; set; } = new SkinV2ImportingConfig();

        [Required]
        public SkinV2MapLoadingConfig Loading { get; set; } = new SkinV2MapLoadingConfig();

        [Required]
        public SkinV2SelectionConfig Selection { get; set; } = new SkinV2SelectionConfig();

        [Required]
        public SkinV2DownloadingConfig Downloading { get; set; } = new SkinV2DownloadingConfig();

        [Required]
        public SkinV2MultiplayerLobbyConfig MultiplayerLobby { get; set; } =
            new SkinV2MultiplayerLobbyConfig();

        [Required]
        public SkinV2MultiplayerGameConfig MultiplayerGame { get; set; } =
            new SkinV2MultiplayerGameConfig();

        [Required]
        public SkinV2MultiplayerConfig Multiplayer { get; set; } = new SkinV2MultiplayerConfig();

        [Required]
        public SkinV2MusicPlayerConfig Music { get; set; } = new SkinV2MusicPlayerConfig();

        [Required]
        public SkinV2TheaterConfig Theater { get; set; } = new SkinV2TheaterConfig();

        [Required]
        public SkinV2ResultsConfig Results { get; set; } = new SkinV2ResultsConfig();
    }
}
