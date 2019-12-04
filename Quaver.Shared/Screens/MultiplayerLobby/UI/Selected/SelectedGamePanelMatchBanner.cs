using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Games;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected
{
    public class SelectedGamePanelMatchBanner : ScrollContainer, IMultiplayerGameComponent
    {
        /// <summary>
        /// </summary>
        public Bindable<MultiplayerGame> SelectedGame { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Background { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Map { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus DifficultyRating { get; set; }

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
        /// <param name="size"></param>
        public SelectedGamePanelMatchBanner(Bindable<MultiplayerGame> selectedGame, ScalableVector2 size) : base(size, size)
        {
            SelectedGame = selectedGame;
            Alpha = 0;

            CreateBackground();
            CreateNameText();
            CreateMapText();
            CreateDifficultyRatingText();
            CreateRuleset();
            CreateMode();

            BackgroundHelper.Loaded += OnBackgroundLoaded;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BackgroundHelper.Loaded -= OnBackgroundLoaded;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBackground()
        {
            Background = new Sprite
            {
                Size = new ScalableVector2(1280, 720),
                Alignment = Alignment.MidCenter,
                Image = UserInterface.MenuBackgroundNormal,
                UsePreviousSpriteBatchOptions = true,
                Alpha = 0f,
            };

            AddContainedDrawable(Background);
        }

        /// <summary>
        /// </summary>
        private void CreateNameText()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 24)
            {
                Parent = this,
                Position = new ScalableVector2(14, 14)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMapText()
        {
            Map = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 21)
            {
                Parent = this,
                Position = new ScalableVector2(Name.X, Name.Y + Name.Height + 32)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDifficultyRatingText()
        {
            DifficultyRating = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 21)
            {
                Parent = this,
                Position = new ScalableVector2(Name.X, Map.Y + Map.Height + 10)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateRuleset()
        {
            Ruleset = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(107, 23),
                Position = new ScalableVector2(-Name.X, Name.Y)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMode()
        {
            Mode = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(56, 23),
                Position = new ScalableVector2(Ruleset.X - Ruleset.Width - 12, Ruleset.Y)
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void UpdateState()
        {
            if (SelectedGame.Value == null)
                return;

            // Load new background
            ThreadScheduler.Run(() =>
            {
                var map = MapManager.FindMapFromMd5(SelectedGame.Value.MapMd5);

                if (map == null)
                    map = MapManager.FindMapFromMd5(SelectedGame.Value.AlternativeMd5);

                Background.ClearAnimations();
                Background.FadeTo(0, Easing.Linear, 200);

                BackgroundHelper.Load(map);
            });

            ScheduleUpdate(() =>
            {
                var maxWidth = (int) Width - 225;

                Name.Text = SelectedGame.Value.Name;
                Name.TruncateWithEllipsis(maxWidth);

                Map.Text = SelectedGame.Value.GetMapName();
                Map.TruncateWithEllipsis(maxWidth);

                DifficultyRating.Text = $"{StringHelper.RatingToString(SelectedGame.Value.DifficultyRating)} - {SelectedGame.Value.GetDifficultyName()}";
                DifficultyRating.Tint = ColorHelper.DifficultyToColor((float) SelectedGame.Value.DifficultyRating);

                Ruleset.Image = DrawableMultiplayerGame.GetRulesetIcon(SelectedGame.Value);
                Mode.Image = DrawableMultiplayerGame.GetModeIcon(SelectedGame.Value);
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e)
        {
            lock (Background.Image)
            {
                Background.Image = e.Texture;
                Background.ClearAnimations();
                Background.FadeTo(0.45f, Easing.Linear, 200);
            }
        }
    }
}