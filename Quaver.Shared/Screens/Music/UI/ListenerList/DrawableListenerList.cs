using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using TagLib.Id3v2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Music.UI.ListenerList
{
    public class DrawableListenerList : Sprite
    {
        /// <summary>
        /// </summary>
        private Sprite HeaderBackground { get; set; }

        /// <summary>
        /// </summary>
        private IconTextButton HeaderText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ListenerCount { get; set; }

        /// <summary>
        /// </summary>
        private Sprite DividerLine { get; set; }

        /// <summary>
        /// </summary>
        private ListenerListScrollContainer UserContainer { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public DrawableListenerList(ScalableVector2 size)
        {
            Size = size;

            Tint = ColorHelper.HexToColor("#202020");
            CreateHeader();
            CreateUserContainer();

            SubscribeToEvents();
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            UnsubscribeToEvents();
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            HeaderBackground = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(Width, 52),
                Tint = ColorHelper.HexToColor("#181818")
            };

            HeaderText = new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_music_headphones),
                FontManager.GetWobbleFont(Fonts.LatoBlack), "LISTENING PARTY", null, Color.White, Color.White)
            {
                Parent = HeaderBackground,
                Alignment = Alignment.MidLeft,
                X = 14
            };

            ListenerCount = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "0/16", 22)
            {
                Parent = HeaderBackground,
                Alignment = Alignment.MidRight,
                X = -HeaderText.X
            };

            if (OnlineManager.ListeningParty != null)
                ListenerCount.Text = $"{OnlineManager.ListeningParty.Listeners.Count}/16";

            DividerLine = new Sprite
            {
                Parent = HeaderBackground,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Width, 2),
                Alpha = 0.65f
            };
        }

        /// <summary>
        /// </summary>
        private void CreateUserContainer()
        {
            var height = Height - HeaderBackground.Height + DividerLine.Height;

            UserContainer = new ListenerListScrollContainer(new ScalableVector2(Width, height))
            {
                Parent = this,
                Y = HeaderBackground.Height
            };
        }

        /// <summary>
        /// </summary>
        private void SubscribeToEvents()
        {
            if (OnlineManager.Client == null)
                return;

            OnlineManager.Client.OnListeningPartyFellowJoined += OnListeningPartyFellowJoined;
            OnlineManager.Client.OnListeningPartyFellowLeft += OnListeningPartyFellowLeft;
        }

        /// <summary>
        /// </summary>
        private void UnsubscribeToEvents()
        {
            if (OnlineManager.Client == null)
                return;

            OnlineManager.Client.OnListeningPartyFellowJoined -= OnListeningPartyFellowJoined;
            OnlineManager.Client.OnListeningPartyFellowLeft -= OnListeningPartyFellowLeft;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListeningPartyFellowJoined(object sender, ListeningPartyFellowJoinedEventArgs e)
            => ScheduleUpdate(() => ListenerCount.Text = $"{OnlineManager.ListeningParty.Listeners.Count}/16");

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListeningPartyFellowLeft(object sender, ListeningPartyFellowLeftEventArgs e)
            => ScheduleUpdate(() => ListenerCount.Text = $"{OnlineManager.ListeningParty.Listeners.Count}/16");

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            // Reset the count
            ScheduleUpdate(() => ListenerCount.Text = $"0/16");

            // Resubscribe to online events
            if (e.Value == ConnectionStatus.Connected)
                SubscribeToEvents();
        }
    }
}