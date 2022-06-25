using System.Net;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
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
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 100;

        /// <summary>
        /// </summary>
        private Sprite ContentContainer { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        private ProgressBar ProgressBar { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ProgressPercentage { get; set; }

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
            CreateButton();
            CreateSongName();
            CreateProgressBar();
            CreateProgressPercentage();

            UpdateText();

            Item.Progress.ValueChanged += OnDownloadProgressChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Button.Alpha = Button.IsHovered ? 0.35f : 0;

            var game = (QuaverGame) GameBase.Game;

            if (Container != null)
                Button.IsClickable = game.OnlineHub.SelectedSection == game.OnlineHub.Sections[OnlineHubSectionType.ActiveDownloads];

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
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(Width * 0.94f, HEIGHT * 0.85f),
                Image = UserInterface.HubDownloadContainer,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = ContentContainer,
                Alignment = Alignment.MidCenter,
                Alpha = 0,
                Size = new ScalableVector2(ContentContainer.Width - 2, ContentContainer.Height - 2)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateSongName()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Artist - Title", 22)
            {
                Parent = ContentContainer,
                Alignment = Alignment.MidLeft,
                X = 18,
                Y = -14,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateProgressBar()
        {
            ProgressBar = new ProgressBar(new Vector2(Width - 110, 14), 0, 100, 0, Color.Transparent,
                Colors.MainAccent)
            {
                Parent = ContentContainer,
                Alignment = Alignment.MidLeft,
                X = Name.X,
                Y = -Name.Y,
                UsePreviousSpriteBatchOptions = true,
                ActiveBar =
                {
                    UsePreviousSpriteBatchOptions = true
                },
            };

            ProgressBar.AddBorder(Colors.MainBlue, 2);
        }

        /// <summary>
        /// </summary>
        private void CreateProgressPercentage()
        {
            ProgressPercentage = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "0%", 20)
            {
                Parent = ContentContainer,
                Alignment = Alignment.MidLeft,
                Y = ProgressBar.Y + 1,
                X = ProgressBar.X + ProgressBar.Width + 14,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void UpdateText()
        {
            Name.Text = string.IsNullOrEmpty(Item.Title) ? Item.Artist : $"{Item.Artist} - {Item.Title}";
            Name.TruncateWithEllipsis((int) ProgressBar.Width - 10);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadProgressChanged(object sender, BindableValueChangedEventArgs<DownloadProgressChangedEventArgs> e)
        {
            ScheduleUpdate(() =>
            {
                var percent = e.Value.ProgressPercentage;

                if (e.Value.BytesReceived == 0)
                    ProgressBar.Bindable.Value = 0;

                ProgressBar.Bindable.Value = percent;
                ProgressPercentage.Text = $"{percent}%";
            });
        }
    }
}