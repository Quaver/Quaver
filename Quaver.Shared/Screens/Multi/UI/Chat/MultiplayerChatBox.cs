using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Overlays.Chatting;
using Quaver.Shared.Graphics.Overlays.Chatting.Messages.Scrolling;
using Quaver.Shared.Graphics.Overlays.Chatting.Messages.Textbox;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Multi.UI.Players;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Multi.UI.Chat
{
    public class MultiplayerChatBox : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        private ChatMessageScrollContainer MessageContainer { get; set; }

        /// <summary>
        /// </summary>
        private OnlineChatTextbox Textbox { get; set; }

        /// <summary>
        /// </summary>
        public MultiplayerChatBox(Bindable<MultiplayerGame> game)
        {
            Game = game;
            Size = new ScalableVector2(MultiplayerPlayerList.ContainerSize.X.Value, 344);
            Image = UserInterface.MultiplayerChatBox;
            Tint = ColorHelper.HexToColor("#242424");

            CreateTextbox();
            CreateMessageContainer();
        }

        public override void Update(GameTime gameTime)
        {
            Textbox.AlwaysFocused = DialogManager.Dialogs.Count == 0;

            if (DialogManager.Dialogs.Count == 0)
                Textbox.Focused = false;
            
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateMessageContainer()
        {
            MessageContainer = new ChatMessageScrollContainer(GetChatChannel(),
                new ScalableVector2(Textbox.Width, Height - 64), 0, 50)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 0,
                Alpha = 0
            };
        }

        /// <summary>
        ///     Creates <see cref="Textbox"/>
        /// </summary>
        private void CreateTextbox()
        {
            var chan = new Bindable<ChatChannel>(GetChatChannel()) {Value = GetChatChannel()};

            Textbox = new OnlineChatTextbox(chan, new ScalableVector2(Width - 20, 40), true)
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Y = -12,
                InputEnabled = true,
                Focused = true
            };
        }

        /// <summary>
        ///     Retrieves the multiplayer chat channel from the list of channels
        /// </summary>
        /// <returns></returns>
        private ChatChannel GetChatChannel()
        {
            return OnlineChat.JoinedChatChannels.Find(x => x.Name == $"#multiplayer_{Game.Value.Id}")
                   ?? new ChatChannel {Name = "No Chat"};
        }
    }
}