using Microsoft.Xna.Framework.Graphics;
using Quaver.Helpers;
using Quaver.Resources;

namespace Quaver.Assets
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
        internal static Texture2D Pencil { get; set; }
        internal static Texture2D Play { get; set; }
        internal static Texture2D Pause { get; set; }
        internal static Texture2D Stop { get; set; }
        internal static Texture2D File { get; set; }
        internal static Texture2D Folder { get; set; }
        internal static Texture2D Save { get; set; }
        internal static Texture2D FastForward { get; set; }
        internal static Texture2D Clock { get; set; }
        internal static Texture2D ArrowLeft { get; set; }
        internal static Texture2D ArrowRight { get; set; }
        internal static Texture2D ChevronSignLeft { get; set; }
        internal static Texture2D ChevronSignRight { get; set; }
        internal static Texture2D Twitter { get; set; }
        internal static Texture2D Rss { get; set; }
        internal static Texture2D Code { get; set; }
        internal static Texture2D Bars { get; set; }
        internal static Texture2D Question { get; set; }
        internal static Texture2D Trophy { get; set; }
        internal static Texture2D Globe { get; set; }
        internal static Texture2D Comments { get; set; }
        internal static Texture2D Spinner { get; set; }

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
            VideoPlay = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_play_video_button);
            Pencil = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_pencil);
            Play = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_play_button);
            Pause = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_pause_symbol);
            Stop = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_square_shape_shadow);
            File = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_file);
            Folder = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_open_folder);
            Save = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_save_file_option);
            FastForward = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_fast_forward_arrows);
            Clock = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_time);
            ArrowLeft = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_arrow_pointing_to_left);
            ArrowRight = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_arrow_pointing_to_right);
            ChevronSignLeft = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_angle_pointing_to_left);
            ChevronSignRight = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_angle_arrow_pointing_to_right);
            VideoPlay= ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_play_video_button);
            Twitter = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_twitter_black_shape);
            Rss = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_rss_symbol);
            Code = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_code);
            Bars = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_signal_bars);
            Question = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_question_sign);
            Trophy = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_trophy);
            Globe = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_earth_globe);
            Comments = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_comments);
            Spinner = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_spinner_of_dots);
        }
    }
}