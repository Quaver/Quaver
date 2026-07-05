using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Messages.Textbox
{
    public class OnlineChatTextbox : Wobble.Graphics.UI.Form.Textbox
    {
        /// <summary>
        /// </summary>
        private Bindable<ChatChannel> ActiveChannel { get; }

        /// <summary>
        ///     If the chat box is standalone and separate form the chat
        /// </summary>
        private bool Standalone { get; }

        /// <summary>
        /// </summary>
        private const string DefaultPlaceholderTextKey = "Chat_SendAMessage";

        /// <summary>
        /// </summary>
        private static string DefaultPlaceholderText => LocalizationManager.Get(DefaultPlaceholderTextKey);

        /// <summary>
        /// </summary>
        private long LastDisplayedMuteEndTime { get; set; }

        /// <summary>
        /// </summary>
        private int LastDisplayedMuteSecondsLeft { get; set; } = -1;

        /// <summary>
        /// </summary>
        private bool? AlwaysFocusedBeforeMute { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="activeChatChannel"></param>
        /// <param name="size"></param>
        /// <param name="standalone"></param>
        public OnlineChatTextbox(Bindable<ChatChannel> activeChatChannel, ScalableVector2 size, bool standalone = false)
            : base(size, FontManager.GetWobbleFont(Fonts.InterBold), 20, "", LocalizationManager.Get(DefaultPlaceholderTextKey))
        {
            ActiveChannel = activeChatChannel;
            Standalone = standalone;

            Image = UserInterface.SearchBox;
            Tint = ColorHelper.HexToColor("#181818");

            OnSubmit += msg =>
            {
                if (ActiveChannel.Value == null)
                    return;

                var user = OnlineManager.Self;

                // ONLY USED FOR TESTING
                if (user == null)
                {
                    user = new User()
                    {
                        OnlineUser = new OnlineUser()
                        {
                            Id = -1,
                            CountryFlag = "US",
                            SteamId = 0,
                            UserGroups = UserGroups.Admin,
                            Username = "God"
                        }
                    };
                }

                foreach (var word in msg.Split(" "))
                {
                    if (!EmojiHelper.Emojis.ContainsKey(word))
                        continue;

                    msg = msg.Replace(word, char.ConvertFromUtf32(EmojiHelper.Emojis[word]));
                }

                var message = new ChatMessage(user.OnlineUser.Id, user.OnlineUser.Username,
                    user.OnlineUser.ClanTag, user.OnlineUser.ClanAccentColor,
                    ActiveChannel.Value.Name, msg, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
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

                ActiveChannel.Value?.QueueMessage(message);
                OnlineManager.Client?.SendMessage(ActiveChannel.Value.Name, message.Message);
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var muted = IsMuted();
            UpdateMutedState(muted);
            
            if (Standalone && DialogManager.Dialogs.Count != 0)
            {
                AlwaysFocused = false;
                InputEnabled = !muted;
                Focused = !muted;
                AllowSubmission = false;
            }
            else
            {
                AllowSubmission = true;
            }

            if (!Standalone && OnlineChat.Instance != null)
            {
                if (OnlineChat.Instance.IsOpen && OnlineChat.Instance.IsHovered())
                {
                    InputEnabled = !muted;
                    Focused = !muted;
                }
                else if (!OnlineChat.Instance.IsOpen)
                {
                    InputEnabled = false;
                    Focused = false;
                }
            }

            if (muted)
            {
                InputEnabled = false;
                Focused = false;
            }
            else if (Standalone)
                InputEnabled = true;

            base.Update(gameTime);

            if (muted)
                Cursor.Visible = false;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static bool IsMuted() => OnlineManager.Self != null && OnlineManager.Self.IsMuted;

        /// <summary>
        /// </summary>
        /// <param name="muted"></param>
        private void UpdateMutedState(bool muted)
        {
            if (!muted)
            {
                if (AlwaysFocusedBeforeMute != null)
                {
                    AlwaysFocused = AlwaysFocusedBeforeMute.Value;
                    AlwaysFocusedBeforeMute = null;
                }

                if (PlaceholderText != DefaultPlaceholderText)
                {
                    PlaceholderText = DefaultPlaceholderText;

                    if (string.IsNullOrEmpty(RawText))
                        RawText = "";
                }

                LastDisplayedMuteEndTime = 0;
                LastDisplayedMuteSecondsLeft = -1;
                AllowSubmission = true;
                return;
            }

            AllowSubmission = false;

            if (AlwaysFocusedBeforeMute == null)
                AlwaysFocusedBeforeMute = AlwaysFocused;

            AlwaysFocused = false;
            Cursor.Visible = false;

            if (!string.IsNullOrEmpty(RawText))
                RawText = "";

            var muteEndTime = OnlineManager.Self.OnlineUser.MuteEndTime;
            var secondsLeft = Math.Max(0, (int) Math.Ceiling((muteEndTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / 1000d));

            if (muteEndTime == LastDisplayedMuteEndTime && secondsLeft == LastDisplayedMuteSecondsLeft)
                return;

            LastDisplayedMuteEndTime = muteEndTime;
            LastDisplayedMuteSecondsLeft = secondsLeft;
            PlaceholderText = $"You're muted. You can chat again in {FormatMuteTimeLeft(secondsLeft)}.";
            RawText = "";
        }

        /// <summary>
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        private static string FormatMuteTimeLeft(int seconds)
        {
            var timeLeft = TimeSpan.FromSeconds(seconds);

            if (timeLeft.Days > 0)
                return $"{timeLeft.Days}d {timeLeft.Hours:00}h {timeLeft.Minutes:00}m {timeLeft.Seconds:00}s";

            if (timeLeft.Hours > 0)
                return $"{timeLeft.Hours:00}h {timeLeft.Minutes:00}m {timeLeft.Seconds:00}s";

            return $"{timeLeft.Minutes:00}m {timeLeft.Seconds:00}s";
        }
    }
}
