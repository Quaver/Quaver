using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using TagLib.Ape;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public class DrawableMapsetContainer : Sprite, IDrawableMapsetComponent
    {
        /// <summary>
        ///     The parent mapset
        /// </summary>
        public DrawableMapset ParentMapset { get; }

        /// <summary>
        ///     The button/clickable area of the mapset
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        ///     Displays the map background/banner for the mapset
        /// </summary>
        private Sprite Banner { get; set; }

        /// <summary>
        ///     The title of the map
        /// </summary>
        private SpriteTextPlus Title { get; set; }

        /// <summary>
        ///     Displays the artist of the song
        /// </summary>
        private SpriteTextPlus Artist { get; set; }

        /// <summary>
        ///     The divider line between <see cref="Artist"/> and <see cref="Creator"/>
        /// </summary>
        private SpriteTextPlus DividerLine { get; set; }

        /// <summary>
        ///    Displays the creator of the map
        /// </summary>
        private SpriteTextPlus Creator { get; set; }

        /// <summary>
        ///     The amount of x axis spacing between the artist and creator
        /// </summary>
        private const int ArtistCreatorSpacingX = 4;

        /// <summary>
        ///     The ranked status of the map
        /// </summary>
        private Sprite RankedStatusSprite { get; set; }

        /// <summary>
        ///     The game modes the mapset has
        /// </summary>
        private Sprite GameModes { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ByText { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        public DrawableMapsetContainer(DrawableMapset mapset)
        {
            ParentMapset = mapset;
            Parent = ParentMapset;

            Size = new ScalableVector2(1188, ParentMapset.HEIGHT);
            Tint = ColorHelper.HexToColor("#242424");
            AddBorder(ColorHelper.HexToColor("#0587e5"), 2);

            CreateButton();
            CreateTitle();
            CreateArtist();
            CreateDividerLine();
            CreateCreator();
            CreateBannerImage();
            CreateRankedStatus();
            CreateGameModes();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformHoverAnimation(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public void UpdateContent(Mapset item, int index)
        {
            Title.Text = item.Title.ToUpper();
            Artist.Text = $"{item.Artist}";
            Creator.Text = $"{item.Creator}";

            DividerLine.X = Artist.X + Artist.Width + ArtistCreatorSpacingX;
            ByText.X = DividerLine.X + DividerLine.Width + ArtistCreatorSpacingX;
            Creator.X = ByText.X + ByText.Width + ArtistCreatorSpacingX;

            RankedStatusSprite.Image = GetRankedStatusImage();
            GameModes.Image = GetGameModeImage();
        }

        /// <summary>
        ///     Creates <see cref="Button"/>
        /// </summary>
        private void CreateButton()
        {
            Button = new ImageButton(WobbleAssets.WhiteBox)
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
                Alignment = Alignment.MidCenter
            };
        }

        /// <summary>
        ///    Creates <see cref="Banner"/>
        /// </summary>
        private void CreateBannerImage()
        {
            Banner = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(421, 82),
                X = -Border.Thickness,
                Image = UserInterface.MenuBackgroundNormal
            };
        }

        /// <summary>
        ///     Creates <see cref="Title"/>
        /// </summary>
        private void CreateTitle()
        {
            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "SONG TITLE", 26)
            {
                Parent = this,
                Position = new ScalableVector2(26, 18)
            };
        }

        /// <summary>
        ///     Creates <see cref="Artist"/>
        /// </summary>
        private void CreateArtist()
        {
            Artist = new SpriteTextPlus(Title.Font, "Artist", 20)
            {
                Parent = this,
                Position = new ScalableVector2(Title.X, Title.Y + Title.Height + 5),
                Tint = ColorHelper.HexToColor("#0587e5")
            };
        }

        /// <summary>
        ///     Creates <see cref="DividerLine"/>
        /// </summary>
        private void CreateDividerLine()
        {
            DividerLine = new SpriteTextPlus(Artist.Font, "|", Artist.FontSize)
            {
                Parent = this,
                Position = new ScalableVector2(Artist.X + Artist.Width + ArtistCreatorSpacingX, Artist.Y),
                Tint = ColorHelper.HexToColor("#808080")
            };
        }

        /// <summary>
        ///     Creates <see cref="Creator"/>
        /// </summary>
        private void CreateCreator()
        {
            ByText = new SpriteTextPlus(Title.Font, "By:", Artist.FontSize)
            {
                Parent = this,
                Position = new ScalableVector2(DividerLine.X + DividerLine.Width + ArtistCreatorSpacingX, Artist.Y),
                Tint = ColorHelper.HexToColor("#757575")
            };

            Creator = new SpriteTextPlus(Title.Font, "Creator", Artist.FontSize)
            {
                Parent = this,
                Position = new ScalableVector2(ByText.X + ByText.Width + ArtistCreatorSpacingX, Artist.Y),
                Tint = Artist.Tint
            };
        }

        /// <summary>
        ///     Creates <see cref="RankedStatusSprite"/>
        /// </summary>
        private void CreateRankedStatus()
        {
            RankedStatusSprite = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(115, 28),
                X = Banner.X - Banner.Width - 18,
                Image = UserInterface.StatusPanel
            };
        }

        /// <summary>
        ///
        /// </summary>
        private void CreateGameModes()
        {
            GameModes = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(71, 28),
                X = RankedStatusSprite.X - RankedStatusSprite.Width - 18
            };
        }

        /// <summary>
        ///     Retrieves the color of a map's ranked status
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Texture2D GetRankedStatusImage()
        {
            switch (ParentMapset.Item.Maps.Max(x => x.RankedStatus))
            {
                case RankedStatus.NotSubmitted:
                    return UserInterface.StatusNotSubmitted;
                case RankedStatus.Unranked:
                    return UserInterface.StatusUnranked;
                case RankedStatus.Ranked:
                    return UserInterface.StatusRanked;
                case RankedStatus.DanCourse:
                    return UserInterface.StatusNotSubmitted;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Gets the image for the game mode(s)
        /// </summary>
        /// <returns></returns>
        private Texture2D GetGameModeImage()
        {
            var has4k = false;
            var has7K = false;

            foreach (var map in ParentMapset.Item.Maps)
            {
                switch (map.Mode)
                {
                    case GameMode.Keys4:
                        has4k = true;
                        break;
                    case GameMode.Keys7:
                        has7K = true;
                        break;
                }
            }

            if (has4k && !has7K)
                return UserInterface.Keys4Panel;
            if (has7K && !has4k)
                return UserInterface.Keys7Panel;

            return UserInterface.BothModesPanel;
        }

        /// <summary>
        ///     Performs an animation when hovered over the button
        /// </summary>
        /// <param name="gameTime"></param>
        private void PerformHoverAnimation(GameTime gameTime)
        {
            var targetAlpha = Button.IsHovered ? 0.35f : 0;

            Button.Alpha = MathHelper.Lerp(Button.Alpha, targetAlpha,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 30, 1));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Select()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Deselect()
        {
        }
    }
}