using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Main;

namespace Quaver.Graphics.UserInterface.Online
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
        ///     Sprite that changes the brightness for the title.
        /// </summary>
        private Sprite TitleBrightness { get; }

        /// <summary>
        ///     The username of the player.
        /// </summary>
        private SpriteText Username { get; }

        /// <summary>
        ///     The player's rating
        /// </summary>
        private SpriteText Rating { get; }

        /// <summary>
        ///     The player's rank.
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
            Size = new UDim2D(312, 56);

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
                Image = Titles.Default,
                Size = new UDim2D(SizeX - Avatar.SizeX, SizeY),
                Alignment = Alignment.TopLeft,
                PosX = Avatar.SizeX,
            };

            TitleBrightness = new Sprite()
            {
                Parent = this,
                Image = GameBase.QuaverUserInterface.BlankBox,
                Tint = Color.Black,
                Alpha = 0.50f,
                Size = Title.Size,
                Alignment = Title.Alignment,
                Position = Title.Position
            };

            Username = new SpriteText()
            {
                Parent = this,
                Font = Fonts.AllerRegular16,
                PosX = Avatar.PosX + Avatar.SizeX + 7,
                PosY = 5,
                Text = ConfigManager.Username.Value,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.TopLeft,
                TextColor = Color.White,
                TextScale = 0.90f
            };
            
            Rating = new SpriteText()
            {
                Parent = this,
                Font = Fonts.AllerRegular16,
                PosX = Username.PosX,
                PosY = Username.PosY + 28,
                Text = "Rating: 0.00",
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.TopLeft,
                TextColor = Color.White,
                TextScale = 0.70f
            };

            GameMode = new Sprite()
            {
                Parent = this,
                Image = FontAwesome.Desktop,
                Size = new UDim2D(20, 20),
                Alignment = Alignment.TopRight,
                Position = new UDim2D(-7, Username.PosY),
                Alpha = 1f
            };

            Rank = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                TextScale = 0.70f,
                PosY = Rating.PosY,
                Text = "#0 (Lv.0)",
                Font = Fonts.AllerRegular16,
                TextAlignment = Alignment.TopRight,
                PosX = -7
            };
        }
    }
}