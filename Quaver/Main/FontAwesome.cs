using Microsoft.Xna.Framework.Graphics;
using Quaver.Helpers;
using Quaver.Resources;

namespace Quaver.Main
{
    internal static class FontAwesome
    {
        internal static Texture2D Home { get; set; }
        internal static Texture2D Cog { get; set; }
        internal static Texture2D PowerOff { get; set; }
        internal static Texture2D Discord { get; set; }
        internal static Texture2D GamePad { get; set; }
        internal static Texture2D Coffee { get; set; }
        internal static Texture2D Cloud { get; set; }
        internal static Texture2D Github { get; set; }
        internal static Texture2D Copy { get; set; }
        internal static Texture2D Exclamation { get; set; }
        internal static Texture2D Archive { get; set; }

        /// <summary>
        ///     Loads all FontAwesome icon textures.
        /// </summary>
        internal static void Load()
        {
            Home = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_home);
            Cog = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_cog_wheel_silhouette);
            PowerOff = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_power_button_off);
            Discord = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_discord);
            GamePad = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_gamepad_console);
            Coffee = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_coffee_cup_on_a_plate_black_silhouettes);
            Cloud = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_cloud_storage_download);
            Github = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_github_logo);
            Copy = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_copy_document);
            Exclamation = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_exclamation);
            Archive = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_archive_black_box);
        }
    }
}