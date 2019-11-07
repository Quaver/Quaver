using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Twitch;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using TagLib.Riff;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Hub.SongRequests.Scrolling
{
    public class SongRequestScrollContainer : PoolableScrollContainer<SongRequest>
    {
        /// <summary>
        /// </summary>
        private SpriteTextPlus NoRequests { get; set; }

        /// <summary>
        ///     The currently active right click options for the screen
        /// </summary>
        public RightClickOptions ActiveRightClickOptions { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public SongRequestScrollContainer(ScalableVector2 size) : base(OnlineManager.SongRequests,int.MaxValue, 0, size, size)
        {
            Tint = ColorHelper.HexToColor("#242424");
            Alpha = 1;

            Scrollbar.Width = 4;
            Scrollbar.Tint = Color.White;

            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            InputEnabled = true;

            CreateNoRequestsText();
            CreatePool();

            SubscribeToEvents();
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            UpdateNoRequestsText();

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void SubscribeToEvents()
        {
            if (OnlineManager.Client == null || OnlineManager.Status.Value != ConnectionStatus.Connected)
                return;

            OnlineManager.Client.OnSongRequestReceived += OnSongRequestReceived;
        }

        /// <summary>
        /// </summary>
        private void UnsubscribeToEvents()
        {
            if (OnlineManager.Client == null)
                return;

            OnlineManager.Client.OnSongRequestReceived -= OnSongRequestReceived;
        }

        /// <summary>
        ///     Removes the song request from the list
        /// </summary>
        /// <param name="request"></param>
        public void Remove(SongRequest request)
        {
            var item = Pool.Find(x => x.Item == request);
            AvailableItems.Remove(request);

            // Remove the item if it exists in the pool.
            if (item != null)
            {
                item.Destroy();
                RemoveContainedDrawable(item);
                Pool.Remove(item);
            }

            RecalculateContainerHeight();

            // Reset the pool item index
            for (var i = 0; i < Pool.Count; i++)
            {
                Pool[i].Index = i;
                Pool[i].ClearAnimations();
                Pool[i].MoveToY((PoolStartingIndex + i) * Pool[i].HEIGHT, Easing.OutQuint, 400);
                Pool[i].UpdateContent(Pool[i].Item, i);
            }
        }

        /// <summary>
        ///     Removes all items from the pool
        /// </summary>
        public void RemoveAll()
        {
            AvailableItems.Clear();
            Pool.ForEach(x => x.Destroy());
            Pool.Clear();

            RecalculateContainerHeight();
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<SongRequest> CreateObject(SongRequest item, int index) => new DrawableSongRequest(this, item, index);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<SongRequest> GetTestRequests()
        {
            var requests = new List<SongRequest>();

            for (var i = 0; i < 20; i++)
            {
                requests.Add(new SongRequest()
                {
                    TwitchUsername = $"User#{i}",
                    UserId = i % 2 == 0 ? i : -1,
                    Artist = $"Artist",
                    Title = $"Title-{i}",
                    DifficultyName = "Easy",
                    Creator = $"John Doe",
                    DifficultyRating = 3.5f * i,
                    MapId = i % 2 == 0 ? -1 : i
                });
            }

            return requests;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSongRequestReceived(object sender, SongRequestEventArgs e)
        {
            AddObjectToBottom(e.Request, false);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (e.Value == ConnectionStatus.Connected)
                SubscribeToEvents();
            else
                UnsubscribeToEvents();
        }

        /// <summary>
        /// </summary>
        private void CreateNoRequestsText()
        {
            NoRequests = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center
            };
        }

        /// <summary>
        /// </summary>
        private void UpdateNoRequestsText()
        {
            const string noRequests = "YOU CURRENTLY DO NOT HAVE ANY\nSONG REQUESTS!";
            const string notLoggedIn = "YOU MUST BE LOGGED IN TO VIEW\nSONG REQUESTS!";

            NoRequests.Visible = AvailableItems.Count == 0;

            if (!OnlineManager.Connected)
                NoRequests.Text = notLoggedIn;
            else if (AvailableItems.Count == 0)
                NoRequests.Text = noRequests;
        }

        /// <summary>
        /// </summary>
        /// <param name="rco"></param>
        public void ActivateRightClickOptions(RightClickOptions rco)
        {
            if (ActiveRightClickOptions != null)
            {
                ActiveRightClickOptions.Visible = false;
                ActiveRightClickOptions.Parent = null;
                ActiveRightClickOptions.Destroy();
            }

            ActiveRightClickOptions = rco;
            ActiveRightClickOptions.Parent = this;

            ActiveRightClickOptions.ItemContainer.Height = 0;
            ActiveRightClickOptions.Visible = true;

            var x = MathHelper.Clamp(MouseManager.CurrentState.X - ActiveRightClickOptions.Width - AbsolutePosition.X, 0,
                Width - ActiveRightClickOptions.Width);

            var y = MathHelper.Clamp(MouseManager.CurrentState.Y - AbsolutePosition.Y, 0,
                Height - ActiveRightClickOptions.Items.Count * ActiveRightClickOptions.Items.First().Height - 60);

            ActiveRightClickOptions.Position = new ScalableVector2(x, y);
            ActiveRightClickOptions.Open(350);
        }
    }
}