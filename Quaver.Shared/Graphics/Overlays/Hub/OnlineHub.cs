using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Overlays.Hub.Downloads;
using Quaver.Shared.Graphics.Overlays.Hub.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers;
using Quaver.Shared.Graphics.Overlays.Hub.SongRequests;
using Quaver.Shared.Helpers;
using TagLib.Id3v2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Overlays.Hub
{
    public class OnlineHub : Sprite
    {
        /// <summary>
        ///     If the hub is currently open
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// </summary>
        private Sprite Background { get; set; }

        /// <summary>
        /// </summary>
        public Sprite HeaderBackground { get; private set; }

        /// <summary>
        /// </summary>
        public SpriteTextPlus HeaderText { get; private set; }

        /// <summary>
        /// </summary>
        public Sprite IconContainer { get; private set; }

        /// <summary>
        /// </summary>
        public Dictionary<OnlineHubSectionType, OnlineHubSection> Sections { get; private set; }

        /// <summary>
        /// </summary>
        public OnlineHubSection SelectedSection { get; private set; }

        /// <summary>
        /// </summary>
        public static int WIDTH { get; } = 447;

        /// <summary>
        /// </summary>
        public OnlineHub()
        {
            Tint = ColorHelper.HexToColor("#242424");
            Size = new ScalableVector2(WIDTH, WindowManager.Height - MenuBorder.HEIGHT);

            DestroyIfParentIsNull = false;

            CreateBackground();
            CreateHeaderBackground();
            CreateHeaderText();
            CreateIconContainer();
            CreateSections();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Height = WindowManager.Height - MenuBorder.HEIGHT;

            foreach (var section in Sections)
                section.Value.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Performs an open animation
        /// </summary>
        public void Open()
        {
            IsOpen = true;

            ClearAnimations();
            MoveToX(0, Easing.OutQuint, 500);
        }

        /// <summary>
        ///     Performs a close animation
        /// </summary>
        public void Close()
        {
            IsOpen = false;

            ClearAnimations();
            MoveToX(Width + 10, Easing.OutQuint, 500);
        }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        public void SelectSection(OnlineHubSectionType type) => SelectSection(Sections[type]);

        /// <summary>
        /// </summary>
        /// <param name="section"></param>
        public void SelectSection(OnlineHubSection section)
        {
            if (SelectedSection == section)
                return;

            var oldSection = SelectedSection;
            SelectedSection = section;
            SelectedSection.MarkAsRead();

            section.Container.Parent = this;

            if (oldSection != null)
                oldSection.Container.Parent = null;

            ScheduleUpdate(() => HeaderText.Text = section.Name);
        }

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new Sprite
        {
            Parent = this,
            Image = UserInterface.HubTriangles,
            Size = Size
        };

        /// <summary>
        /// </summary>
        private void CreateHeaderBackground() => HeaderBackground = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(Width, 56),
            Image = UserInterface.HubHeaderBackground
        };

        /// <summary>
        /// </summary>
        private void CreateHeaderText()
        {
            HeaderText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "ONLINE HUB", 22)
            {
                Parent = HeaderBackground,
                Alignment = Alignment.MidLeft,
                X = 22
            };
        }

        /// <summary>
        /// </summary>
        private void CreateIconContainer()
        {
            IconContainer = new Sprite()
            {
                Parent = this,
                Y = HeaderBackground.Height,
                Size = HeaderBackground.Size,
                Tint = ColorHelper.HexToColor("#181818")
            };
        }

        /// <summary>
        /// </summary>
        private void CreateSections()
        {
            Sections = new Dictionary<OnlineHubSectionType, OnlineHubSection>()
            {
                {OnlineHubSectionType.Notifications, new OnlineHubSectionNotifications(this)},
                {OnlineHubSectionType.ActiveDownloads, new OnlineHubSectionDownloads(this)},
                {OnlineHubSectionType.OnlineUsers, new OnlineHubSectionOnlineUsers(this)},
                {OnlineHubSectionType.SongRequests, new OnlineHubSectionSongRequests(this)}
            };

            for (var i = 0; i < Sections.Count; i++)
            {
                var section = Sections[(OnlineHubSectionType)i];

                // Update the container once
                section.Container.Update(new GameTime());

                section.Container.Parent = null;
                section.Container.Y = IconContainer.Y + IconContainer.Height;

                section.Icon.Parent = IconContainer;
                section.Icon.Alignment = Alignment.MidLeft;

                var width = (Width - 28) / Sections.Count;
                section.Icon.X = width * (i + 1) - 56;
            }

            SelectSection(Sections[OnlineHubSectionType.OnlineUsers]);
        }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="force"></param>
        public void MarkSectionAsUnread(OnlineHubSectionType type, bool force = false)
        {
            if (SelectedSection == Sections[type] && !force)
            {
                SelectedSection.MarkAsRead();
                return;
            }

            Sections[type].MarkAsUnread();
        }
    }
}