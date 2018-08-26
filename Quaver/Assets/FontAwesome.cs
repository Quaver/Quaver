using System.Drawing.Imaging;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Resources;
using Wobble.Assets;

namespace Quaver.Assets
{
    public static class FontAwesome
    {
        public static Texture2D Home { get; set; }
        public static Texture2D Cog { get; set; }
        public static Texture2D PowerOff { get; set; }
        public static Texture2D Discord { get; set; }
        public static Texture2D GamePad { get; set; }
        public static Texture2D Coffee { get; set; }
        public static Texture2D Cloud { get; set; }
        public static Texture2D Github { get; set; }
        public static Texture2D Copy { get; set; }
        public static Texture2D Exclamation { get; set; }
        public static Texture2D Archive { get; set; }
        public static Texture2D CircleOpen { get; set; }
        public static Texture2D Volume { get; set; }
        public static Texture2D Music { get; set; }
        public static Texture2D Headphones { get; set; }
        public static Texture2D CircleClosed { get; set; }
        public static Texture2D CaretDown { get; set; }
        public static Texture2D ChevronDown { get; set; }
        public static Texture2D Desktop { get; set; }
        public static Texture2D GiftBox { get; set; }
        public static Texture2D VideoPlay { get; set; }
        public static Texture2D Pencil { get; set; }
        public static Texture2D Play { get; set; }
        public static Texture2D Pause { get; set; }
        public static Texture2D Stop { get; set; }
        public static Texture2D File { get; set; }
        public static Texture2D Folder { get; set; }
        public static Texture2D Save { get; set; }
        public static Texture2D FastForward { get; set; }
        public static Texture2D Clock { get; set; }
        public static Texture2D ArrowLeft { get; set; }
        public static Texture2D ArrowRight { get; set; }
        public static Texture2D ChevronSignLeft { get; set; }
        public static Texture2D ChevronSignRight { get; set; }
        public static Texture2D Twitter { get; set; }
        public static Texture2D Rss { get; set; }
        public static Texture2D Code { get; set; }
        public static Texture2D Bars { get; set; }
        public static Texture2D Question { get; set; }
        public static Texture2D Trophy { get; set; }
        public static Texture2D Globe { get; set; }
        public static Texture2D Comments { get; set; }
        public static Texture2D Spinner { get; set; }
        public static Texture2D Heart { get; set; }
        public static Texture2D Times { get; set; }
        public static Texture2D Keyboard { get; set; }
        public static Texture2D Search { get; set; }
        public static Texture2D ArrowCircle { get; set; }

