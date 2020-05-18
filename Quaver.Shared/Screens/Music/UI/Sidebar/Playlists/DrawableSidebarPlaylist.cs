using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Music.UI.Sidebar.Playlists
{
    public sealed class DrawableSidebarPlaylist : PoolableSprite<Playlist>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 59;

        /// <summary>
        /// </summary>
        private ImageButton Button { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableSidebarPlaylist(PoolableScrollContainer<Playlist> container, Playlist item, int index) : base(container, item, index)
        {
            Alpha = 0;
            Size = new ScalableVector2(container.Width, HEIGHT);

            Button = new ImageButton(UserInterface.BlankBox, (sender, args) =>
            {
                if (PlaylistManager.Selected.Value == Item)
                    return;

                ConfigManager.SelectGroupMapsetsBy.Value = Item == null ? GroupMapsetsBy.None : GroupMapsetsBy.Playlists;
                PlaylistManager.Selected.Value = Item;
            })
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
                UsePreviousSpriteBatchOptions = true
            };

            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = Button,
                Alignment = Alignment.MidLeft,
                X = 16,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            FadeToColor(PlaylistManager.Selected.Value == Item || Button.IsHovered ? Colors.MainAccent : Color.White, gameTime.ElapsedGameTime.TotalMilliseconds, 30);
            Name.Tint = Tint;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(Playlist item, int index)
        {
            Item = item;
            Index = index;

            ScheduleUpdate(() =>
            {
                if (Item == null)
                {
                    Name.Text = "All Songs";
                    Name.Tint = PlaylistManager.Selected.Value == null ? Colors.MainAccent : Color.White;
                }
                else
                {
                    Name.Text = item.Name;
                    Name.TruncateWithEllipsis(240);
                    Name.Tint = PlaylistManager.Selected.Value == Item ? Colors.MainAccent : Color.White;
                }
            });
        }
    }
}