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
        internal static Texture2D CircleOpen { get; set; }
        internal static Texture2D Volume { get; set; }
        internal static Texture2D Music { get; set; }
        internal static Texture2D Headphones { get; set; }
        internal static Texture2D CircleClosed { get; set; }
        internal static Texture2D CaretDown { get; set; }
        internal static Texture2D ChevronDown { get; set; }
        internal static Texture2D Desktop { get; set; }
        internal static Texture2D GiftBox { get; set; }
        internal static Texture2D VideoPlay { get; set; }
        internal static Texture2D Twitter { get; set; }
        internal static Texture2D Rss { get; set; }
        internal static Texture2D Code { get; set; }

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
            CircleOpen = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_circle_shape_outline);
            Volume = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_volume_up_interface_symbol);
            Music = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_music_note_black_symbol);
            Headphones = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_music_headphones);
            CircleClosed = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_circle);
            CaretDown = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_caret_down);
            ChevronDown = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_chevron_arrow_down);
            Desktop = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_desktop_monitor);
            GiftBox = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_gift_box);
            VideoPlay= ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_play_video_button);
            Twitter = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_twitter_black_shape);
            Rss = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_rss_symbol);
            Code = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_code);
        }
    }
}