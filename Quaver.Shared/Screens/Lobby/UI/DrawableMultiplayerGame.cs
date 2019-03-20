using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Lobby.UI
{
    public class DrawableMultiplayerGame : PoolableSprite<MultiplayerGame>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override int HEIGHT { get; } = 92;

        /// <summary>
        /// </summary>
        private DrawableMultiplayerGameButton Background { get; }

        /// <summary>
        /// </summary>
        private Sprite MapBanner { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap GameTitle { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap GameType { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap MapTitle { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap PlayerCount { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap DifficultyRating { get; set; }

        /// <summary>
        /// </summary>
        private Sprite PasswordLock { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableMultiplayerGame(PoolableScrollContainer<MultiplayerGame> container, MultiplayerGame item, int index) : base(container, item, index)
        {
            Alpha = 0f;
            Size = new ScalableVector2(container.Width, HEIGHT);

            Background = new DrawableMultiplayerGameButton(container as LobbyMatchScrollContainer, this) { Parent = this };
            CreateMapBanner();
            CreateGameTitle();
            CreateGameType();
            CreateDifficultyRating();
            CreateMapTitle();
            CreatePlayerCount();
            CreatePasswordLock();

            X = -Width;
            MoveToX(0, Easing.OutQuint, 600);
        }

        /// <summary>
        /// </summary>
        private void CreateDifficultyRating() => DifficultyRating= new SpriteTextBitmap(FontsBitmap.GothamRegular, "0.00")
        {
            Parent = this,
            X = MapBanner.X + MapBanner.Width + 12,
            Y = GameType.Y + GameType.Height + 5,
            FontSize = 13,
            UsePreviousSpriteBatchOptions = true,
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(MultiplayerGame item, int index)
        {
            Item = item;
            Index = index;
            Container.AvailableItems[index] = item;

            GameTitle.Text = item.Name;

            DifficultyRating.Text = $"{item.DifficultyRating:0.00}";
            DifficultyRating.Tint = ColorHelper.DifficultyToColor((float) item.DifficultyRating);

            MapTitle.Text = " - " + item.Map;
            MapTitle.X = DifficultyRating.X + DifficultyRating.Width + 2;

            PlayerCount.Text = $"{item.Players.Count}/{item.MaxPlayers} Players";
            PasswordLock.Alpha = Item.HasPassword ? 0.75f : 0.30f;

            MapBanner.Alpha = 0;
            FetchMapsetBanner();

            switch (item.Type)
            {
                case MultiplayerGameType.Friendly:
                    GameType.Text = $"[{ModeHelper.ToShortHand((GameMode) item.GameMode)}] Friendly - {item.Ruleset.ToString().Replace("_", "-")}";
                    break;
                case MultiplayerGameType.Competitive:
                    GameType.Text = $"[{ModeHelper.ToShortHand((GameMode) item.GameMode)}] Competitive - {item.Ruleset.ToString().Replace("_", "-")}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Background.Destroy();
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateMapBanner() => MapBanner = new Sprite
        {
            Parent = Background,
            Image = UserInterface.MenuBackground,
            Size = new ScalableVector2(280, Background.Height),
            Alpha = 0,
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        /// </summary>
        private void CreateGameTitle() => GameTitle = new SpriteTextBitmap(FontsBitmap.GothamRegular, Item.Name)
        {
            Parent = this,
            X = MapBanner.X + MapBanner.Width + 12,
            Y = 10,
            FontSize = 16,
            UsePreviousSpriteBatchOptions = true,
        };

        /// <summary>
        /// </summary>
        private void CreateGameType() => GameType = new SpriteTextBitmap(FontsBitmap.GothamRegular, $"Custom Game")
        {
            Parent = this,
            X = MapBanner.X + MapBanner.Width + 12,
            Y = GameTitle.Y + GameTitle.Height + 5,
            FontSize = 14,
            UsePreviousSpriteBatchOptions = true,
            Tint = Colors.MainAccent
        };

        /// <summary>
        /// </summary>
        private void CreateMapTitle() => MapTitle = new SpriteTextBitmap(FontsBitmap.GothamRegular, Item.Map)
        {
            Parent = this,
            X = DifficultyRating.X + DifficultyRating.Width + 4,
            Y = DifficultyRating.Y,
            FontSize = 13,
            UsePreviousSpriteBatchOptions = true,
        };

        /// <summary>
        /// </summary>
        private void CreatePlayerCount() => PlayerCount = new SpriteTextBitmap(FontsBitmap.GothamRegular, $"{Item.Players.Count}/{Item.MaxPlayers} Players")
        {
            Parent = this,
            Alignment = Alignment.BotRight,
            X = -12,
            Y = -12,
            FontSize = 13,
            UsePreviousSpriteBatchOptions = true,
            Tint = Color.White
        };

        /// <summary>
        /// </summary>
        private void CreatePasswordLock() => PasswordLock = new Sprite
        {
            Parent = this,
            Alignment = Alignment.TopRight,
            Position = new ScalableVector2(-12, 12),
            Size = new ScalableVector2(20, 20),
            Alpha = Item.HasPassword ? 0.75f : 0.30f,
            Tint = Colors.SecondaryAccent,
            Image = FontAwesome.Get(FontAwesomeIcon.fa_padlock)
        };

        /// <summary>
        /// </summary>
        private void FetchMapsetBanner() => Task.Run(async () =>
        {
            MapBanner.Image = await ImageDownloader.DownloadMapsetBanner(Item.MapsetId);
            MapBanner.FadeTo(1, Easing.OutQuint, 4000);
        });
    }
}