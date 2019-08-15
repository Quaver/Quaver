using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public class DrawableMap : Sprite, IDrawableMapsetComponent
    {
        /// <summary>
        ///     The parent drawable mapset
        /// </summary>
        private DrawableMapset DrawableMapset { get; }

        /// <summary>
        ///     The map this represents
        /// </summary>
        private Map Map { get; set; }

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
        public static int SelectedHeight { get; } = 84;

        /// <summary>
        ///     The height when the map is deselected
        /// </summary>
        public static int DeselectedHeight { get; } = 41;

        /// <summary>
        ///     Displays the name of the difficulty
        /// </summary>
        private SpriteTextPlus Difficulty { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="drawableMapset"></param>
        /// <param name="map"></param>
        /// <param name="index"></param>
        public DrawableMap(DrawableMapset drawableMapset, Map map, int index)
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

            UpdateContent(Map, Index);
            MapManager.Selected.ValueChanged += OnMapChanged;
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
        }

        /// <summary>
        ///     Creates <see cref="Difficulty"/>
        /// </summary>
        private void CreateTextDifficulty()
        {
            Difficulty = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoHeavy), "Difficulty", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 15,
                Visible = false,
                Alpha = 0,
                SetChildrenAlpha = true
            };
        }

        /// <summary>
        /// </summary>
        public void Open()
        {
            ClearAnimations();

            Wait(200);
            FadeTo(1, Easing.Linear, 250);
            Visible = true;

            Difficulty.ClearAnimations();
            Difficulty.Wait(200);
            Difficulty.FadeTo(1, Easing.Linear, 250);
        }

        /// <summary>
        /// </summary>
        public void Close()
        {
            Width = DrawableMapset.Width - DrawableMapset.Border.Thickness * 2;

            SetSize(true);
            ClearAnimations();
            FadeTo(0, Easing.OutQuint, 1);

            Difficulty.ClearAnimations();
            Visible = false;
            Difficulty.FadeTo(0, Easing.OutQuint, 1);
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

        /// <summary>
        ///     Sets the size of the map
        /// </summary>
        public void SetSize(bool animate)
        {
            var width = DrawableMapset.Width - DrawableMapset.Border.Thickness * 2;
            var height = IsSelected ? SelectedHeight - DrawableMapset.Border.Thickness - 1 : DeselectedHeight;

            var size = new ScalableVector2(width, height);

            if (animate)
                ChangeSizeTo(new Vector2(width, height), Easing.OutQuint, 300);
            else
                Size = size;
        }

        /// <summary>
        ///     Sets the tint of the map based on its index
        /// </summary>
        private void SetTint() => Tint = Index % 2 == 0 ? ColorHelper.HexToColor("#363636") : ColorHelper.HexToColor("#242424");

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            SetSize(true);
            DrawableMapset.RealignY();
        }
    }
}