        /// <summary>
        ///     Loads all FontAwesome icon textures.
        /// </summary>
        public static void Load()
        {
            Home = AssetLoader.LoadTexture2D(QuaverResources.fa_home, ImageFormat.Png);
            Cog = AssetLoader.LoadTexture2D(QuaverResources.fa_cog_wheel_silhouette, ImageFormat.Png);
            PowerOff = AssetLoader.LoadTexture2D(QuaverResources.fa_power_button_off, ImageFormat.Png);
            Discord = AssetLoader.LoadTexture2D(QuaverResources.fa_discord, ImageFormat.Png);
            GamePad = AssetLoader.LoadTexture2D(QuaverResources.fa_gamepad_console, ImageFormat.Png);
            Coffee = AssetLoader.LoadTexture2D(QuaverResources.fa_coffee_cup_on_a_plate_black_silhouettes, ImageFormat.Png);
            Cloud = AssetLoader.LoadTexture2D(QuaverResources.fa_cloud_storage_download, ImageFormat.Png);
            Github = AssetLoader.LoadTexture2D(QuaverResources.fa_github_logo, ImageFormat.Png);
            Copy = AssetLoader.LoadTexture2D(QuaverResources.fa_copy_document, ImageFormat.Png);
            Exclamation = AssetLoader.LoadTexture2D(QuaverResources.fa_exclamation, ImageFormat.Png);
            Archive = AssetLoader.LoadTexture2D(QuaverResources.fa_archive_black_box, ImageFormat.Png);
            CircleOpen = AssetLoader.LoadTexture2D(QuaverResources.fa_circle_shape_outline, ImageFormat.Png);
            Volume = AssetLoader.LoadTexture2D(QuaverResources.fa_volume_up_interface_symbol, ImageFormat.Png);
            Music = AssetLoader.LoadTexture2D(QuaverResources.fa_music_note_black_symbol, ImageFormat.Png);
            Headphones = AssetLoader.LoadTexture2D(QuaverResources.fa_music_headphones, ImageFormat.Png);
            CircleClosed = AssetLoader.LoadTexture2D(QuaverResources.fa_circle, ImageFormat.Png);
            CaretDown = AssetLoader.LoadTexture2D(QuaverResources.fa_caret_down, ImageFormat.Png);
            ChevronDown = AssetLoader.LoadTexture2D(QuaverResources.fa_chevron_arrow_down, ImageFormat.Png);
            Desktop = AssetLoader.LoadTexture2D(QuaverResources.fa_desktop_monitor, ImageFormat.Png);
            GiftBox = AssetLoader.LoadTexture2D(QuaverResources.fa_gift_box, ImageFormat.Png);
            VideoPlay = AssetLoader.LoadTexture2D(QuaverResources.fa_play_video_button, ImageFormat.Png);
            Pencil = AssetLoader.LoadTexture2D(QuaverResources.fa_pencil, ImageFormat.Png);
            Play = AssetLoader.LoadTexture2D(QuaverResources.fa_play_button, ImageFormat.Png);
            Pause = AssetLoader.LoadTexture2D(QuaverResources.fa_pause_symbol, ImageFormat.Png);
            Stop = AssetLoader.LoadTexture2D(QuaverResources.fa_square_shape_shadow, ImageFormat.Png);
            File = AssetLoader.LoadTexture2D(QuaverResources.fa_file, ImageFormat.Png);
            Folder = AssetLoader.LoadTexture2D(QuaverResources.fa_open_folder, ImageFormat.Png);
            Save = AssetLoader.LoadTexture2D(QuaverResources.fa_save_file_option, ImageFormat.Png);
            FastForward = AssetLoader.LoadTexture2D(QuaverResources.fa_fast_forward_arrows, ImageFormat.Png);
            Clock = AssetLoader.LoadTexture2D(QuaverResources.fa_time, ImageFormat.Png);
            ArrowLeft = AssetLoader.LoadTexture2D(QuaverResources.fa_arrow_pointing_to_left, ImageFormat.Png);
            ArrowRight = AssetLoader.LoadTexture2D(QuaverResources.fa_arrow_pointing_to_right, ImageFormat.Png);
            ChevronSignLeft = AssetLoader.LoadTexture2D(QuaverResources.fa_angle_pointing_to_left, ImageFormat.Png);
            ChevronSignRight = AssetLoader.LoadTexture2D(QuaverResources.fa_angle_arrow_pointing_to_right, ImageFormat.Png);
            VideoPlay= AssetLoader.LoadTexture2D(QuaverResources.fa_play_video_button, ImageFormat.Png);
            Twitter = AssetLoader.LoadTexture2D(QuaverResources.fa_twitter_black_shape, ImageFormat.Png);
            Rss = AssetLoader.LoadTexture2D(QuaverResources.fa_rss_symbol, ImageFormat.Png);
            Code = AssetLoader.LoadTexture2D(QuaverResources.fa_code, ImageFormat.Png);
            Bars = AssetLoader.LoadTexture2D(QuaverResources.fa_signal_bars, ImageFormat.Png);
            Question = AssetLoader.LoadTexture2D(QuaverResources.fa_question_sign, ImageFormat.Png);
            Trophy = AssetLoader.LoadTexture2D(QuaverResources.fa_trophy, ImageFormat.Png);
            Globe = AssetLoader.LoadTexture2D(QuaverResources.fa_earth_globe, ImageFormat.Png);
            Comments = AssetLoader.LoadTexture2D(QuaverResources.fa_comments, ImageFormat.Png);
            Spinner = AssetLoader.LoadTexture2D(QuaverResources.fa_spinner_of_dots, ImageFormat.Png);
            Heart = AssetLoader.LoadTexture2D(QuaverResources.fa_heart_shape_silhouette, ImageFormat.Png);
            Times = AssetLoader.LoadTexture2D(QuaverResources.fa_times, ImageFormat.Png);
            Keyboard = AssetLoader.LoadTexture2D(QuaverResources.fa_keyboard, ImageFormat.Png);
            Search = AssetLoader.LoadTexture2D(QuaverResources.fa_magnifying_glass, ImageFormat.Png);
            ArrowCircle = AssetLoader.LoadTexture2D(QuaverResources.fa_right_arrow_in_a_circle, ImageFormat.Png);
        }
    }
}