using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Components;
using Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Components.Difficulty;
using Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Metadata;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps
{
    public class DrawableMap : ImageButton, IDrawableMapsetComponent, IDrawableMapComponent
    {
        /// <summary>
        ///     The parent drawable mapset
        /// </summary>
        private DrawableMapset DrawableMapset { get; }

        /// <summary>
        ///     The map this represents
        /// </summary>
        public Map Map { get; private set; }

        /// <summary>
        ///     The index this map is in the mapset
        /// </summary>
        private int Index { get; set; }

        /// <summary>
        ///     Dictates if the map is selected
        /// </summary>
        private bool IsSelected => MapManager.Selected?.Value == Map;

        /// <summary>
        ///     The height when the map is selected
        /// </summary>
        public static int SelectedHeight { get; } = 90;

        /// <summary>
        ///     The height when the map is deselected
        /// </summary>
        public static int DeselectedHeight { get; } = 50;

        /// <summary>
        ///     Displays the name of the difficulty
        /// </summary>
        private DrawableMapTextDifficultyName Difficulty { get; set; }

        /// <summary>
        ///     Displays the difficulty rating of the map
        /// </summary>
        private DrawableMapTextDifficultyRating DifficultyRating { get; set; }

        /// <summary>
        ///     The button that allows the user to play
        /// </summary>
        private DrawableMapButtonPlay PlayButton { get; set; }

        /// <summary>
        ///     The button that sends the user to the editor
        /// </summary>
        private DrawableMapButtonEdit EditButton { get; set; }

        /// <summary>
        ///     Visual represnetation of the difficulty
        /// </summary>
        private CachedDifficultyBarDisplay DifficultyBar { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="drawableMapset"></param>
        /// <param name="map"></param>
        /// <param name="index"></param>
        public DrawableMap(DrawableMapset drawableMapset, Map map, int index) : base(WobbleAssets.WhiteBox)
        {
            Parent = DrawableMapset;
            DrawableMapset = drawableMapset;
            Map = map;
            Index = index;

            Alpha = 0;
            SetChildrenVisibility = true;
            SetSize(false);
            SetTint();
            CreateTextDifficulty();
            CreateTextRating();
            CreateDifficultyBar();
            CreateActionButtons();

            UpdateContent(Map, Index);
            MapManager.Selected.ValueChanged += OnMapChanged;
            Clicked += OnMapClicked;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            IsClickable = DrawableMapset.IsSelected;
            HandleHoverAnimation(gameTime);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="index"></param>
        public void UpdateContent(Map map, int index)
        {
            Map = map;
            Index = index;

            Alpha = IsSelected ? 1 : 0;
            SetTint();

            Difficulty.Text = Map.DifficultyName;
            Difficulty.Tint = ColorHelper.DifficultyToColor((float) Map.DifficultyFromMods(ModManager.Mods));

            DifficultyRating.UpdateText();
        }

        /// <summary>
        ///     Creates <see cref="Difficulty"/>
        /// </summary>
        private void CreateTextDifficulty()
        {
            Difficulty = new DrawableMapTextDifficultyName(this)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = DrawableMapset.DrawableContainer.Title.X,
                Visible = false,
                Alpha = 0,
                SetChildrenAlpha = true
            };
        }

        /// <summary>
        ///     Creates <see cref="DifficultyRating"/>
        /// </summary>
        private void CreateTextRating()
        {
            DifficultyRating = new DrawableMapTextDifficultyRating(Map)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -12,
                Visible = false,
                Alpha = 0,
                SetChildrenAlpha = true
            };
        }

        /// <summary>
        ///     Creates <see cref="DifficultyBarDisplay"/>
        /// </summary>
        private void CreateDifficultyBar()
        {
            DifficultyBar = new CachedDifficultyBarDisplay(new DifficultyBarDisplay(Map, false, true))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = DifficultyRating.X - 60,
                Visible = false,
                SetChildrenVisibility = true
            };
        }

        /// <summary>
        ///
        /// </summary>
        private void CreateActionButtons()
        {
            PlayButton = new DrawableMapButtonPlay(Map)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = DifficultyRating.X,
                Y = 22,
                Visible = false,
                Alpha = 0,
                SetChildrenAlpha = true
            };

            EditButton = new DrawableMapButtonEdit(Map)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = PlayButton.X - PlayButton.Width - 20,
                Y = PlayButton.Y,
                Visible = false,
                Alpha = 0,
                SetChildrenAlpha = true
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Open()
        {
            ClearAnimations();

            // Make the entire sprite visible and fade in the background
            Wait(200);
            FadeTo(1, Easing.Linear, 250);
            Visible = true;

            Children.ForEach(x =>
            {
                if (x is IDrawableMapComponent metadata)
                    metadata.Open();
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Close()
        {
            // Make sure the sizes are all up-to-date since we're now closing it
            SetSize(true);

            // Fade to 0 instantly and make it invisible
            ClearAnimations();
            Alpha = 0;
            Visible = false;

            Children.ForEach(x =>
            {
                if (x is IDrawableMapComponent metadata)
                    metadata.Close();
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Select()
        {
            Tint = ColorHelper.HexToColor("#293943");
            DifficultyBar.Y = -18;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Deselect()
        {
            if (!IsHovered)
                Tint = GetDefaultcolor();

            DifficultyBar.Y = 0;
        }

        /// <summary>
        ///     Sets the size of the map.
        ///
        ///     The selected map will have a different height to deselected ones.
        /// </summary>
        public void SetSize(bool animate)
        {
            var width = DrawableMapset.Width - DrawableMapset.Border.Thickness * 2;
            var height = IsSelected ? SelectedHeight - DrawableMapset.Border.Thickness - 1 : DeselectedHeight;

            if ((int) Width == (int) width && (int) Height == (int) height)
                return;

            var size = new ScalableVector2(width, height);

            if (animate)
                ChangeSizeTo(new Vector2(width, height), Easing.OutQuint, 300);
            else
                Size = size;
        }

        /// <summary>
        ///     Sets the tint of the map based on its index (Light, Dark, Light, Dark, etc...)
        /// </summary>
        private void SetTint() => Tint = GetDefaultcolor();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Color GetDefaultcolor() => Index % 2 == 0 ? ColorHelper.HexToColor("#363636") : ColorHelper.HexToColor("#242424");

        /// <summary>
        ///     Called when a new map has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            // Update the size of the map, so that it is up-to-date and aligned properly.
            SetSize(true);

            if (MapManager.Selected.Value == Map)
                Select();
            else
                Deselect();
        }

        /// <summary>
        ///     Called when the map has been clicked.
        ///
        ///     Either changes the map, or goes to play
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapClicked(object sender, EventArgs e)
        {
            if (MapManager.Selected.Value == Map)
            {
                Logger.Important($"User clicked on DrawableMap: {Map}. Heading to gameplay", LogType.Runtime, false);
                return;
            }

            MapManager.Selected.Value = Map;
            Logger.Important($"User selected DrawableMap: {Map}", LogType.Runtime, false);
        }

        /// <summary>
        ///     Performs an animation when hovering over the map
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleHoverAnimation(GameTime gameTime)
        {
            if (IsSelected)
                return;

            Color targetColor;

            if (IsHovered)
                // targetColor = Index % 2 == 0 ? ColorHelper.HexToColor("#3F3F3F") : ColorHelper.HexToColor("#4D4D4D");
                targetColor = ColorHelper.HexToColor("#4D4D4D");
            else
                targetColor = GetDefaultcolor();

            FadeToColor(targetColor, GameBase.Game.TimeSinceLastFrame, 60);
        }
    }
}