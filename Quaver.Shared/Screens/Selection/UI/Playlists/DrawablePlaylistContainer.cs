using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Selection.UI.Maps;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Playlists
{
    public class DrawablePlaylistContainer : Sprite
    {
        /// <summary>
        /// </summary>
        private DrawablePlaylist Playlist { get; }

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        public SpriteTextPlus Title { get; private set; }

        /// <summary>
        /// </summary>
        private DrawableBanner Banner { get; set; }

        /// <summary>
        /// </summary>
        private PlaylistKeyValueDisplay MapCount { get; set; }

        /// <summary>
        /// </summary>
        private PlaylistKeyValueDisplay Creator { get; set; }

        /// <summary>
        /// </summary>
        private PlaylistDifficultyDisplay DifficultyDisplay { get; set; }

        /// <summary>
        ///     The ranked status of the map
        /// </summary>
        private Sprite RankedStatusSprite { get; set; }

        /// <summary>
        ///     The game modes the mapset has
        /// </summary>
        private Sprite GameModes { get; set; }

        /// <summary>
        ///     Signifies if the playlist is online
        /// </summary>
        private Sprite OnlineMapPoolIcon { get; set; }

        /// <summary>
        ///     The X position of the title/first element
        /// </summary>
        private const int TitleX = 26;

        /// <summary>
        /// </summary>
        public DrawablePlaylistContainer(DrawablePlaylist playlist)
        {
            Playlist = playlist;
            Parent = Playlist;

            Size = new ScalableVector2(Playlist.Width, 86);
            UsePreviousSpriteBatchOptions = true;

            CreateButton();
            CreateTitle();
            CreateBannerImage();
            CreateMapCount();
            CreateCreator();
            CreateDifficultyDisplay();
            CreateRankedStatus();
            CreateGameModes();
            CreateOnlineMapPoolIcon();
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
        public void UpdateContent(Playlist item, int index)
        {
            OnlineMapPoolIcon.Visible = item.IsOnlineMapPool();

            Title.Text = item.Name;
            Title.TruncateWithEllipsis(400);
            Title.X = item.IsOnlineMapPool() ? OnlineMapPoolIcon.Width + OnlineMapPoolIcon.X + 10 : TitleX;

            MapCount.ChangeValue(item.Maps.Count.ToString("n0"));

            const int metadataSpacing = 4;

            Creator.ChangeValue(item.Creator);
            Creator.X = MapCount.X + MapCount.Width + metadataSpacing;

            if (item.Maps.Count != 0)
            {
                DifficultyDisplay.ChangeValue(item.Maps.Min(x => x.DifficultyFromMods(ModManager.Mods)),
                    item.Maps.Max(x => x.DifficultyFromMods(ModManager.Mods)));
            }
            else
                DifficultyDisplay.ChangeValue(0, 0);

            DifficultyDisplay.X = Creator.X + Creator.Width + metadataSpacing;

            RankedStatusSprite.Image = GetRankedStatusImage();
            GameModes.Image = GetGameModeImage();
            Banner.UpdateContent(Playlist.Item);

            if (Playlist.IsSelected)
                Select(true);
            else
                Deselect(true);
        }

        /// <summary>
        ///     Creates <see cref="Button"/>
        /// </summary>
        private void CreateButton()
        {
            var container = (PlaylistContainer) Playlist.Container;

            Button = new SongSelectContainerButton(SkinManager.Skin?.SongSelect?.MapsetHovered ?? WobbleAssets.WhiteBox, container.ClickableArea)
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
                Alignment = Alignment.MidCenter,
                UsePreviousSpriteBatchOptions = true,
                Depth = 1
            };

            Button.Clicked += (sender, args) =>
            {
                var wasSelectedPrior = Playlist.IsSelected;

                if (!wasSelectedPrior)
                    PlaylistManager.Selected.Value = Playlist.Item;

                if (Playlist.Container == null)
                    return;

                container.SelectedIndex.Value = Playlist.Index;

                if (!wasSelectedPrior)
                    return;

                // No maps inside playlist. Prevent opening
                if (PlaylistManager.Selected.Value.Maps.Count == 0)
                {
                    NotificationManager.Show(NotificationLevel.Error, "There are no maps inside of this playlist! You can right-click maps to add to it");
                    return;
                }

                container.ActiveScrollContainer.Value = SelectScrollContainerType.Mapsets;
            };

            Button.RightClicked += (sender, args) =>
            {
                var game = (QuaverGame) GameBase.Game;
                game?.CurrentScreen?.ActivateRightClickOptions(new PlaylistRightClickOptions(Playlist));
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void PerformHoverAnimation(GameTime gameTime)
        {
            var targetAlpha = Button.IsHovered ? 0.35f : 0;

            Button.Alpha = MathHelper.Lerp(Button.Alpha, targetAlpha,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 30, 1));
        }

        /// <summary>
        /// </summary>
        public void Select(bool changeWidthInstantly = false)
        {
            Image = SkinManager.Skin?.SongSelect?.MapsetSelected ?? UserInterface.SelectedMapset;

            const int time = 200;
            AnimateSprites(1, 200);

            if (changeWidthInstantly)
                Width = Playlist.Width;
            else
                ChangeWidthTo((int) Playlist.Width, Easing.OutQuint, time + 400);
        }

        /// <summary>
        /// </summary>
        public void Deselect(bool changeWidthInstantly = false)
        {
            Image = SkinManager.Skin?.SongSelect.MapsetDeselected ?? UserInterface.DeselectedMapset;

            const int time = 200;
            AnimateSprites(0.85f, 200);

            if (changeWidthInstantly)
                Width = Playlist.Width - 50;
            else
                ChangeWidthTo((int) Playlist.Width - 50, Easing.OutQuint, time + 400);
        }

        /// <summary>
        ///     Creates <see cref="Title"/>
        /// </summary>
        private void CreateTitle()
        {
            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "PLAYLIST TITLE", 26)
            {
                Parent = this,
                Position = new ScalableVector2(TitleX, 18),
                UsePreviousSpriteBatchOptions = true,
                Tint = SkinManager.Skin?.SongSelect?.MapsetPanelSongTitleColor ?? Color.White
            };
        }

        /// <summary>
        ///    Creates <see cref="Banner"/>
        /// </summary>
        private void CreateBannerImage()
        {
            Banner = new DrawableBanner(Playlist.Item)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = SkinManager.Skin?.SongSelect?.MapsetPanelBannerSize ?? new ScalableVector2(421, 82),
                Image = UserInterface.DefaultBanner,
                X = -2,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMapCount()
        {
            MapCount = new PlaylistKeyValueDisplay("Maps:", "0", ColorHelper.HexToColor("#00FFDE"))
            {
                Parent = this,
                Position = new ScalableVector2(Title.X, Title.Y + Title.Height + 5),
                Key = { Tint = SkinManager.Skin?.SongSelect?.MapsetPanelByColor ?? ColorHelper.HexToColor("#808080") }
            };
        }

        /// <summary>
        /// </summary>
        private void CreateCreator()
        {
            Creator = new PlaylistKeyValueDisplay("By:", "Me",
                SkinManager.Skin?.SongSelect?.MapsetPanelCreatorColor ?? ColorHelper.HexToColor("#0587E5"))
            {
                Parent = this,
                Position = new ScalableVector2(Title.X, MapCount.Y),
                UsePreviousSpriteBatchOptions = true,
                Key = { Tint = SkinManager.Skin?.SongSelect?.MapsetPanelByColor ?? ColorHelper.HexToColor("#808080") }
            };
        }

        /// <summary>
        ///
        /// </summary>
        private void CreateDifficultyDisplay()
        {
            DifficultyDisplay = new PlaylistDifficultyDisplay()
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Y = MapCount.Y,
                Key = { Tint = SkinManager.Skin?.SongSelect?.MapsetPanelByColor ?? ColorHelper.HexToColor("#808080") }
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
        /// <exception cref="NotImplementedException"></exception>
        private void CreateOnlineMapPoolIcon()
        {
            OnlineMapPoolIcon = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(18, 18),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_earth_globe),
                UsePreviousSpriteBatchOptions = true,
                Visible = false,
                X = TitleX,
                Y = Title.Y + 4
            };
        }

        /// <summary>
        ///     Retrieves the color of a map's ranked status
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Texture2D GetRankedStatusImage()
        {
            if (Playlist.Item.Maps.Count == 0)
                return UserInterface.StatusNone;

            if (Playlist.Item.PlaylistGame != MapGame.Quaver)
            {
                switch (Playlist.Item.PlaylistGame)
                {
                    case MapGame.Osu:
                        return SkinManager.Skin?.SongSelect?.StatusOsu ?? UserInterface.StatusOtherGameOsu;
                    case MapGame.Etterna:
                        return SkinManager.Skin?.SongSelect?.StatusStepmania ?? UserInterface.StatusOtherGameEtterna;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (Playlist.Item.Maps.Any(o => o.RankedStatus != Playlist.Item.Maps.First().RankedStatus))
                return UserInterface.StatusVarious;

            switch (Playlist.Item.Maps.Max(x => x.RankedStatus))
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
            if (Playlist.Item.Maps.Count == 0)
                return UserInterface.KeysNonePanel;

            var has4k = false;
            var has7K = false;

            foreach (var map in Playlist.Item.Maps)
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
        /// </summary>
        /// <param name="fade"></param>
        /// <param name="time"></param>
        private void AnimateSprites(float fade, int time)
        {
            Title.ClearAnimations();
            Title.FadeTo(fade, Easing.Linear, time);

            MapCount.RemoveAnimations();
            MapCount.FadeTo(fade, Easing.Linear, time);

            Creator.RemoveAnimations();
            Creator.FadeTo(fade, Easing.Linear, time);

            DifficultyDisplay.RemoveAnimations();
            DifficultyDisplay.FadeTo(fade, Easing.Linear, time);

            RankedStatusSprite.ClearAnimations();
            RankedStatusSprite.FadeTo(fade, Easing.Linear, time);

            GameModes.ClearAnimations();
            GameModes.FadeTo(fade, Easing.Linear, time);

            ClearAnimations();
        }
    }
}