using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Wobble;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Music.UI.Sidebar
{
    public class RecentlyPlayedSong : ImageButton
    {
        /// <summary>
        /// </summary>
        private Map Map { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Text { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        public RecentlyPlayedSong(Map map) : base(UserInterface.BlankBox)
        {
            Map = map;
            Alpha = 0;

            Text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22) { Parent = this };
            SetMap(Map);

            Clicked += (sender, args) =>
            {
                if (Map == null)
                    return;

                if (!OnlineManager.IsListeningPartyHost)
                {
                    NotificationManager.Show(NotificationLevel.Error, "You are not the host of the listening party!");
                    return;
                }

                MapManager.Selected.Value = Map;
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            FadeToColor(IsHovered ? Colors.MainAccent : Color.White, gameTime.ElapsedGameTime.TotalMilliseconds, 30);
            Text.Tint = Tint;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        public void SetMap(Map map)
        {
            Map = map;

            if (Map == null)
            {
                ScheduleUpdate(() =>
                {
                    Text.Text = "";
                    Size = Text.Size;
                });
                return;
            }

            ScheduleUpdate(() =>
            {
                Text.Text = $"{Map.Title}";
                Text.TruncateWithEllipsis(240);

                Size = Text.Size;
            });
        }
    }
}