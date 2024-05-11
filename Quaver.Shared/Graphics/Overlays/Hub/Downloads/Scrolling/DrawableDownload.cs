using System;
using System.Net;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Events.Download;
using Quaver.Server.Client.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Download;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Hub.Downloads.Scrolling
{
    public sealed class DrawableDownload : PoolableSprite<MapsetDownload>
    {
        private int _height = 114;
        private bool _expanded = true;
        private static readonly Color ContentRed = new Color(235, 87, 87);
        private static readonly Color BorderRed = new Color(191, 71, 71);

        /// <summary>
        /// </summary>
        public override int HEIGHT => _height;

        /// <summary>
        /// </summary>
        private Sprite ContentContainer { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        private ImageButton CancelButton { get; set; }
        private ImageButton RetryButton { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        private SpriteTextPlus EstimatedTimeLeft { get; set; }

        /// <summary>
        /// </summary>
        private ProgressBar ProgressBar { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ProgressPercentage { get; set; }

        public event EventHandler DimensionsChanged;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableDownload(PoolableScrollContainer<MapsetDownload> container, MapsetDownload item, int index)
            : base(container, item, index)
        {
            Size = new ScalableVector2(container.Width, HEIGHT);
            Alpha = 0;

            CreateContentContainer();
            CreateButtons();
            CreateEstimatedTimeLeft();
            CreateSongName();
            CreateProgressPercentage();
            CreateProgressBar();
            UpdateContainerSize();

            UpdateText();

            Item.Progress.ValueChanged += OnDownloadProgressChanged;
            Item.FileDownloader.ValueChanged += (sender, args) =>
            {
                if (args.Value == null) return;
                args.Value.StatusUpdated += OnDownloadStatusUpdated;
            };
            Item.FileDownloader.TriggerChange();
        }

        private void OnDownloadStatusUpdated(object sender, DownloadStatusChangedEventArgs e)
        {
            CreateProgressBar(e.Cancelled);
            ContentContainer.Image =
                e.Cancelled ? UserInterface.HubDownloadContainerRed : UserInterface.HubDownloadContainerBlue;
            if (e.Status is FileDownloaderStatus.Cancelled or FileDownloaderStatus.Complete)
            {
                EstimatedTimeLeft.Text = "Download Complete";
                Collapse();
                ((DownloadScrollContainer)Container).DownloadNextItem();
            }
            else
            {
                EstimatedTimeLeft.Text = "Downloading";
                Expand();
            }

            if (e.Status is FileDownloaderStatus.Downloading or FileDownloaderStatus.Initialized
                or FileDownloaderStatus.Connecting)
            {
            }
            else
            {
            }
        }

        private void Collapse()
        {
            _expanded = false;
            ProgressBar.Visible = false;
            ProgressPercentage.Visible = false;
            UpdateContainerSize();
            Button.Size = new ScalableVector2(ContentContainer.Width - 2, ContentContainer.Height - 2);
            DimensionsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Expand()
        {
            _expanded = true;
            ProgressBar.Visible = true;
            ProgressPercentage.Visible = true;
            UpdateContainerSize();
            Button.Size = new ScalableVector2(ContentContainer.Width - 2, ContentContainer.Height - 2);
            DimensionsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Button.Alpha = Button.IsHovered ? 0.35f : 0;
            RetryButton.Alpha = RetryButton.IsHovered ? 0.75f : 1;
            CancelButton.Alpha = CancelButton.IsHovered ? 0.75f : 1;

            var game = (QuaverGame)GameBase.Game;

            if (Container != null)
                Button.IsClickable = game.OnlineHub.SelectedSection ==
                                     game.OnlineHub.Sections[OnlineHubSectionType.ActiveDownloads];

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            Item.Progress.ValueChanged -= OnDownloadProgressChanged;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(MapsetDownload item, int index)
        {
            Item = item;
            Index = index;

            ScheduleUpdate(UpdateText);
        }

        /// <summary>
        /// </summary>
        private void CreateContentContainer()
        {
            ContentContainer = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Image = UserInterface.HubDownloadContainerBlue,
                UsePreviousSpriteBatchOptions = true
            };
        }

        private void UpdateContainerSize()
        {
            var contentContainerHeight = _expanded
                ? ProgressBar.RelativeRectangle.Bottom + 20
                : Name.RelativeRectangle.Bottom + 20;
            ContentContainer.Size = new ScalableVector2(Width * 0.94f, contentContainerHeight);
            _height = (int)(ContentContainer.Height / 0.85f);
        }

        /// <summary>
        /// </summary>
        private void CreateButtons()
        {
            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = ContentContainer,
                Alignment = Alignment.MidCenter,
                Alpha = 0,
                Size = new ScalableVector2(ContentContainer.Width - 2, ContentContainer.Height - 2)
            };
            RetryButton = new ImageButton(UserInterface.HubDownloadRetry)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopRight,
                Position = new ScalableVector2(-44, 20),
                Size = new ScalableVector2(16, 16)
            };
            RetryButton.Clicked += (sender, args) => { Item.FileDownloader.Value?.Restart(); };
            CancelButton = new ImageButton(UserInterface.HubDownloadRemove)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopRight,
                Position = new ScalableVector2(-18, 20),
                Size = new ScalableVector2(16, 16)
            };
            CancelButton.Clicked += (sender, args) =>
            {
                if (!_expanded || Item.FileDownloader.Value == null)
                    Item.RemoveDownload();
                else
                    Item.FileDownloader.Value?.Cancel();
            };
        }

        /// <summary>
        /// </summary>
        private void CreateSongName()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBold), "Artist - Title", 16)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopLeft,
                X = 18,
                Y = EstimatedTimeLeft.RelativeRectangle.Bottom + 12,
                UsePreviousSpriteBatchOptions = true
            };
        }

        private void CreateEstimatedTimeLeft()
        {
            EstimatedTimeLeft = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBold), "Downloading (In Queue)", 18)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(18, 20),
            };
        }

        /// <summary>
        /// </summary>
        private void CreateProgressBar(bool red = false)
        {
            ProgressBar?.Destroy();
            var activeColor = red ? ContentRed : Colors.MainAccent;
            var borderColor = red ? BorderRed : Colors.MainBlue;
            ProgressBar = new ProgressBar(new Vector2(Width - 110, 14), 0, 100, 0, Color.Transparent,
                activeColor)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopLeft,
                X = Name.X,
                Y = ProgressPercentage.Y,
                UsePreviousSpriteBatchOptions = true,
                ActiveBar =
                {
                    UsePreviousSpriteBatchOptions = true
                },
            };

            ProgressBar.AddBorder(borderColor, 2);
        }

        /// <summary>
        /// </summary>
        private void CreateProgressPercentage()
        {
            ProgressPercentage = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBold), "0%", 16)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopRight,
                TextAlignment = TextAlignment.Right,
                X = -18,
                Y = Name.RelativeRectangle.Bottom + 12,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void UpdateText()
        {
            Name.Text = string.IsNullOrEmpty(Item.Title) ? Item.Artist : $"{Item.Artist} - {Item.Title}";
            Name.TruncateWithEllipsis((int)ProgressBar.Width - 10);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadProgressChanged(object sender,
            BindableValueChangedEventArgs<DownloadProgressEventArgs> e)
        {
            ScheduleUpdate(() =>
            {
                if (Item.FileDownloader.Value?.Status is FileDownloaderStatus.Complete
                    or FileDownloaderStatus.Cancelled)
                    return;
                var percent = e.Value.ProgressPercentage;
                var bytesPerSecond = e.Value.TimeElapsed == TimeSpan.Zero
                    ? 0
                    : e.Value.NewBytesWritten / e.Value.TimeElapsed.TotalSeconds;
                var bytesLeft = Item.FileDownloader.Value?.ContentLength - e.Value.BytesReceived ?? -1;
                var etaSeconds =
                    TimeSpan.FromSeconds(bytesPerSecond == 0 || bytesLeft == -1 ? 0 : bytesLeft / bytesPerSecond);

                if (e.Value.BytesReceived == 0)
                    ProgressBar.Bindable.Value = 0;

                ProgressBar.Bindable.Value = percent;
                ProgressPercentage.Text = $"{percent}%";
                EstimatedTimeLeft.Text = $"Downloading (ETA {etaSeconds.Minutes:00}:{etaSeconds.Seconds:00})";
            });
        }
    }
}
