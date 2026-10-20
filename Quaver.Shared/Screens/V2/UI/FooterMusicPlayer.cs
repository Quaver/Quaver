using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.UI
{
    internal sealed class FooterMusicPlayer : Container
    {
        private SkinV2FooterMusicPlayerConfig Config { get; }

        private MusicPlaybackController Playback { get; }

        private bool Expanded { get; }

        private Sprite Background { get; }

        private Sprite Banner { get; }

        private Sprite BannerBrightness { get; }

        private MarqueeSpriteText Title { get; }

        private MarqueeSpriteText Artist { get; }

        private FlexContainer Controls { get; }

        private RoundedButton PlayButton { get; }

        private RoundedButton VolumeButton { get; }

        public Drawable VolumeDialogAnchor => VolumeButton;

        private RoundedButton QueueButton { get; }

        public Drawable QueueDialogAnchor => this;

        private RoundedButton ProgressButton { get; }

        private RoundedButton ProgressFill { get; }

        private Container TimeSlot { get; }

        private SpriteTextPlus TimeText { get; }

        private Texture2D PlayIcon { get; }

        private Texture2D PauseIcon { get; }

        private float LastProgressWidth { get; set; } = -1;

        public FooterMusicPlayer(SkinStoreV2Lease skin, SkinV2FooterMusicPlayerConfig config,
            MusicPlaybackController playback, bool expanded, Action<Drawable> showQueue,
            Action<Drawable> showVolume)
        {
            Config = config;
            Playback = playback;
            Expanded = expanded;
            Size = new ScalableVector2(GetPreferredWidth(), Expanded ? GetPreferredHeight() : GetCollapsedHeight());

            Background = new Sprite
            {
                Parent = this,
                Size = Size,
                Image = RoundedRectTextureCache.Get(Width, Height, Config.CornerRadius),
                Tint = SkinV2Color.Parse(Config.BackgroundColor)
            };

            if (Expanded)
            {
                Banner = new Sprite
                {
                    Parent = this,
                    Position = new ScalableVector2((Width - Config.BannerWidth) / 2f, Config.Padding),
                    Size = new ScalableVector2(Config.BannerWidth, Config.BannerHeight),
                    Image = UserInterface.DefaultBanner
                };

                BannerBrightness = new Sprite
                {
                    Parent = Banner,
                    Size = Banner.Size,
                    Tint = Color.White * Config.BannerBrightness
                };

                Title = CreateBannerText(Config.TitleFont, Config.TitleFontSize, Config.TitleColor);
                Artist = CreateBannerText(Config.ArtistFont, Config.ArtistFontSize, Config.ArtistColor);
                RefreshBannerMetadataLayout();
            }

            Controls = new FlexContainer
            {
                Parent = this,
                Position = new ScalableVector2(Config.Padding, GetControlsY()),
                Size = new ScalableVector2(Width - Config.Padding * 2, Config.ControlsHeight),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = Config.ControlGap
            };

            var previous = CreateIconButton(skin.LoadTexture(Config.Icons.Previous, UserInterface.JukeboxBackwardButtonV2), new Vector2(Config.PreviousNextIconWidth, Config.PreviousNextIconHeight), Playback.Previous);

            PlayIcon = skin.LoadTexture(Config.Icons.Play, UserInterface.JukeboxPlayButtonV2);
            PauseIcon = skin.LoadTexture(Config.Icons.Pause, UserInterface.JukeboxPauseButton);
            PlayButton = CreateIconButton(PlayIcon, new Vector2(Config.PlayPauseIconWidth, Config.PlayPauseIconHeight), Playback.TogglePlayback);
            PlayButton.CornerRadius = Config.ButtonSize / 2f;

            var next = CreateIconButton(skin.LoadTexture(Config.Icons.Next, UserInterface.JukeboxForwardButton), new Vector2(Config.PreviousNextIconWidth, Config.PreviousNextIconHeight), Playback.Next);

            ProgressButton = new RoundedButton((sender, args) => SeekFromMouse())
            {
                Parent = Controls,
                Size = new ScalableVector2(Config.ProgressWidth, Config.ProgressHeight),
                CornerRadius = Config.ProgressCornerRadius,
                Tint = SkinV2Color.Parse(Config.ProgressTrackColor),
                PerformHoverFade = false
            };
            Controls.SetItemOptions(ProgressButton, new FlexItemOptions { Basis = Config.ProgressWidth, Grow = 1, Shrink = 1 });

            ProgressFill = new RoundedButton
            {
                Parent = ProgressButton,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(0, Config.ProgressHeight),
                CornerRadius = Config.ProgressCornerRadius,
                Tint = SkinV2Color.Parse(Config.ProgressFillColor),
                UsePreviousSpriteBatchOptions = true,
                PerformHoverFade = false,
                IsInteractionEnabled = false,
                Visible = false
            };

            TimeSlot = new Container
            {
                Parent = Controls,
                Size = new ScalableVector2(Config.TimeWidth, Config.ControlsHeight)
            };
            Controls.SetItemOptions(TimeSlot, new FlexItemOptions { Basis = Config.TimeWidth, Shrink = 0 });

            TimeText = new SpriteTextPlus(FontManager.GetWobbleFont(Config.TimeFont),
                "00:00 / 00:00", Config.TimeFontSize, false)
            {
                Parent = TimeSlot,
                Alignment = Alignment.MidCenter,
                Tint = SkinV2Color.Parse(Config.SecondaryForegroundColor)
            };

            QueueButton = CreateIconButton(skin.LoadTexture(Config.Icons.Queue, UserInterface.JukeboxHamburgerIconV2),
                new Vector2(Config.ListIconWidth, Config.ListIconHeight), () => showQueue(this));
            VolumeButton = CreateIconButton(GlobalIcons.Get(GlobalIcon.Volume), new Vector2(Config.VolumeIconWidth, Config.VolumeIconHeight), () => showVolume(VolumeButton));

            SetFixedControl(previous);
            SetFixedControl(PlayButton);
            SetFixedControl(next);
            SetFixedControl(QueueButton);
            SetFixedControl(VolumeButton);
            Controls.RefreshLayout();

            if (Expanded)
            {
                MapManager.Selected.ValueChanged += OnSelectedMapChanged;
                BackgroundHelper.BannerLoaded += OnBannerLoaded;
                UpdateBanner(MapManager.Selected?.Value);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateMarqueeAlignment(Title);
            UpdateMarqueeAlignment(Artist);
            UpdatePlaybackState();
        }

        public override void Destroy()
        {
            if (Expanded)
            {
                MapManager.Selected.ValueChanged -= OnSelectedMapChanged;
                BackgroundHelper.BannerLoaded -= OnBannerLoaded;
            }

            base.Destroy();
        }

        public void ApplyWidth(float width)
        {
            width = Math.Max(Config.MinimumWidth, width);
            if (Math.Abs(Width - width) < 0.001f)
                return;

            Width = width;
            Background.Width = width;
            Background.Image = RoundedRectTextureCache.Get(width, Height, Config.CornerRadius);

            if (Banner != null)
            {
                Banner.Width = Math.Min(Config.BannerWidth, Math.Max(1, width - Config.Padding * 2));
                Banner.X = (width - Banner.Width) / 2f;
                BannerBrightness.Size = Banner.Size;
                RefreshBannerMetadataLayout();
            }

            Controls.Width = Math.Max(1, width - Config.Padding * 2);
            Controls.RefreshLayout();
            LastProgressWidth = -1;
        }

        private float GetPreferredWidth() => Config.BannerWidth + Config.Padding * 2;

        private float GetPreferredHeight() => Config.Padding * 2 + Config.BannerHeight + Config.SectionGap + Config.ControlsHeight;

        private float GetCollapsedHeight() => Config.Padding * 2 + Config.ControlsHeight;

        private RoundedButton CreateIconButton(Texture2D icon, Vector2 iconSize, Action action, Color? background = null, Color? foreground = null)
        {
            var button = new RoundedButton((sender, args) => action())
            {
                Parent = Controls,
                Size = new ScalableVector2(iconSize.X, Config.ButtonSize),
                CornerRadius = Config.CornerRadius,
                Tint = background ?? Color.Transparent
            };
            button.SetIcon(icon, iconSize);
            button.Icon.Tint = foreground ?? SkinV2Color.Parse(Config.ForegroundColor);

            return button;
        }

        private RoundedButton CreateIconButton(TextureRegion icon, Vector2 iconSize, Action action)
        {
            var button = new RoundedButton((sender, args) => action())
            {
                Parent = Controls,
                Size = new ScalableVector2(iconSize.X, Config.ButtonSize),
                CornerRadius = Config.CornerRadius,
                Tint = Color.Transparent
            };
            button.SetIcon(icon, iconSize);
            button.Icon.Tint = SkinV2Color.Parse(Config.ForegroundColor);

            return button;
        }

        private void SetFixedControl(Drawable control) => Controls.SetItemOptions(control, new FlexItemOptions { Basis = control.Width, Grow = 0, Shrink = 0 });

        private float GetControlsY() => Expanded
            ? Config.Padding + Config.BannerHeight + Config.SectionGap : Config.Padding;

        private void UpdatePlaybackState()
        {
            var track = AudioEngine.Track;
            var validTrack = track != null && !track.IsDisposed;
            var position = validTrack ? Math.Max(0, track.Position) : 0;
            var length = validTrack ? Math.Max(0, track.Length) : 0;
            var progress = length > 0 ? Math.Min(1, position / length) : 0;
            var progressWidth = (float) (ProgressButton.Width * progress);

            if (Math.Abs(progressWidth - LastProgressWidth) >= 0.5f)
            {
                LastProgressWidth = progressWidth;
                ProgressFill.Visible = progressWidth > 0;
                ProgressFill.Width = progressWidth;
            }

            var time = $"{FormatTime(position)} / {FormatTime(length)}";
            if (TimeText.Text != time)
                TimeText.Text = time;

            var icon = validTrack && track.IsPlaying ? PauseIcon : PlayIcon;
            if (PlayButton.Icon.Image != icon)
                PlayButton.Icon.Image = icon;
        }

        private void SeekFromMouse()
        {
            if (ProgressButton.AbsoluteSize.X <= 0)
                return;

            var progress = (MouseManager.CurrentState.X - ProgressButton.AbsolutePosition.X) / ProgressButton.AbsoluteSize.X;
            Playback.Seek(progress);
        }

        private void OnSelectedMapChanged(object sender, BindableValueChangedEventArgs<Map> args) => UpdateBanner(args.Value);

        private void UpdateBanner(Map map)
        {
            SetBannerMetadata(map?.Title ?? string.Empty, map?.Artist ?? string.Empty);

            if (map == null)
            {
                Banner.Image = UserInterface.DefaultBanner;
                return;
            }

            var mapset = map.Mapset;
            if (mapset == null)
            {
                Banner.Image = UserInterface.DefaultBanner;
                return;
            }

            if (BackgroundHelper.MapsetBanners.TryGetValue(mapset.Directory, out var banner) && banner != null && !banner.IsDisposed)
            {
                Banner.Image = banner;
                return;
            }

            BackgroundHelper.LoadMapsetBanner(mapset);
        }

        private void OnBannerLoaded(object sender, BannerLoadedEventArgs args)
        {
            if (args.Mapset != MapManager.Selected?.Value?.Mapset || args.Banner == null || args.Banner.IsDisposed)
                return;

            Banner.Image = args.Banner;
        }

        private MarqueeSpriteText CreateBannerText(string font, int fontSize, string color)
        {
            var text = new MarqueeSpriteText(FontManager.GetWobbleFont(font), string.Empty, fontSize, Config.BannerWidth - Config.BannerTextHorizontalPadding * 2)
            {
                Parent = Banner,
                IsActive = true
            };
            text.TextSprite.Tint = SkinV2Color.Parse(color);
            return text;
        }

        private void SetBannerMetadata(string title, string artist)
        {
            Title.TextSprite.Text = title;
            Artist.TextSprite.Text = artist;

            Title.ResetPosition();
            Artist.ResetPosition();
            UpdateMarqueeAlignment(Title);
            UpdateMarqueeAlignment(Artist);
        }

        private void RefreshBannerMetadataLayout()
        {
            var textWidth = Math.Max(1, Banner.Width - Config.BannerTextHorizontalPadding * 2);
            Title.Width = textWidth;
            Title.Height = Title.TextSprite.Height;
            Artist.Width = textWidth;
            Artist.Height = Artist.TextSprite.Height;

            var contentHeight = Title.Height + Config.BannerTextGap + Artist.Height;
            var contentY = (Banner.Height - contentHeight) / 2f;

            Title.Position = new ScalableVector2(Config.BannerTextHorizontalPadding, contentY);
            Title.ResetPosition();
            UpdateMarqueeAlignment(Title);

            Artist.Position = new ScalableVector2(Config.BannerTextHorizontalPadding, contentY + Title.Height + Config.BannerTextGap);
            Artist.ResetPosition();
            UpdateMarqueeAlignment(Artist);
        }

        private static void UpdateMarqueeAlignment(MarqueeSpriteText marquee)
        {
            if (marquee == null)
                return;

            var alignment = marquee.TextSprite.Width <= marquee.Width ? Alignment.MidCenter : Alignment.MidLeft;
            if (marquee.TextSprite.Alignment != alignment)
                marquee.TextSprite.Alignment = alignment;
        }

        private static string FormatTime(double milliseconds)
        {
            var totalSeconds = Math.Max(0, (int) (milliseconds / 1000));
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }

    }
}
