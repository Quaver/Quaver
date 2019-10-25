using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Online;
using SQLite;
using Wobble;
using Wobble.Bindables;
using Wobble.Discord.RPC;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Music.UI.ListenerList
{
    public sealed class DrawableListener : PoolableSprite<OnlineUser>
    {
        /// <summary>
        /// </summary>
        public static int ItemHeight { get; } = 61;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = ItemHeight;

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Avatar { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Flag { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Username { get; set; }

        /// <summary>
        /// </summary>
        private Sprite HostCrown { get; set; }

        /// <summary>
        /// </summary>
        private Sprite MissingSongIcon { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableListener(PoolableScrollContainer<OnlineUser> container, OnlineUser item, int index) : base(container, item, index)
        {
            Alpha = 0;
            Size = new ScalableVector2(container.Width, HEIGHT);

            CreateButton();
            CreateAvatar();
            CreateFlag();
            CreateUsername();
            CreateHostCrown();
            CreateMissingSongIcon();

            SteamManager.SteamUserAvatarLoaded += OnAvatarLoaded;
            OnlineManager.Client.OnUserInfoReceived += OnUserInfoReceived;
            OnlineManager.Client.OnListeningPartyChangeHost += OnHostChanged;
            MapManager.Selected.ValueChanged += OnMapChanged;
            OnlineManager.Client.OnListeningPartyUserMissingSong += OnListeningPartyUserMissingSong;
            OnlineManager.Client.OnListeningPartyStateUpdate += OnListeningPartyStateUpdate;
            OnlineManager.Client.OnListeningPartyUserHasSong += OnListeningPartyUserHasSong;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Button.Alpha = Button.IsHovered ? 0.45f : 0;
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            SteamManager.SteamUserAvatarLoaded -= OnAvatarLoaded;
            OnlineManager.Client.OnUserInfoReceived -= OnUserInfoReceived;
            MapManager.Selected.ValueChanged -= OnMapChanged;
            OnlineManager.Client.OnListeningPartyUserMissingSong -= OnListeningPartyUserMissingSong;
            OnlineManager.Client.OnListeningPartyStateUpdate -= OnListeningPartyStateUpdate;
            OnlineManager.Client.OnListeningPartyUserMissingSong -= OnListeningPartyUserMissingSong;
            OnlineManager.Client.OnListeningPartyChangeHost -= OnHostChanged;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(OnlineUser item, int index)
        {
            Item = item;
            Index = index;

            ScheduleUpdate(() =>
            {
                var steamId = (ulong) Item.SteamId;

                Avatar.ClearAnimations();

                if (SteamManager.UserAvatars.ContainsKey(steamId))
                {
                    Avatar.Image = SteamManager.UserAvatars[steamId];
                    Avatar.Alpha = 1;
                }
                else
                {
                    Avatar.Alpha = 0;
                    SteamManager.SendAvatarRetrievalRequest(steamId);
                }

                Flag.Image = Flags.Get(Item.CountryFlag ?? "XX");

                Username.Text = Item.Username ?? "Loading...";
                Username.Tint = Colors.GetUserChatColor(Item.UserGroups);
                Avatar.Border.Tint = Username.Tint;

                HostCrown.X = Username.X + Username.Width + 6;
                HostCrown.Visible = OnlineManager.ListeningParty.Host == Item;

                MissingSongIcon.X = HostCrown.X;
                MissingSongIcon.Visible = OnlineManager.ListeningParty.ListenersWithoutSong.Contains(Item) && !HostCrown.Visible;
            });
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
                Depth = 1,
                UsePreviousSpriteBatchOptions = true
            };

            var game = (QuaverGame) GameBase.Game;
            var container = (ListenerListScrollContainer) Container;

            Button.Clicked += (sender, args)
                => game.CurrentScreen.ActivateRightClickOptions(new DrawableListenerRightClickOptions(Item, container));

            Button.RightClicked += (sender, args)
                => game.CurrentScreen.ActivateRightClickOptions(new DrawableListenerRightClickOptions(Item, container));
        }

        /// <summary>
        /// </summary>
        private void CreateAvatar()
        {
            Avatar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(36, 36),
                X = 12,
                Image = UserInterface.YouAvatar,
                UsePreviousSpriteBatchOptions = true,
                SetChildrenAlpha = true,
            };

            Avatar.AddBorder(Color.White, 2);
            Avatar.Alpha = 0;
        }

        /// <summary>
        /// </summary>
        private void CreateFlag()
        {
            Flag = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(25, 25),
                X = Avatar.X + Avatar.Width + 10,
                Image = Flags.Get("XX"),
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateUsername()
        {
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Loading...", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Flag.X + Flag.Width + 8,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateHostCrown()
        {
            HostCrown = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Username.X + Username.Width + 10,
                Size = new ScalableVector2(14, 14),
                Image = UserInterface.HostCrown,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMissingSongIcon()
        {
            MissingSongIcon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = HostCrown.X,
                Size = HostCrown.Size,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_times),
                Tint = Color.Crimson,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
        {
            if (e.SteamId != (ulong) Item.SteamId)
                return;

            Avatar.ClearAnimations();
            Avatar.Image = e.Texture;
            Avatar.FadeTo(1, Easing.Linear, 200);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserInfoReceived(object sender, UserInfoEventArgs e)
        {
            if (e.Users.All(x => x.Id != Item.Id))
                return;

            UpdateContent(Item, Index);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHostChanged(object sender, ListeningPartyChangeHostEventArgs e) => UpdateContent(Item, Index);

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => UpdateContent(Item, Index);

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListeningPartyUserMissingSong(object sender, ListeningPartyUserMissingSongEventArgs e)
        {
            if (Item.Id != e.UserId)
                return;

            UpdateContent(Item, Index);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListeningPartyUserHasSong(object sender, ListeningPartyUserHasSongEventArgs e)
        {
            if (Item.Id != e.UserId)
                return;

            UpdateContent(Item, Index);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListeningPartyStateUpdate(object sender, ListeningPartyStateUpdateEventArgs e) => UpdateContent(Item, Index);
    }
}