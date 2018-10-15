using System.Drawing.Drawing2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Assets;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;

namespace Quaver.Graphics.Online
{
    public class UserPlayercard : Sprite
    {
        /// <summary>
        ///     The user's currently selected title
        /// </summary>
        private Sprite Title { get; set; }

        /// <summary>
        ///     The user's avatar
        /// </summary>
        private Sprite Avatar { get; set; }

        /// <summary>
        ///     The user's username.
        /// </summary>
        private SpriteTextBitmap Username { get; set; }

        /// <summary>
        ///     Contains the user's current status.
        /// </summary>
        private Sprite StatusContainer { get; set; }

        /// <summary>
        ///     The user's status text.
        /// </summary>
        private SpriteTextBitmap Status { get; set; }

        /// <summary>
        ///     A badge that symbolizes the user's competitive rank.
        /// </summary>
        private Sprite CompetitiveRankBadge { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public UserPlayercard()
        {
            Tint = Colors.DarkGray;
            Size = new ScalableVector2(384, 120);

            CreateTitle();
            CreateAvatar();
            CreateUsername();
            // CreateStatusContainer();
            // CreateStatus();
            CreateCompetitiveRankBadge();

            AddBorder(Color.White, 2);
        }

        /// <summary>
        ///     Creates the sprite with the user's currently selected title.
        /// </summary>
        private void CreateTitle() => Title = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(256, 30),
            X = 10,
            Y = 10,
            Image = UserInterface.BlankBox,
            Tint = Color.White,
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        ///     Creates the sprite for the user's avatar.
        /// </summary>
        private void CreateAvatar()
        {
            Avatar = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(26, 26),
                Y = Title.Y + Title.Height + 5,
                X = Title.X,
                Image = UserInterface.UnknownAvatar,
                UsePreviousSpriteBatchOptions = true
            };

            Avatar.AddBorder(Color.LightGray, 2);
        }

        /// <summary>
        ///     Creates the sprite that shows the user's username.
        /// </summary>
        private void CreateUsername()
        {
            Username = new SpriteTextBitmap(BitmapFonts.Exo2BoldItalic, "Player", 24, Color.White, Alignment.MidCenter, int.MaxValue)
            {
                Parent = Avatar,
                X = Avatar.Width + 5,
                Y = 1,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };

            Username.Size = new ScalableVector2(Username.Width * 0.60f, Username.Height * 0.60f);
        }

        /// <summary>
        ///     Creates the container where the user's current status will go.
        /// </summary>
        private void CreateStatusContainer() => StatusContainer = new Sprite()
        {
            Parent = this,
            Size = new ScalableVector2(Width - 20, 25),
            Tint = Color.Black,
            X = Title.X,
            UsePreviousSpriteBatchOptions = true,
            Y = Avatar.Y + Avatar.Height + 5,
            Alpha = 0.35f,
        };

        /// <summary>
        ///     Creates the status text
        /// </summary>
        private void CreateStatus()
        {
            Status = new SpriteTextBitmap(BitmapFonts.Exo2SemiBold, "Playing Artist - Title", 24,
                Color.White, Alignment.MidCenter, int.MaxValue)
            {
                Parent = StatusContainer,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.MidLeft,
            };

            Status.Size = new ScalableVector2(Status.Width * 0.50f, Status.Height * 0.50f);
        }

        /// <summary>
        ///     Creates the sprite to show the user's rank badge.
        /// </summary>
        private void CreateCompetitiveRankBadge()
        {
            CompetitiveRankBadge = new Sprite()
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                X = Title.X + Title.Width + 28,
                Y = Title.Y,
                Size = new ScalableVector2(60, 60),
                Image = FontAwesome.ArrowCircle
            };
        }
    }
}