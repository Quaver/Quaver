using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting
{
    internal sealed class ChatInputBar : Textbox
    {

        private static string DefaultPlaceholderText => LocalizationManager.Get("Chat_SendAMessage");

        private Bindable<ChatChannel> ActiveChannel { get; }

        private long LastDisplayedMuteEndTime { get; set; }

        private int LastDisplayedMuteSecondsLeft { get; set; } = -1;

        public ChatInputBar(Bindable<ChatChannel> activeChannel, ScalableVector2 size) : base(size, FontManager.GetWobbleFont(Fonts.InterSemiBold), 18, string.Empty, DefaultPlaceholderText)
        {
            ActiveChannel = activeChannel;

            Tint = MonoGame.Extended.ColorHelper.FromHex("#181E25");
            InputText.Tint = MonoGame.Extended.ColorHelper.FromHex("#FFFFFF");
            Cursor.Tint = Color.White;
            InputText.X = 15;

            OnSubmit += SubmitMessage;
            ApplySize();
        }

        public override void Update(GameTime gameTime)
        {
            var muted = IsMuted();
            UpdateMutedState(muted);

            var canType = ActiveChannel.Value != null && !muted;
            InputEnabled = canType;
            AllowSubmission = canType;
            Button.IsInteractionEnabled = canType;

            if (!canType)
                Focused = false;

            base.Update(gameTime);

            if (muted)
                Cursor.Visible = false;
        }

        private void SubmitMessage(string text)
        {
            var channel = ActiveChannel.Value;
            if (channel == null)
                return;

            var user = OnlineManager.Self;

            // Used when there is no connected account
            if (user == null)
            {
                user = new User
                {
                    OnlineUser = new OnlineUser
                    {
                        Id = -1,
                        CountryFlag = "US",
                        SteamId = 0,
                        UserGroups = UserGroups.Admin,
                        Username = "God"
                    }
                };
            }

            foreach (var word in text.Split(' '))
            {
                if (EmojiHelper.Emojis.TryGetValue(word, out var emoji))
                    text = text.Replace(word, char.ConvertFromUtf32(emoji));
            }

            var message = new ChatMessage(user.OnlineUser.Id, user.OnlineUser.Username, user.OnlineUser.ClanTag, user.OnlineUser.ClanAccentColor, channel.Name, text, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                Sender = user,
                IsFromSelf = true
            };

            if (message.Message.StartsWith("/"))
            {
                QuaverBot.HandleClientSideCommands(message);
                return;
            }

            if (IsMuted())
                return;

            channel.QueueMessage(message);
            OnlineManager.Client?.SendMessage(channel.Name, message.Message);
        }

        private static bool IsMuted() => OnlineManager.Self != null && OnlineManager.Self.IsMuted;

        private void UpdateMutedState(bool muted)
        {
            if (!muted)
            {
                if (PlaceholderText != DefaultPlaceholderText)
                {
                    PlaceholderText = DefaultPlaceholderText;

                    if (string.IsNullOrEmpty(RawText))
                        RawText = string.Empty;
                }

                LastDisplayedMuteEndTime = 0;
                LastDisplayedMuteSecondsLeft = -1;
                return;
            }

            if (!string.IsNullOrEmpty(RawText))
                RawText = string.Empty;

            var muteEndTime = OnlineManager.Self.OnlineUser.MuteEndTime;
            var secondsLeft = Math.Max(0,
                (int)Math.Ceiling((muteEndTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / 1000d));

            if (muteEndTime == LastDisplayedMuteEndTime && secondsLeft == LastDisplayedMuteSecondsLeft)
                return;

            LastDisplayedMuteEndTime = muteEndTime;
            LastDisplayedMuteSecondsLeft = secondsLeft;
            PlaceholderText = $"You're muted. You can chat again in {FormatMuteTimeLeft(secondsLeft)}.";
            RawText = string.Empty;
        }

        private static string FormatMuteTimeLeft(int seconds)
        {
            var timeLeft = TimeSpan.FromSeconds(seconds);

            if (timeLeft.Days > 0)
                return $"{timeLeft.Days}d {timeLeft.Hours:00}h {timeLeft.Minutes:00}m {timeLeft.Seconds:00}s";

            if (timeLeft.Hours > 0)
                return $"{timeLeft.Hours:00}h {timeLeft.Minutes:00}m {timeLeft.Seconds:00}s";

            return $"{timeLeft.Minutes:00}m {timeLeft.Seconds:00}s";
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();

            if (Button != null)
                ApplySize();
        }
        private void ApplySize()
        {
            var texture = RoundedRectTextureCache.Get(Width, Height, 6);
            if (Image != texture)
                Image = texture;

            Button.Size = Size;
            ContentContainer.Size = Size;
        }

        public override void Destroy()
        {
            OnSubmit -= SubmitMessage;
            base.Destroy();
        }
    }
}
