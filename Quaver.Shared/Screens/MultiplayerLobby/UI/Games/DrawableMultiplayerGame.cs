using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;
using GameMode = Quaver.API.Enums.GameMode;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Games
{
    public sealed class DrawableMultiplayerGame : PoolableSprite<MultiplayerGame>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = DrawableMapset.MapsetHeight;

        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> SelectedGame { get; }

        /// <summary>
        /// </summary>
        private bool IsSelected => SelectedGame.Value?.Id == Item?.Id;

        /// <summary>
        /// </summary>
        private Sprite Panel { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton ClickableButton { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Banner { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus DifficultyRating { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus DividerLine { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Map { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Lock { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus PlayerCount { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Ruleset { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Mode { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="selectedGame"></param>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableMultiplayerGame(Bindable<MultiplayerGame> selectedGame, PoolableScrollContainer<MultiplayerGame> container,
            MultiplayerGame item, int index) : base(container, item, index)
        {
            Alpha = 0;
            Size = new ScalableVector2(DrawableMapset.WIDTH, HEIGHT);

            SelectedGame = selectedGame;

            CreatePanel();
            CreateButton();
            CreateBanner();
            CreateName();
            CreateDifficultyRating();
            CreateDividerLine();
            CreateMapName();
            CreatePlayerCount();
            CreateLock();
            CreateRuleset();
            CreateMode();

            FadeText(Easing.Linear, 0.85f, 1);
            FadeSprites(Easing.Linear, 0.85f, 1);

            UpdateContent(item, index);
            SelectedGame.ValueChanged += OnSelectedGameChanged;
            BackgroundHelper.BannerLoaded += OnBannerLoaded;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ClickableButton.Alpha = ClickableButton.IsHovered && Button.IsGloballyClickable ? 0.35f : 0;
            ClickableButton.Size = Panel.Size;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SelectedGame.ValueChanged -= OnSelectedGameChanged;
            BackgroundHelper.BannerLoaded -= OnBannerLoaded;
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(MultiplayerGame item, int index)
        {
            Item = item;
            Index = index;

            AddScheduledUpdate(() =>
            {
                Name.Text = Item.Name ?? "";
                Name.Tint = Color.White;

                if (Item.InProgress)
                {
                    Name.Text += $" (In Progress)";
                    Name.Tint = ColorHelper.HexToColor("#808080");
                }

                Name.TruncateWithEllipsis(400);

                DifficultyRating.Text = $"{Item.DifficultyRating:0.00}";
                DifficultyRating.Tint = ColorHelper.DifficultyToColor((float) Item.DifficultyRating);

                DividerLine.X = DifficultyRating.X + DifficultyRating.Width + 6;

                Map.X = DividerLine.X + DividerLine.Width + 6;
                Map.Text = Item.Map;
                Map.Tint = DifficultyRating.Tint;
                Map.TruncateWithEllipsis(400);

                if (Item.HasPassword)
                {
                    Lock.Tint = Colors.SecondaryAccent;
                    Lock.Alpha = 1;
                }
                else
                {
                    Lock.Tint = Color.White;
                    Lock.Alpha = 0.45f;
                }

                PlayerCount.Text = $"{Item.PlayerIds.Count}/{Item.MaxPlayers}";
                PlayerCount.X = Lock.X - Lock.Width - 12;

                Mode.Image = GetModeIcon(Item);
                Ruleset.Image = GetRulesetIcon(Item);

                LoadBanner();

                if (IsSelected)
                    Select();
                else
                    Deselect();
            });
        }

        /// <summary>
        /// </summary>
        public void SlideIn(int time = 450)
        {
            X = -Width;
            ClearAnimations();
            MoveToX(0, Easing.OutQuint, time);
        }

        /// <summary>
        /// </summary>
        private void CreatePanel()
        {
            Panel = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(Width - 50, 86),
                Image = UserInterface.DeselectedMapset,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            ClickableButton = new ImageButton(UserInterface.BlankBox)
            {
                Parent = Panel,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(Panel.Width - 4, Panel.Height - 4),
                Depth = 1,
                UsePreviousSpriteBatchOptions = true
            };

            ClickableButton.Clicked += OnClicked;

            ClickableButton.RightClicked += (o, e) =>
            {
                var game = (QuaverGame) GameBase.Game;
                game.CurrentScreen.ActivateRightClickOptions(new MultiplayerGameRightClickOptions(Item));
            };
        }

        /// <summary>
        /// </summary>
        private void CreateBanner()
        {
            Banner = new Sprite
            {
                Parent = Panel,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(406, 82),
                Image = UserInterface.DefaultBanner,
                Alpha = 0f,
                X = 2,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateName()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 24)
            {
                Parent = Panel,
                X = Banner.X + Banner.Width + 18,
                Y = 16,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDifficultyRating()
        {
            DifficultyRating = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = Panel,
                Alignment = Alignment.BotLeft,
                X = Name.X,
                Y = -Name.Y,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Creates <see cref="DividerLine"/>
        /// </summary>
        private void CreateDividerLine()
        {
            DividerLine = new SpriteTextPlus(DifficultyRating.Font, "-", DifficultyRating.FontSize)
            {
                Parent = Panel,
                Alignment = DifficultyRating.Alignment,
                Position = new ScalableVector2(0, DifficultyRating.Y),
                Tint = ColorHelper.HexToColor("#808080"),
                UsePreviousSpriteBatchOptions = true,
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMapName()
        {
            Map = new SpriteTextPlus(DifficultyRating.Font, "", DifficultyRating.FontSize)
            {
                Parent = Panel,
                Alignment = DifficultyRating.Alignment,
                Y = DifficultyRating.Y,
                Tint = ColorHelper.HexToColor("#0587e5"),
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateLock()
        {
            Lock = new Sprite
            {
                Parent = Panel,
                Alignment = Alignment.BotRight,
                Position = new ScalableVector2(-18, DifficultyRating.Y),
                Size = new ScalableVector2(22, 22),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_padlock),
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePlayerCount()
        {
            PlayerCount = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = Panel,
                Alignment = Alignment.BotRight,
                Y = DifficultyRating.Y,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateRuleset() => Ruleset = new Sprite
        {
            Parent = Panel,
            Alignment = Alignment.TopRight,
            Position = new ScalableVector2(Lock.X, Name.Y - 2),
            Size = new ScalableVector2(107, 23),
            UsePreviousSpriteBatchOptions = true,
        };

        /// <summary>
        /// </summary>
        private void CreateMode() => Mode = new Sprite()
        {
            Parent = Panel,
            Alignment = Ruleset.Alignment,
            Position = new ScalableVector2(Ruleset.X - Ruleset.Width - 12, Ruleset.Y),
            Size = new ScalableVector2(56, 23),
            UsePreviousSpriteBatchOptions = true,
        };

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClicked(object sender, EventArgs e)
        {
            if (SelectedGame.Value != Item)
            {
                SelectedGame.Value = Item;
                return;
            }

            JoinGame();
        }

        /// <summary>
        /// </summary>
        private void Select()
        {
            Panel.Image = UserInterface.SelectedMapset;

            const Easing easing = Easing.OutQuint;
            const int time = 500;

            Panel.ClearAnimations();
            Panel.ChangeWidthTo(DrawableMapset.WIDTH, easing, time);
            FadeSprites(easing, 1, time);

            FadeText(Easing.Linear, 1f, 200);
        }

        /// <summary>
        /// </summary>
        private void Deselect()
        {
            Panel.Image = UserInterface.DeselectedMapset;

            const Easing easing = Easing.OutQuint;
            const int time = 500;

            Panel.ClearAnimations();
            Panel.ChangeWidthTo(DrawableMapset.WIDTH - 50, easing, time);

            const float fade = 0.85f;

            FadeSprites(easing, fade, time);
            FadeText(Easing.Linear, fade, 200);
        }

        /// <summary>
        /// </summary>
        /// <param name="easing"></param>
        /// <param name="fade"></param>
        /// <param name="time"></param>
        private void FadeSprites(Easing easing, float fade, int time)
        {
            foreach (var child in Panel.Children)
            {
                switch (child)
                {
                    case SpriteTextPlus _:
                        continue;
                    case Sprite sprite:
                        // The button & lock are exceptions to the fading
                        if (sprite == ClickableButton || sprite == Lock || sprite == Banner)
                            continue;

                        sprite.ClearAnimations();
                        sprite.FadeTo(fade, easing, time);
                        break;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="easing"></param>
        /// <param name="fade"></param>
        /// <param name="time"></param>
        private void FadeText(Easing easing, float fade, int time)
        {
            foreach (var child in Panel.Children)
            {
                if (!(child is SpriteTextPlus text))
                    continue;

                text.ClearAnimations();
                text.FadeTo(fade, easing, time);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedGameChanged(object sender, BindableValueChangedEventArgs<MultiplayerGame> e)
            => UpdateContent(Item, Index);

        /// <summary>
        /// </summary>
        private void LoadBanner()
        {
            var map = MapManager.FindMapFromMd5(Item.MapMd5);

            // If the user doesn't have the map loaded, and the image isn't the default banner, then
            // switch to it
            if (map == null)
            {
                Banner.Image = UserInterface.DefaultBanner;
                FadeBanner();
                return;
            }

            // Check if the banner is already the same image
            if (BackgroundHelper.MapsetBanners.ContainsKey(map.Mapset.Directory))
            {
                if (Banner.Image == BackgroundHelper.MapsetBanners[map.Mapset.Directory])
                    return;

                Banner.Image = BackgroundHelper.MapsetBanners[map.Mapset.Directory];
                FadeBanner();
                return;
            }

            // Load mapset banner
            if (map != null)
                BackgroundHelper.LoadMapsetBanner(map.Mapset);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBannerLoaded(object sender, BannerLoadedEventArgs e)
        {
            var map = MapManager.FindMapFromMd5(Item.MapMd5);

            if (map != null && e.Mapset != map.Mapset)
                return;
            if (map == null)
            {
                Banner.Image = UserInterface.DefaultBanner;
                FadeBanner();
                return;
            }

            Banner.Image = e.Banner;
            FadeBanner();
        }

        /// <summary>
        /// </summary>
        private void FadeBanner()
        {
            Banner.ClearAnimations();
            Banner.FadeTo(0.85f, Easing.Linear, 200);
        }

        /// <summary>
        /// </summary>
        private void JoinGame()
        {
            if (Item.HasPassword)
                DialogManager.Show(new JoinPasswordGameDialog(Item));
            else
                DialogManager.Show(new JoinGameDialog(Item));
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Texture2D GetModeIcon(MultiplayerGame game)
        {
            switch ((GameMode) game.GameMode)
            {
                case GameMode.Keys4:
                    return UserInterface.Mode4KSmall;
                case GameMode.Keys7:
                    return UserInterface.Mode7KSmall;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Texture2D GetRulesetIcon(MultiplayerGame game)
        {
            switch (game.Ruleset)
            {
                case MultiplayerGameRuleset.Free_For_All:
                    return UserInterface.RulesetFFA;
                case MultiplayerGameRuleset.Team:
                    return UserInterface.RulesetTeam;
                case MultiplayerGameRuleset.Battle_Royale:
                    return UserInterface.RulesetBR;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}