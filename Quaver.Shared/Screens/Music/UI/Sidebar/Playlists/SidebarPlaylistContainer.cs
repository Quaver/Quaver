using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Music.UI.Sidebar.Playlists
{
    public class SidebarPlaylistContainer : PoolableScrollContainer<Playlist>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public SidebarPlaylistContainer(ScalableVector2 size) : base(GetAvailablePlaylists(), 8,
            0, size, size)
        {
            Alpha = 0;

            Scrollbar.Width = 4;
            Scrollbar.Tint = Color.White;
            
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;
            
            CreatePool();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);
            
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<Playlist> CreateObject(Playlist item, int index) => new DrawableSidebarPlaylist(this, item, index);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<Playlist> GetAvailablePlaylists()
        {
            var playlists = new List<Playlist>();

            // Add a null playlist to represent "all songs"
            playlists.Add(null);
            playlists = playlists.Concat(PlaylistManager.Playlists).ToList();

            return playlists;
        }
    }
}