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
        private Sprite Avatar { get; }

        private Sprite Title { get; }

        private SpriteText Username { get; }

        private Sprite RankBadge { get; }

        private SpriteText Rank { get; }

        private Sprite GameMode { get; }

        internal Playercard()
        {
            Size = new UDim2D(310, 75);

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
                Size = new UDim2D(200, 25),
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
                Position = new UDim2D(-5, -5),
                Alpha = 0.75f
            };
        }
    }
}