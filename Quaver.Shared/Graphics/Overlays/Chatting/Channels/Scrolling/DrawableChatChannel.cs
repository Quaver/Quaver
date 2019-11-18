using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Channels.Scrolling
{
    public sealed class DrawableChatChannel : PoolableSprite<ChatChannel>
    {
        /// <summary>
        /// </summary>
        private Bindable<ChatChannel> ActiveChannel { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 62;

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        private Sprite MentionedIcon { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="activeChannel"></param>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableChatChannel(Bindable<ChatChannel> activeChannel, PoolableScrollContainer<ChatChannel> container,
            ChatChannel item, int index) : base(container, item, index)
        {
            ActiveChannel = activeChannel;
            Alpha = 0;
            Tint = index % 2 == 0 ? Colors.DarkGray : Colors.BlueishDarkGray;

            Size = new ScalableVector2(container.Width, HEIGHT);

            CreateButton();
            CreateName();
            CreateMentionedIcon();

            ActiveChannel.ValueChanged += OnActiveChannelChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (ActiveChannel.Value == Item)
                Button.Alpha = 0.4f;
            else if (Button.IsHovered)
                Button.Alpha = 0.35f;
            else
                Button.Alpha = 0;

            var container = (ChatChannelScrollContainer) Container;

            Button.Depth = container.ActiveRightClickOptions != null && container.ActiveRightClickOptions.Opened ? 1 : 0;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Dispose()
        {
            // ReSharper disable once DelegateSubtraction
            ActiveChannel.ValueChanged -= OnActiveChannelChanged;

            base.Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(ChatChannel item, int index)
        {
            Item = item;
            Index = index;

            AddScheduledUpdate(() =>
            {
                Name.Text = Item.GetDisplayedName();
                Name.Tint = GetTextColor();

                MentionedIcon.X = Name.X + Name.Width + 8;
                MentionedIcon.Visible = Item.IsMentioned;
            });
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new DrawableChatChannelButton(UserInterface.BlankBox, Container)
            {
                Parent = this,
                Size = Size,
                UsePreviousSpriteBatchOptions = true,
                Alpha = 0
            };

            Button.Clicked += (sender, args) => ActiveChannel.Value = Item;

            Button.RightClicked += (sender, args) =>
            {
                var container = Container as ChatChannelScrollContainer;
                container?.ActivateRightClickOptions(new ChatChannelRightClickOptions(Item, ActiveChannel, (ChatChannelScrollContainer) Container));
            };
        }

        /// <summary>
        /// </summary>
        private void CreateName()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 16,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMentionedIcon()
        {
            MentionedIcon = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(10, 10),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_circle),
                Tint = Color.Orange
            };
        }

        /// <summary>
        ///    Returns the appropriate text color based on the channel's state
        /// </summary>
        /// <returns></returns>
        private Color GetTextColor()
        {
            if (Item == ActiveChannel.Value)
                return Color.White;
            if (Item.IsUnread)
                return Colors.SecondaryAccent;

            return Color.White;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnActiveChannelChanged(object sender, BindableValueChangedEventArgs<ChatChannel> e)
        {
            if (e.Value == Item)
            {
                Item.IsUnread = false;
                Item.IsMentioned = false;
            }

            UpdateContent(Item, Index);
        }
    }
}