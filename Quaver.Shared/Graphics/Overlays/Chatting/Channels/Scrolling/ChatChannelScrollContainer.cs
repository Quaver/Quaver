using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Online.Chat;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Input;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Channels.Scrolling
{
    public class ChatChannelScrollContainer : PoolableScrollContainer<ChatChannel>, IResizable
    {
        /// <summary>
        /// </summary>
        private Bindable<ChatChannel> ActiveChatChannel { get; }

        /// <summary>
        ///     The size of the header. Used to determine the amount we should subtract when
        ///     resizing the container
        /// </summary>
        private float HeaderHeight { get; }

        /// <summary>
        ///     The currently active right click options for the screen
        /// </summary>
        public RightClickOptions ActiveRightClickOptions { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="activeChannel"></param>
        /// <param name="headerHeight"></param>
        /// <param name="size"></param>
        public ChatChannelScrollContainer(Bindable<ChatChannel> activeChannel, float headerHeight, ScalableVector2 size)
            : base(ChatSession.JoinedChannels.Value, int.MaxValue, 0, size, size)
        {
            ActiveChatChannel = activeChannel;
            HeaderHeight = headerHeight;
            Alpha = 0;

            Scrollbar.Width = 4;
            Scrollbar.Tint = Color.White;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            CreatePool();

            ChatSession.JoinedChannels.ItemAdded += OnChannelAdded;
            ChatSession.JoinedChannels.ItemRemoved += OnChannelRemoved;
            ChatSession.ChannelUpdated += OnChannelUpdated;
        }

        public override void Destroy()
        {
            ChatSession.JoinedChannels.ItemAdded -= OnChannelAdded;
            ChatSession.JoinedChannels.ItemRemoved -= OnChannelRemoved;
            ChatSession.ChannelUpdated -= OnChannelUpdated;
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            HandleKeyPressTab();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<ChatChannel> CreateObject(ChatChannel item, int index)
            => new DrawableChatChannel(ActiveChatChannel, this, item, index);

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public void ChangeSize(ScalableVector2 size)
        {
            Height = size.Y.Value - HeaderHeight;
            RecalculateContainerHeight();
        }

        /// <summary>
        ///     Adds a chat channel to the list
        /// </summary>
        /// <param name="chan"></param>
        private void Add(ChatChannel chan)
        {
            AddObjectToBottom(chan, false);
        }

        /// <summary>
        ///     Removes a chat channel from the list
        /// </summary>
        /// <param name="chan"></param>
        public void Remove(ChatChannel chan)
        {
            var item = Pool.Find(x => x.Item == chan);

            // Remove the item if it exists in the pool.
            if (item != null)
            {
                item.Destroy();
                RemoveContainedDrawable(item);
                Pool.Remove(item);
            }

            RecalculateContainerHeight();

            for (var i = 0; i < Pool.Count; i++)
            {
                Pool[i].Index = i;
                Pool[i].ClearAnimations();
                Pool[i].MoveToY((PoolStartingIndex + i) * Pool[i].HEIGHT, Easing.OutQuint, 400);
                Pool[i].UpdateContent(Pool[i].Item, i);
            }

        }

        private void OnChannelAdded(object sender, BindableListItemAddedEventArgs<ChatChannel> e) => Add(e.Item);

        private void OnChannelRemoved(object sender, BindableListItemRemovedEventArgs<ChatChannel> e) => Remove(e.Item);

        private void OnChannelUpdated(ChatChannel channel)
        {
            var drawable = Pool.Find(x => x.Item == channel);
            drawable?.UpdateContent(drawable.Item, drawable.Index);
        }

        /// <summary>
        /// </summary>
        /// <param name="rco"></param>
        public void ActivateRightClickOptions(RightClickOptions rco)
        {
            if (ActiveRightClickOptions != null)
            {
                DismissActiveRightClickOptions();
            }

            ActiveRightClickOptions = rco;
            ActiveRightClickOptions.Parent = this;

            ActiveRightClickOptions.ItemContainer.Height = 0;
            ActiveRightClickOptions.Visible = true;

            var x = MathHelper.Clamp(MouseManager.CurrentState.X - ActiveRightClickOptions.Width - AbsolutePosition.X, 0,
                Width - ActiveRightClickOptions.Width);

            var y = MathHelper.Clamp(MouseManager.CurrentState.Y - AbsolutePosition.Y, 0,
                Height - ActiveRightClickOptions.Items.Count * ActiveRightClickOptions.Items.First().Height);

            ActiveRightClickOptions.Position = new ScalableVector2(x, y);
            ActiveRightClickOptions.Open(350);
        }

        /// <summary>
        /// </summary>
        public void DismissActiveRightClickOptions()
        {
            if (ActiveRightClickOptions == null)
                return;

            ActiveRightClickOptions.Visible = false;
            ActiveRightClickOptions.Parent = null;
            ActiveRightClickOptions.Destroy();
            ActiveRightClickOptions = null;
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressTab()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Tab))
                return;

            if (AvailableItems.Count == 0)
                return;

            var index = AvailableItems.IndexOf(ActiveChatChannel.Value);

            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftShift) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightShift))
            {
                ActiveChatChannel.Value = index - 1 >= 0 ? AvailableItems[index - 1] : AvailableItems[AvailableItems.Count - 1];
                return;
            }

            ActiveChatChannel.Value = index + 1 < AvailableItems.Count ? AvailableItems[index + 1] : AvailableItems.First();
        }
    }
}
