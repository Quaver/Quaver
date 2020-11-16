using System;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Selection.UI.Maps.Components;
using Quaver.Shared.Screens.Selection.UI.Maps.Components.Difficulty;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
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

namespace Quaver.Shared.Screens.Selection.UI.Maps
{
    public class DrawableMapContainer : Sprite
    {
        /// <summary>
        /// </summary>
        private DrawableMap ParentMap { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        private DrawableMapTextDifficultyRating Rating { get; set; }

        /// <summary>
        /// </summary>
        private CachedDifficultyBarDisplay BarDisplay { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ByText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Creator { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private Sprite OnlineGrade { get; set; }

        /// <summary>
        ///     The amount of spacing between the panel and first value
        /// </summary>
        private const int PaddingX = 26;

        /// <summary>
        /// </summary>
        /// <param name="parentMap"></param>
        public DrawableMapContainer(DrawableMap parentMap)
        {
            ParentMap = parentMap;
            Parent = ParentMap;

            Size = new ScalableVector2(1188, 86);

            CreateButton();
            CreateDifficultyName();
            CreateDifficultyRating();
            CreateBarDisplay();
            CreateCreator();
            CreateGrade();

            UsePreviousSpriteBatchOptions = true;

            ModManager.ModsChanged += OnModsChanged;
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ModManager.ModsChanged -= OnModsChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="index"></param>
        public void UpdateContent(Map map, int index)
        {
            Name.Text = map.DifficultyName ?? "";
            Name.Tint = ColorHelper.DifficultyToColor((float) map.DifficultyFromMods(ModManager.Mods));

            Creator.Text = map.Creator ?? "";

            BarDisplay.DifficultyBar.ChangeMap(map);
            Rating.ChangeMap(map);

            if (map.OnlineGrade != Grade.None)
            {
                const int width = 40;

                OnlineGrade.Visible = true;
                OnlineGrade.Image = SkinManager.Skin.Grades[map.OnlineGrade];
                OnlineGrade.Size = new ScalableVector2(width, OnlineGrade.Image.Height / OnlineGrade.Image.Width * width);

                Name.X = OnlineGrade.X + OnlineGrade.Width + 16;
                ByText.X = Name.X;
            }
            else
            {
                Name.X = PaddingX;
                ByText.X = Name.X;
                OnlineGrade.Visible = false;
            }

            Creator.X = ByText.X + ByText.Width + 4;

            if (ParentMap.IsSelected)
                Select(true);
            else
                Deselect(true);
        }

        /// <summary>
        /// </summary>
        public void Select(bool changeWidthInstantly = false)
        {
            Image = SkinManager.Skin?.SongSelect?.MapsetSelected ?? UserInterface.SelectedMapset;

            var fade = 1f;
            var time = 200;

            Name.ClearAnimations();
            Name.FadeTo(fade, Easing.Linear, time);

            ByText.ClearAnimations();
            ByText.FadeTo(fade, Easing.Linear, time);

            Creator.ClearAnimations();
            Creator.FadeTo(fade, Easing.Linear, time);

            Rating.ClearAnimations();
            Rating.FadeTo(fade, Easing.Linear, time);

            BarDisplay.CachedSprite.ClearAnimations();
            BarDisplay.CachedSprite.FadeTo(fade, Easing.Linear, time);

            OnlineGrade.ClearAnimations();
            OnlineGrade.FadeTo(fade, Easing.Linear, time);

            ClearAnimations();

            if (changeWidthInstantly)
                Width = ParentMap.Width;
            else
                ChangeWidthTo((int) ParentMap.Width, Easing.OutQuint, time + 400);
        }

        /// <summary>
        /// </summary>
        public void Deselect(bool changeWidthInstantly = false)
        {
            Image = SkinManager.Skin?.SongSelect.MapsetDeselected ?? UserInterface.DeselectedMapset;

            var fade = 0.85f;
            var time = 200;

            Name.ClearAnimations();
            Name.FadeTo(fade, Easing.Linear, time);

            ByText.ClearAnimations();
            ByText.FadeTo(fade, Easing.Linear, time);

            Creator.ClearAnimations();
            Creator.FadeTo(fade, Easing.Linear, time);

            Rating.ClearAnimations();
            Rating.FadeTo(fade, Easing.Linear, time);

            BarDisplay.CachedSprite.ClearAnimations();
            BarDisplay.CachedSprite.FadeTo(fade, Easing.Linear, time);

            OnlineGrade.ClearAnimations();
            OnlineGrade.FadeTo(fade, Easing.Linear, time);

            ClearAnimations();

            if (changeWidthInstantly)
                Width = ParentMap.Width - 50;
            else
                ChangeWidthTo((int) ParentMap.Width - 50, Easing.OutQuint, time + 400);
        }

        /// <summary>
        ///     Creates <see cref="Button"/>
        /// </summary>
        private void CreateButton()
        {
            var container = (SongSelectContainer<Map>) ParentMap.Container;

            Button = new SongSelectContainerButton(SkinManager.Skin?.SongSelect?.MapsetHovered ?? WobbleAssets.WhiteBox, container.ClickableArea)
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
                Alignment = Alignment.MidCenter,
                UsePreviousSpriteBatchOptions = true,
                Depth = 1
            };

            Button.Clicked += (sender, args) => OnMapClicked();

            Button.RightClicked += (sender, args) =>
            {
                var game = (QuaverGame) GameBase.Game;
                game?.CurrentScreen?.ActivateRightClickOptions(new MapRightClickOptions(ParentMap));
            };
        }

        /// <summary>
        ///     Creates <see cref="Name"/>
        /// </summary>
        private void CreateDifficultyName()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "DIFFICULTY", 26)
            {
                Parent = this,
                Position = new ScalableVector2(PaddingX, 18),
                UsePreviousSpriteBatchOptions = true,
            };
        }

        /// <summary>
        ///     Creates <see cref="Rating"/>
        /// </summary>
        private void CreateDifficultyRating()
        {
            Rating = new DrawableMapTextDifficultyRating(ParentMap.Item)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                UsePreviousSpriteBatchOptions = true,
                FontSize = 24
            };
        }

