using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Main;

namespace Quaver.Graphics.UserInterface.User
{
    internal class Playercard : Sprite
    {
        /// <summary>
        ///     The avatar of the player.
        /// </summary>
        private Sprite Avatar { get; }

        /// <summary>
        ///     The player's title.
        /// </summary>
        private Sprite Title { get; }

        /// <summary>
        ///     The username of the player.
        /// </summary>
        private SpriteText Username { get; }

        /// <summary>
        ///     The badge that signifiies their "rank" or "prestige"
        /// </summary>
        private Sprite RankBadge { get; }

        /// <summary>
        ///     The player's rank #
        /// </summary>
        private SpriteText Rank { get; }

        /// <summary>
        ///     The game mode the player currently has activated.
        /// </summary>
        private Sprite GameMode { get; }

        /// <summary>
        /// 
        /// </summary>
        internal Playercard()
        {
            Size = new UDim2D(325, 75);

            Tint = Color.Black;
            Alignment = Alignment.TopLeft;
            Alpha = 0.50f;
            
            Avatar = new Sprite()
            {
                Parent = this,
                Image = GameBase.QuaverUserInterface.YouAvatar,
                Size = new UDim2D(SizeY, SizeY),
                Alignment = Alignment.TopLeft
            };

            Title = new Sprite()
            {
                Parent = this,
                Image = Titles.OfflinePlayer,
                Size = new UDim2D(215, 25),
                Alignment = Alignment.TopLeft,
                PosX = Avatar.SizeX + 15,
                PosY = 5
            };

            RankBadge = new Sprite()
            {
                Parent = this,
                Image = FontAwesome.Archive,
                Size = new UDim2D(10, 10),
                PosX = Title.PosX + 2,
                PosY = Title.PosY + Title.SizeY + 8,
                Tint = Color.Gold
            };
            
            Username = new SpriteText()
            {
                Parent = this,
                Font = QuaverFonts.GoodTimes16,
                PosX = RankBadge.PosX + RankBadge.SizeX + 5,
                PosY = RankBadge.PosY - 2,
                Text = ConfigManager.Username.Value,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.TopLeft,
                TextColor = Color.White,
                TextScale = 0.55f
            };
            
            Rank = new SpriteText()
            {
                Parent = this,
                Font = QuaverFonts.GoodTimes16,
                PosX = RankBadge.PosX,
                PosY = RankBadge.PosY + 15,
                Text = "Rank: ",
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.TopLeft,
                TextColor = Color.White,
                TextScale = 0.55f
            };

            GameMode = new Sprite()
            {
                Parent = this,
                Image = FontAwesome.Desktop,
                Size = new UDim2D(20, 20),
                Alignment = Alignment.BotRight,
                Position = new UDim2D(-10, -10),
                Alpha = 0.75f
            };
        }
    }
}