using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Selection.UI.Maps;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public class DrawableMapsetContainer : Sprite
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
        private DrawableBanner Banner { get; set; }

        /// <summary>
        ///     The title of the map
        /// </summary>
        public SpriteTextPlus Title { get; private set; }

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
        ///     The X position of the title/first element
        /// </summary>
        private const int TitleX = 26;

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
        ///     When the mapsets are sorted by difficulty/grade achieved
        ///     this will display the difficulty rating & name to make it act as an individual map
        /// </summary>
        public SpriteTextPlus DifficultyName { get; private set; }

        /// <summary>
        ///     The highest online grade that the user has achieved
        /// </summary>
        public Sprite OnlineGrade { get; set; }

        private bool _isCached = true;
        public bool IsCached
        {
            get => _isCached;
            set
            {
                if (value == _isCached)
                    return;

                SetCaching(value);
                _isCached = value;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        public DrawableMapsetContainer(DrawableMapset mapset)
        {
            ParentMapset = mapset;
            Parent = ParentMapset;

            Size = new ScalableVector2(1188, 86);

            CreateButton();
            CreateTitle();
            CreateArtist();
            CreateDifficultyName();
            CreateDividerLine();
            CreateCreator();
            CreateBannerImage();
            CreateRankedStatus();
            CreateGameModes();
            CreateOnlineGrade();

            UsePreviousSpriteBatchOptions = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Button.Width = Width;

            PerformHoverAnimation(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public void UpdateContent(Mapset item, int index)
        {
            Creator.Text = $"{item.Creator}";

            if (MapsetHelper.IsSingleDifficultySorted())
            {
                Title.FontSize = 22;
                Title.Text = $"{item.Artist} - {item.Title}";

                var map = ParentMapset.Item.Maps.First();

                var diff = map.DifficultyFromMods(ModManager.Mods);

                DifficultyName.Text = $"{StringHelper.RatingToString(diff)} - {map.DifficultyName}";
                DifficultyName.Tint = ColorHelper.DifficultyToColor((float) diff);
                DifficultyName.Visible = true;

                DifficultyName.TruncateWithEllipsis(260);

                if (map.OnlineGrade != Grade.None)
                {
                    const int width = 40;

                    OnlineGrade.Visible = true;
                    OnlineGrade.Image = SkinManager.Skin.Grades[map.OnlineGrade];
                    OnlineGrade.Size = new ScalableVector2(width, OnlineGrade.Image.Height / OnlineGrade.Image.Width * width);

                    Title.X = OnlineGrade.X + OnlineGrade.Width + 16;
                    Title.TruncateWithEllipsis(400 - (int)OnlineGrade.Width - 16);
                    Artist.X = Title.X;
                    DifficultyName.X = Artist.X;
                }
                else
                {
                    Title.X = TitleX;
                    Title.TruncateWithEllipsis(400);
                    Artist.X = TitleX;
                    DifficultyName.X = TitleX;
                    OnlineGrade.Visible = false;
                }

                DividerLine.X = DifficultyName.X + DifficultyName.Width + ArtistCreatorSpacingX;
                Artist.Visible = false;
            }
            else
            {
                Title.FontSize = 26;
                Title.Text = item.Title;
                Title.TruncateWithEllipsis(400);

                Artist.Text = $"{item.Artist}";
                Artist.TruncateWithEllipsis(400);

                // Title.X = TitleX;
                Artist.X = TitleX;
                DividerLine.X = Artist.X + Artist.Width + ArtistCreatorSpacingX;
                Artist.Visible = true;
                OnlineGrade.Visible = false;
                DifficultyName.Visible = false;
            }

            ByText.X = DividerLine.X + DividerLine.Width + ArtistCreatorSpacingX;
            Creator.X = ByText.X + ByText.Width + ArtistCreatorSpacingX;

            RankedStatusSprite.Image = GetRankedStatusImage();
            GameModes.Image = GetGameModeImage();

            if (ParentMapset.IsSelected)
                Select(true);
            else
                Deselect(true);

            Banner.UpdateContent(ParentMapset);
        }

        /// <summary>
        ///     Creates <see cref="Button"/>
        /// </summary>
        private void CreateButton()
        {
            var container = (SongSelectContainer<Mapset>) ParentMapset.Container;

            Button = new SongSelectContainerButton(SkinManager.Skin?.SongSelect?.MapsetHovered ?? WobbleAssets.WhiteBox, container.ClickableArea)
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
                Alignment = Alignment.MidCenter,
                UsePreviousSpriteBatchOptions = true,
                Depth = 1
            };

            Button.Clicked += (sender, args) => OnMapsetClicked();

            Button.RightClicked += (sender, args) =>
            {
                var game = (QuaverGame) GameBase.Game;

                if (MapsetHelper.IsSingleDifficultySorted())
                    game?.CurrentScreen?.ActivateRightClickOptions(new MapRightClickOptions(ParentMapset));
                else
                    game?.CurrentScreen?.ActivateRightClickOptions(new MapsetRightClickOptions(ParentMapset));
            };
        }

        /// <summary>
        ///    Creates <see cref="Banner"/>
        /// </summary>
        private void CreateBannerImage()
        {
            Banner = new DrawableBanner(ParentMapset)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = SkinManager.Skin?.SongSelect?.MapsetPanelBannerSize ?? new ScalableVector2(421, 82),
                X = -2,
                UsePreviousSpriteBatchOptions = true
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
                Position = new ScalableVector2(TitleX, 18),
                UsePreviousSpriteBatchOptions = true,
                Tint = SkinManager.Skin?.SongSelect?.MapsetPanelSongTitleColor ?? Color.White
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
                Tint = SkinManager.Skin?.SongSelect?.MapsetPanelSongArtistColor ?? ColorHelper.HexToColor("#0587e5"),
                UsePreviousSpriteBatchOptions = true
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
                Tint = ColorHelper.HexToColor("#808080"),
                UsePreviousSpriteBatchOptions = true
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
                Tint = SkinManager.Skin?.SongSelect?.MapsetPanelByColor ?? ColorHelper.HexToColor("#757575"),
                UsePreviousSpriteBatchOptions = true
            };

            Creator = new SpriteTextPlus(Title.Font, "Creator", Artist.FontSize)
            {
                Parent = this,
                Position = new ScalableVector2(ByText.X + ByText.Width + ArtistCreatorSpacingX, Artist.Y),
                Tint = SkinManager.Skin?.SongSelect?.MapsetPanelCreatorColor ?? Artist.Tint,
                UsePreviousSpriteBatchOptions = true,
            };
        }

        /// <summary>
        ///     Creates <see cref="RankedStatusSprite"/>
        /// </summary>
        private void CreateRankedStatus()
        {
            RankedStatusSprite = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(115, 28),
                X = Banner.X - Banner.Width - 18,
                Image = UserInterface.StatusPanel,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateGameModes()
        {
            GameModes = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(71, 28),
                X = RankedStatusSprite.X - RankedStatusSprite.Width - 18,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateOnlineGrade()
        {
            OnlineGrade = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Visible = false,
                Alpha = 0,
                X = TitleX,
                UsePreviousSpriteBatchOptions = true,
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDifficultyName()
        {
            DifficultyName = new SpriteTextPlus(Title.Font, "Difficulty", 20)
            {
                Parent = this,
                Position = new ScalableVector2(Title.X, Artist.Y),
                UsePreviousSpriteBatchOptions = true,
                Alpha = 0
            };
        }

        /// <summary>
        ///     Retrieves the color of a map's ranked status
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Texture2D GetRankedStatusImage()
        {
            if (ParentMapset.Item.Maps.First().Game != MapGame.Quaver)
            {
                switch (ParentMapset.Item.Maps.First().Game)
                {
                    case MapGame.Osu:
                        return SkinManager.Skin?.SongSelect?.StatusOsu ?? UserInterface.StatusOtherGameOsu;
                    case MapGame.Etterna:
                        return SkinManager.Skin?.SongSelect?.StatusStepmania ?? UserInterface.StatusOtherGameEtterna;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            switch (ParentMapset.Item.Maps.Max(x => x.RankedStatus))
            {
                case RankedStatus.NotSubmitted:
                    return SkinManager.Skin?.SongSelect?.StatusNotSubmitted ??  UserInterface.StatusNotSubmitted;
                case RankedStatus.Unranked:
                    return SkinManager.Skin?.SongSelect?.StatusUnranked ?? UserInterface.StatusUnranked;
                case RankedStatus.Ranked:
                    return SkinManager.Skin?.SongSelect?.StatusRanked ?? UserInterface.StatusRanked;
                case RankedStatus.DanCourse:
                    return SkinManager.Skin?.SongSelect?.StatusNotSubmitted ?? UserInterface.StatusNotSubmitted;
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
                return SkinManager.Skin?.SongSelect?.GameMode4K ?? UserInterface.Keys4Panel;
            if (has7K && !has4k)
                return SkinManager.Skin?.SongSelect?.GameMode7K ?? UserInterface.Keys7Panel;

            return SkinManager.Skin?.SongSelect?.GameMode4K7K ?? UserInterface.BothModesPanel;
        }

        /// <summary>
        ///     Performs an animation when hovered over the button
        /// </summary>
        /// <param name="gameTime"></param>
        private void PerformHoverAnimation(GameTime gameTime)
        {
            var targetAlpha = Button.IsHovered ? (SkinManager.Skin?.SongSelect?.MapsetPanelHoveringAlpha ?? 0.35f) : 0;

            Button.Alpha = MathHelper.Lerp(Button.Alpha, targetAlpha,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 30, 1));
        }

        /// <summary>
        /// </summary>
        public void Select(bool instantSizeChange = false)
        {
            Image = SkinManager.Skin?.SongSelect?.MapsetSelected ?? UserInterface.SelectedMapset;

            var fade = 1f;
            var time = 200;

            Title.ClearAnimations();
            Title.FadeTo(fade, Easing.Linear, time);

            Artist.ClearAnimations();
            Artist.FadeTo(fade, Easing.Linear, time);

            DividerLine.ClearAnimations();
            DividerLine.FadeTo(fade, Easing.Linear, time);

            Creator.ClearAnimations();
            Creator.FadeTo(fade, Easing.Linear, time);

            RankedStatusSprite.ClearAnimations();
            RankedStatusSprite.FadeTo(fade, Easing.Linear, time);

            GameModes.ClearAnimations();
            GameModes.FadeTo(fade, Easing.Linear, time);

            if (MapsetHelper.IsSingleDifficultySorted())
            {
                DifficultyName.ClearAnimations();
                DifficultyName.FadeTo(fade, Easing.Linear, time);

                OnlineGrade.ClearAnimations();
                OnlineGrade.FadeTo(fade, Easing.Linear, time);
            }

            if (Banner.HasBannerLoaded)
            {
                Banner.ClearAnimations();
                Banner.FadeTo(1, Easing.Linear, time);
            }

            ClearAnimations();

            if (instantSizeChange)
                Width = ParentMapset.Width;
            else
                ChangeWidthTo((int) ParentMapset.Width, Easing.OutQuint, time + 400);
        }

        /// <summary>
        /// </summary>
        public void Deselect(bool changeWidthInstantly = false)
        {
            Image = SkinManager.Skin?.SongSelect.MapsetDeselected ?? UserInterface.DeselectedMapset;

            var fade = 0.85f;
            var time = 200;

            Title.ClearAnimations();
            Title.FadeTo(fade, Easing.Linear, time);

            Artist.ClearAnimations();
            Artist.FadeTo(fade, Easing.Linear, time);

            DividerLine.ClearAnimations();
            DividerLine.FadeTo(fade, Easing.Linear, time);

            Creator.ClearAnimations();
            Creator.FadeTo(fade, Easing.Linear, time);

            RankedStatusSprite.ClearAnimations();
            RankedStatusSprite.FadeTo(fade, Easing.Linear, time);

            GameModes.ClearAnimations();
            GameModes.FadeTo(fade, Easing.Linear, time);

            if (MapsetHelper.IsSingleDifficultySorted())
            {
                DifficultyName.ClearAnimations();
                DifficultyName.FadeTo(fade, Easing.Linear, time);

                OnlineGrade.ClearAnimations();
                OnlineGrade.FadeTo(fade, Easing.Linear, time);
            }

            if (Banner.HasBannerLoaded)
            {
                Banner.ClearAnimations();
                Banner.FadeTo(DrawableBanner.DeselectedAlpha, Easing.Linear, time);
            }

            ClearAnimations();

            if (changeWidthInstantly)
                Width = ParentMapset.Width - 50;
            else
                ChangeWidthTo((int) ParentMapset.Width - 50, Easing.OutQuint, time + 400);
        }

        /// <summary>
        ///     Called when the mapset has been clicked
        /// </summary>
        private void OnMapsetClicked()
        {
            if (ParentMapset.Container != null)
            {
                var container = (MapsetScrollContainer) ParentMapset.Container;
                container.SelectedIndex.Value = ParentMapset.Index;

                // If a mapset is clicked, then we want to take the user to the maps container
                if (ParentMapset.IsSelected)
                {
                    // Go straight to gameplay if sorting by diff
                    if (MapsetHelper.IsSingleDifficultySorted())
                    {
                        var game = (QuaverGame) GameBase.Game;
                        var screen = game.CurrentScreen as SelectionScreen;
                        screen?.ExitToGameplay();
                    }
                    else
                    {
                        container.ActiveScrollContainer.Value = SelectScrollContainerType.Maps;
                    }

                    return;
                }
            }

            // Mapset is already selected, so go play the current map.
            if (ParentMapset.IsSelected)
            {
                Logger.Important($"User clicked on mapset to play: {MapManager.Selected.Value}", LogType.Runtime, false);
                return;
            }

            Logger.Important($"User opened mapset: {ParentMapset.Item.Artist} - {ParentMapset.Item.Title}", LogType.Runtime, false);
            MapManager.SelectMapFromMapset(ParentMapset.Item);
        }

        /// <summary>
        ///     Enables/disables caching of frequently changed strings
        /// </summary>
        private void SetCaching(bool cache)
        {
            Title.IsCached = cache;
            Artist.IsCached = cache;
            Creator.IsCached = cache;
            DifficultyName.IsCached = cache;
        }
    }
}
