using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Messages
{
    public class ChannelTopicHeader : ImageButton
    {
        /// <summary>
        /// </summary>
        private Bindable<ChatChannel> ActiveChannel { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Description { get; set; }

        /// <summary>
        /// </summary>
        private IconButton CloseButton { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="activeChannel"></param>
        /// <param name="size"></param>
        public ChannelTopicHeader(Bindable<ChatChannel> activeChannel, ScalableVector2 size) : base(UserInterface.TopicHeader)
        {
            ActiveChannel = activeChannel;
            Size = size;

            CreateName();
            CreateDescription();
            CreateCloseButton();

            SetText();

            ActiveChannel.ValueChanged += OnActiveChannelChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ActiveChannel.ValueChanged -= OnActiveChannelChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateName()  => Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = 16
        };

        /// <summary>
        /// </summary>
        private void CreateDescription() => Description = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            Tint = ColorHelper.HexToColor("#808080"),
            Y = 1
        };

        /// <summary>
        /// </summary>
        private void CreateCloseButton()
        {
            CloseButton = new IconButton(UserInterface.JudgementWindowCloseButton, (sender, args) => ActiveChannel.Value?.Close())
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X
            };

            const float scale = 0.75f;
            CloseButton.Size = new ScalableVector2(CloseButton.Image.Width * scale, CloseButton.Image.Height * scale);
        }

        /// <summary>
        /// </summary>
        private void SetText()
        {
            AddScheduledUpdate(() =>
            {
                Name.Text = ActiveChannel.Value == null ? "No Channel Selected" : ActiveChannel.Value.GetDisplayedName();
                Description.Text = ActiveChannel.Value != null ? ActiveChannel.Value.Description : "No Description";
                Description.X = Name.X + Name.Width + 20;
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnActiveChannelChanged(object sender, BindableValueChangedEventArgs<ChatChannel> e) => SetText();
    }
}