        /// <summary>
        ///     Creates <see cref="BarDisplay"/>
        /// </summary>
        private void CreateBarDisplay()
        {
            BarDisplay = new CachedDifficultyBarDisplay(new DifficultyBarDisplay(ParentMap.Item, true, true))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -110,
                UsePreviousSpriteBatchOptions = true,
                Visible = true,
                Background =
                {
                    Visible = true,
                    Alpha = 1
                }
            };
        }

        /// <summary>
        ///     Creates <see cref="ByText"/> and <see cref="Creator"/>
        /// </summary>
        private void CreateCreator()
        {
            ByText = new SpriteTextPlus(Name.Font, "By:", 20)
            {
                Parent = this,
                Position = new ScalableVector2(Name.X, Name.Y + Name.Height + 5),
                Tint = SkinManager.Skin?.SongSelect?.MapsetPanelByColor ?? ColorHelper.HexToColor("#757575"),
                UsePreviousSpriteBatchOptions = true
            };

            Creator = new SpriteTextPlus(Name.Font, "Creator", ByText.FontSize)
            {
                Parent = this,
                Position = new ScalableVector2(ByText.X + ByText.Width + 4, ByText.Y),
                Tint = SkinManager.Skin?.SongSelect?.MapsetPanelCreatorColor ?? ColorHelper.HexToColor("#0587e5"),
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateGrade()
        {
            OnlineGrade = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Visible = false,
                X = PaddingX,
                UsePreviousSpriteBatchOptions = true
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
        ///     Called when the map has been clicked
        /// </summary>
        private void OnMapClicked()
        {
            if (ParentMap.Container != null)
            {
                var container = (MapScrollContainer) ParentMap.Container;
                container.SelectedIndex.Value = ParentMap.Index;
            }

            // Map is already selected. Second click should be to play the map
            if (ParentMap.IsSelected)
            {
                Logger.Important($"User clicked on map to play: {ParentMap.Item}", LogType.Runtime, false);

                var game = (QuaverGame) GameBase.Game;
                var screen = game.CurrentScreen as SelectionScreen;
                screen?.ExitToGameplay();

                return;
            }

            MapManager.Selected.Value = ParentMap.Item;
            Logger.Important($"User selected map: {ParentMap.Item}", LogType.Runtime, false);
        }

        /// <summary>
        ///     Called when the activated modifiers has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e)
            => Name.Tint = ColorHelper.DifficultyToColor((float) ParentMap.Item.DifficultyFromMods(ModManager.Mods));
    }
}