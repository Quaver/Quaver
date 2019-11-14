using System;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Overlays.Chatting;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Wobble.Assets;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Multiplayer.UI.Feed
{
    public class MultiplayerFeed : Sprite
    {
        /// <summary>
        ///     All the items in the feed.
        /// </summary>
        private List<MultiplayerFeedItem> FeedItems { get; } = new List<MultiplayerFeedItem>();

        /// <summary>
        ///     Button to open the chat (and also highlight the box)
        /// </summary>
        private ImageButton OpenChatButton { get; }

        /// <summary>
        /// </summary>
        public MultiplayerFeed()
        {
            Size = new ScalableVector2(650, 125);
            Image = UserInterface.FeedPanel;

            OpenChatButton = new ImageButton(UserInterface.BlankBox, (o, e) => DialogManager.Show(new OnlineHubDialog()))
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(Width - 4, Height - 4),
                Alpha = 0
            };

            var chat = OnlineChat.JoinedChatChannels.Find(x => x.Name.StartsWith("#multiplayer"));

            if (chat == null)
                return;

            for (var i = chat.Messages.Count - 5; i >= 0; i++)
            {
                if (i < chat.Messages.Count)
                {
                    AddItem(Color.Yellow, $"[CHAT] {chat.Messages[i].Sender.OnlineUser.Username}: " +
                                          $"{string.Concat(chat.Messages[i].Message.Take(40))}{(chat.Messages[i].Message.Length >= 40 ? "..." : "")}");
                }

                if (FeedItems.Count == 5)
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            OpenChatButton.Alpha = MathHelper.Lerp(OpenChatButton.Alpha, OpenChatButton.IsHovered ? 0.4f : 0f,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));

            base.Update(gameTime);
        }

        /// <summary>
        ///     Adds an item to the feed
        /// </summary>
        /// <param name="color"></param>
        /// <param name="text"></param>
        public void AddItem(Color color, string text)
        {
            var item = new MultiplayerFeedItem(color, text)
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                X = 10
            };

            FeedItems.Add(item);

            if (FeedItems.Count == 6)
            {
                var first = FeedItems.First();
                first.Destroy();
                FeedItems.Remove(first);
            }

            for (var i = FeedItems.Count - 1; i >= 0; i--)
            {
                var feedItem = FeedItems[i];

                feedItem.ClearAnimations();
                feedItem.MoveToY(-22 * (FeedItems.Count - i - 1) - 11, Easing.OutQuint, 200);
            }
        }
    }
}