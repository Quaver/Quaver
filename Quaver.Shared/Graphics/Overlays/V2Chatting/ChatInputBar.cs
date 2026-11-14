using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Skinning.V2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.UI.Form;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting
{
    internal sealed class ChatInputBar : Textbox
    {
        private const int MaximumMessageLength = 500;

        private const UserGroups ChatModeratorGroups = UserGroups.Swan | UserGroups.Developer | UserGroups.Admin |
                                                      UserGroups.Moderator | UserGroups.Bot;

        private Bindable<ChatChannel> ActiveChannel { get; }

        private RoundedButton EmoteButton { get; }

        private IDisposable EmoteButtonTooltipRegistration { get; }

        private long LastDisplayedMuteEndTime { get; set; }

        private int LastDisplayedMuteSecondsLeft { get; set; } = -1;

        public bool IsEmoteButtonHovered => EmoteButton.IsHovered;

        public event EventHandler EmotePickerRequested;

        public ChatInputBar(Bindable<ChatChannel> activeChannel, ScalableVector2 size, SkinV2ChatInputConfig config, ChatEmoteAtlas atlas, SkinV2ChatEmotesConfig emoteConfig)
            : base(size, FontManager.GetWobbleFont(config.Font), config.FontSize, string.Empty, LocalizationManager.Get("Chat_SendAMessage"))
        {
            ActiveChannel = activeChannel;

            Tint = SkinV2Color.Parse(config.BackgroundColor);
            InputText.Tint = SkinV2Color.Parse(config.TextColor);
            Cursor.Tint = SkinV2Color.Parse(config.TextColor);
            InputText.X = 15;
            MaxCharacters = MaximumMessageLength;

            EmoteButton = new RoundedButton
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(size.Y.Value, size.Y.Value),
                CornerRadius = emoteConfig.CornerRadius,
                Tint = SkinV2Color.Parse(config.BackgroundColor)
            };
            EmoteButton.SetIcon(atlas.Get(ChatEmoteAtlas.Names[0]), new Vector2(emoteConfig.InputIconSize, emoteConfig.InputIconSize));
            EmoteButton.Clicked += OnEmoteButtonClicked;
            EmoteButtonTooltipRegistration = EmoteButton.AddTooltip(LocalizationManager.Get("Chat_Emotes"));

            OnSubmit += SubmitMessage;
            ApplySize();
        }

        public override void Update(GameTime gameTime)
        {
            var muted = IsMuted();
            var limitedChat = IsWriteRestricted(ActiveChannel.Value);
            UpdateInputState(muted, limitedChat);

            var canType = ActiveChannel.Value != null && !muted && !limitedChat;
            InputEnabled = canType;
            AllowSubmission = canType;
            Button.IsInteractionEnabled = canType;
            EmoteButton.IsInteractionEnabled = canType;

            if (!canType)
                Focused = false;

            base.Update(gameTime);

            if (!canType)
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
                if (ChatEmoteAtlas.TryGetName(word, out _))
                    continue;

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

            if (IsMuted() || IsWriteRestricted(channel))
                return;

            channel.QueueMessage(message);
            OnlineManager.Client?.SendMessage(channel.Name, message.Message);
        }

        public void AppendEmote(string name)
        {
            var alias = ChatEmoteAtlas.GetAlias(name);
            var separator = string.IsNullOrEmpty(RawText) || char.IsWhiteSpace(RawText[^1]) ? string.Empty : " ";
            if (RawText.Length + separator.Length + alias.Length > MaxCharacters)
                return;

            RawText += separator + alias;
            CursorPosition = RawText.Length;
            DeselectAndReadjust();
            Focused = true;
        }

        private void OnEmoteButtonClicked(object sender, EventArgs args) => EmotePickerRequested?.Invoke(this, EventArgs.Empty);

        private static bool IsMuted() => OnlineManager.Self != null && OnlineManager.Self.IsMuted;

        private static bool IsWriteRestricted(ChatChannel channel)
        {
            if (channel?.LimitedChat != true)
                return false;

            var userGroups = OnlineManager.Self?.OnlineUser?.UserGroups ?? 0;
            return (userGroups & ChatModeratorGroups) == 0;
        }

        private void UpdateInputState(bool muted, bool limitedChat)
        {
            if (!muted && !limitedChat)
            {
                PlaceholderText = LocalizationManager.Get("Chat_SendAMessage");

                if (string.IsNullOrEmpty(RawText))
                    RawText = string.Empty;

                LastDisplayedMuteEndTime = 0;
                LastDisplayedMuteSecondsLeft = -1;
                return;
            }

            if (!string.IsNullOrEmpty(RawText))
                RawText = string.Empty;

            if (limitedChat && !muted)
            {
                PlaceholderText = LocalizationManager.Get("Chat_CannotWriteInChannel");
                RawText = string.Empty;

                LastDisplayedMuteEndTime = 0;
                LastDisplayedMuteSecondsLeft = -1;
                return;
            }

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

            if (!IsDisposed && Button != null)
                ApplySize();
        }
        private void ApplySize()
        {
            var texture = RoundedRectTextureCache.Get(Width, Height, 6);
            if (Image != texture)
                Image = texture;

            Button.Size = Size;
            ContentContainer.Size = Size;
            if (EmoteButton != null)
                EmoteButton.Size = new ScalableVector2(Height, Height);
        }

        public override void Destroy()
        {
            OnSubmit -= SubmitMessage;
            EmoteButton.Clicked -= OnEmoteButtonClicked;

            EmoteButtonTooltipRegistration.Dispose();
            EmotePickerRequested = null;
            base.Destroy();
        }
    }
}
