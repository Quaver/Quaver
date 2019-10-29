using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Download;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Hub.Downloads.Scrolling
{
    public class DownloadScrollContainer : PoolableScrollContainer<MapsetDownload>
    {
        /// <summary>
        /// </summary>
        private SpriteTextPlus NoDownloadsText { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public DownloadScrollContainer(ScalableVector2 size) : base(MapsetDownloadManager.CurrentDownloads, int.MaxValue,
            0, size, size)
        {
            Tint = ColorHelper.HexToColor("#242424");
            Alpha = 0;

            Scrollbar.Width = 4;
            Scrollbar.Tint = Color.White;

            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            PaddingTop = 6;

            CreateNoDownloadsText();

            MapsetDownloadManager.DownloadAdded += OnDownloadAdded;
            CreatePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            NoDownloadsText.Visible = AvailableItems.Count == 0;

            base.Update(gameTime);
        }

        /// <summary>
        ///     Removes the download from the list
        /// </summary>
        /// <param name="download"></param>
        public void Remove(MapsetDownload download)
        {
            var item = Pool.Find(x => x.Item == download);
            AvailableItems.Remove(download);

            // Remove the item if it exists in the pool.
            if (item != null)
            {
                item.Destroy();
                RemoveContainedDrawable(item);
                Pool.Remove(item);
            }

            RecalculateContainerHeight();

            ScheduleUpdate(() =>
            {
                // Reset the pool item index
                for (var i = 0; i < Pool.Count; i++)
                {
                    Pool[i].Index = i;
                    Pool[i].ClearAnimations();
                    Pool[i].MoveToY((PoolStartingIndex + i) * Pool[i].HEIGHT, Easing.OutQuint, 400);
                    Pool[i].UpdateContent(Pool[i].Item, i);
                }
            });

            // Download the next map in the queue
            Pool.Find(x => !x.Item.IsDownloading)?.Item?.Download();

            // Automatically mark the section as read if there are no more downloads left
            if (AvailableItems.Count == 0)
            {
                var game = (QuaverGame) GameBase.Game;
                game.OnlineHub.Sections[OnlineHubSectionType.ActiveDownloads].MarkAsRead();
            }
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            MapsetDownloadManager.DownloadAdded -= OnDownloadAdded;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<MapsetDownload> CreateObject(MapsetDownload item, int index)
            => new DrawableDownload(this, item, index);

        /// <summary>
        /// </summary>
        private void CreateNoDownloadsText()
        {
            NoDownloadsText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "You currently do not have\n any active downloads!\n\nYou can download maps by clicking\nthe button in the top navigation bar.".ToUpper(), 20)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadAdded(object sender, MapsetDownloadAddedEventArgs e)
        {
            AddObjectToBottom(e.Download, false);

            var game = (QuaverGame) GameBase.Game;
            game.OnlineHub.MarkSectionAsUnread(OnlineHubSectionType.ActiveDownloads);
        }
    }
}