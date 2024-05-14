using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Download;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Logging;
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

            ReorganizeItems();

            DownloadNextItem();

            // Automatically mark the section as read if there are no more downloads left
            if (AvailableItems.Count == 0)
            {
                var game = (QuaverGame) GameBase.Game;
                game.OnlineHub.Sections[OnlineHubSectionType.ActiveDownloads].MarkAsRead();
            }
        }

        internal void DownloadNextItem()
        {
            // Download the next map in the queue
            Pool.FindLast(x => !x.Item.HasDownloadEverStarted)?.Item?.Download();
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
        {
            var drawableDownload = new DrawableDownload(this, item, index);
            drawableDownload.DimensionsChanged += (sender, args) =>
            {
                ReorganizeItems();
            };
            return drawableDownload;
        }

        /// <summary>
        /// </summary>
        private void CreateNoDownloadsText()
        {
            NoDownloadsText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),"", 20)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center
            };

            AddScheduledUpdate(() =>
            {
                NoDownloadsText.Text = "You currently do not have\n any active downloads!\n\n".ToUpper() +
                                       "You can download maps by clicking\nthe button in the top navigation bar.".ToUpper();
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadAdded(object sender, MapsetDownloadAddedEventArgs e)
        {
            e.Download.Removed += (o, args) =>
            {
                Remove(e.Download);
            };
            AddObjectAtIndex(0, e.Download, false, true);
            ReorganizeItems();

            // Running update once immediately here to make sure everything scheduled is initialized properly
            Update(new GameTime());

            var game = (QuaverGame) GameBase.Game;
            game.OnlineHub.MarkSectionAsUnread(OnlineHubSectionType.ActiveDownloads);
        }
        

        public void ReorganizeItems()
        {
            RecalculateContainerHeight();

            AddScheduledUpdate(() =>
            {
                if (Pool.Count == 0) 
                    return;

                var y = 0;
                // Reset the pool item index
                for (var i = 0; i < Pool.Count; i++)
                {
                    Pool[i].Index = i;
                    Pool[i].ClearAnimations();
                    Pool[i].MoveToY(y, Easing.OutQuint, 400);
                    Pool[i].UpdateContent(Pool[i].Item, i);
                    y += Pool[i].HEIGHT;
                }
            });
        }
    }
}