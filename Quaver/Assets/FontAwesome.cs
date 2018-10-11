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
        public static Texture2D StepBackward { get; set; }
        public static Texture2D StepForward { get; set; }
        public static Texture2D Undo { get; set; }

        /// <summary>
        ///     Loads all FontAwesome icon textures.
        /// </summary>
        public static void Load()
        {
            Home = AssetLoader.LoadTexture2D(QuaverResources.fa_home);
            Cog = AssetLoader.LoadTexture2D(QuaverResources.fa_cog_wheel_silhouette);
            PowerOff = AssetLoader.LoadTexture2D(QuaverResources.fa_power_button_off);
            Discord = AssetLoader.LoadTexture2D(QuaverResources.fa_discord);
            GamePad = AssetLoader.LoadTexture2D(QuaverResources.fa_gamepad_console);
            Coffee = AssetLoader.LoadTexture2D(QuaverResources.fa_coffee_cup_on_a_plate_black_silhouettes);
            Cloud = AssetLoader.LoadTexture2D(QuaverResources.fa_cloud_storage_download);
            Github = AssetLoader.LoadTexture2D(QuaverResources.fa_github_logo);
            Copy = AssetLoader.LoadTexture2D(QuaverResources.fa_copy_document);
            Exclamation = AssetLoader.LoadTexture2D(QuaverResources.fa_exclamation);
            Archive = AssetLoader.LoadTexture2D(QuaverResources.fa_archive_black_box);
            CircleOpen = AssetLoader.LoadTexture2D(QuaverResources.fa_circle_shape_outline);
            Volume = AssetLoader.LoadTexture2D(QuaverResources.fa_volume_up_interface_symbol);
            Music = AssetLoader.LoadTexture2D(QuaverResources.fa_music_note_black_symbol);
            Headphones = AssetLoader.LoadTexture2D(QuaverResources.fa_music_headphones);
            CircleClosed = AssetLoader.LoadTexture2D(QuaverResources.fa_circle);
            CaretDown = AssetLoader.LoadTexture2D(QuaverResources.fa_caret_down);
            ChevronDown = AssetLoader.LoadTexture2D(QuaverResources.fa_chevron_arrow_down);
            Desktop = AssetLoader.LoadTexture2D(QuaverResources.fa_desktop_monitor);
            GiftBox = AssetLoader.LoadTexture2D(QuaverResources.fa_gift_box);
            VideoPlay = AssetLoader.LoadTexture2D(QuaverResources.fa_play_video_button);
            Pencil = AssetLoader.LoadTexture2D(QuaverResources.fa_pencil);
            Play = AssetLoader.LoadTexture2D(QuaverResources.fa_play_button);
            Pause = AssetLoader.LoadTexture2D(QuaverResources.fa_pause_symbol);
            Stop = AssetLoader.LoadTexture2D(QuaverResources.fa_square_shape_shadow);
            File = AssetLoader.LoadTexture2D(QuaverResources.fa_file);
            Folder = AssetLoader.LoadTexture2D(QuaverResources.fa_open_folder);
            Save = AssetLoader.LoadTexture2D(QuaverResources.fa_save_file_option);
            FastForward = AssetLoader.LoadTexture2D(QuaverResources.fa_fast_forward_arrows);
            Clock = AssetLoader.LoadTexture2D(QuaverResources.fa_time);
            ArrowLeft = AssetLoader.LoadTexture2D(QuaverResources.fa_arrow_pointing_to_left);
            ArrowRight = AssetLoader.LoadTexture2D(QuaverResources.fa_arrow_pointing_to_right);
            ChevronSignLeft = AssetLoader.LoadTexture2D(QuaverResources.fa_angle_pointing_to_left);
            ChevronSignRight = AssetLoader.LoadTexture2D(QuaverResources.fa_angle_arrow_pointing_to_right);
            VideoPlay= AssetLoader.LoadTexture2D(QuaverResources.fa_play_video_button);
            Twitter = AssetLoader.LoadTexture2D(QuaverResources.fa_twitter_black_shape);
            Rss = AssetLoader.LoadTexture2D(QuaverResources.fa_rss_symbol);
            Code = AssetLoader.LoadTexture2D(QuaverResources.fa_code);
            Bars = AssetLoader.LoadTexture2D(QuaverResources.fa_signal_bars);
            Question = AssetLoader.LoadTexture2D(QuaverResources.fa_question_sign);
            Trophy = AssetLoader.LoadTexture2D(QuaverResources.fa_trophy);
            Globe = AssetLoader.LoadTexture2D(QuaverResources.fa_earth_globe);
            Comments = AssetLoader.LoadTexture2D(QuaverResources.fa_comments);
            Spinner = AssetLoader.LoadTexture2D(QuaverResources.fa_spinner_of_dots);
            Heart = AssetLoader.LoadTexture2D(QuaverResources.fa_heart_shape_silhouette);
            Times = AssetLoader.LoadTexture2D(QuaverResources.fa_times);
            Keyboard = AssetLoader.LoadTexture2D(QuaverResources.fa_keyboard);
            Search = AssetLoader.LoadTexture2D(QuaverResources.fa_magnifying_glass);
            ArrowCircle = AssetLoader.LoadTexture2D(QuaverResources.fa_right_arrow_in_a_circle);
            StepBackward = AssetLoader.LoadTexture2D(QuaverResources.fa_step_backward);
            StepForward = AssetLoader.LoadTexture2D(QuaverResources.fa_step_forward);
            Undo = AssetLoader.LoadTexture2D(QuaverResources.fa_undo_arrow);
        }
    }
}