using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.Multiplayer.UI.Feed
{
    public class MultiplayerFeed : Sprite
    {
        /// <summary>
        ///     All the items in the feed.
        /// </summary>
        private List<MultiplayerFeedItem> FeedItems { get; } = new List<MultiplayerFeedItem>();

        /// <summary>
        /// </summary>
        public MultiplayerFeed()
        {
            Size = new ScalableVector2(588, 150);
            Alpha = 0;

            var chat = ChatManager.JoinedChatChannels.Find(x => x.Name.StartsWith("#multiplayer"));

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
                Alignment = Alignment.BotLeft
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
                feedItem.MoveToY(-20 * (FeedItems.Count - i - 1), Easing.OutQuint, 200);
            }
        }
    }
}