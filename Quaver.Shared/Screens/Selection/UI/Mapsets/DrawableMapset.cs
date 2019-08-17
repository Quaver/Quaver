using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.Mapsets.Maps;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public sealed class DrawableMapset : PoolableSprite<Mapset>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 86;

        /// <summary>
        ///     Contains the actual mapset
        /// </summary>
        public DrawableMapsetContainer DrawableContainer { get; }

        /// <summary>
        ///     If this mapset is currently selected
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <summary>
        ///     The maps that are in the set
        /// </summary>
        private List<DrawableMap> Maps { get; set; } = new List<DrawableMap>();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableMapset(PoolableScrollContainer<Mapset> container, Mapset item, int index) : base(container, item, index)
        {
            Size = new ScalableVector2(1188, HEIGHT);
            DrawableContainer = new DrawableMapsetContainer(this) {Parent = this};

            Alpha = 0;
            AddBorder(ColorHelper.HexToColor("#0587e5"), 2);
            UpdateContent(item, index);

            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();

            if (IsSelected)
                RealignY();

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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(Mapset item, int index)
        {
            // Make sure the mapset is properly selected/deselected when updating the content
            if (IsSelected)
                Select();
            else
                Deselect();

            // Update all the values in the mapset
            DrawableContainer.UpdateContent(item, index);

            // If any mapsets exist currently, then dispose of them properly
            Maps?.ForEach(x=> x.Destroy());

            Maps = new List<DrawableMap>();

            // Add new mapsets
            for (var i = 0; i < Item.Maps.Count; i++)
            {
                float height = HEIGHT;

                if (i != 0)
                    height = Maps[i - 1].Height + Maps[i - 1].Y;

                Maps.Add(new DrawableMap(this, Item.Maps[i], i)
                {
                    Parent = this,
                    Y = height,
                    Alignment = Alignment.TopCenter
                });
            }
        }

        /// <summary>
        ///     Selects the mapset and performs an animation
        ///
        ///     TODO: Add argument to change size immediately.
        /// </summary>
        public void Select()
        {
            IsSelected = true;

            var total = DrawableMap.SelectedHeight + DrawableMap.DeselectedHeight * (Item.Maps.Count - 1) + HEIGHT;
            ChangeHeightTo(total, Easing.OutQuint, 500);

            Maps.ForEach(x => x.Open());
        }

        /// <summary>
        ///     Deselects the mapset and performs an animation
        ///
        ///     TODO: Add argument to change size immediately.
        /// </summary>
        public void Deselect()
        {
            IsSelected = false;

            ChangeHeightTo(HEIGHT, Easing.OutQuint, 500);

            Maps.ForEach(x => x.Close());
        }

        /// <summary>
        /// </summary>
        private void HandleInput()
        {
            if (!IsSelected)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Up))
            {
                var mapIndex = Item.Maps.FindIndex(x => x == MapManager.Selected.Value);

                if (mapIndex - 1 < 0)
                    return;

                MapManager.Selected.Value = Item.Maps[mapIndex - 1];
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Down))
            {
                var mapIndex = Item.Maps.FindIndex(x => x == MapManager.Selected.Value);

                if (mapIndex + 1 > Item.Maps.Count - 1)
                    return;

                MapManager.Selected.Value = Item.Maps[mapIndex + 1];
            }
        }

        /// <summary>
        ///     Realigns all the maps to the correct height
        /// </summary>
        public void RealignY()
        {
            for (var i = 0; i < Item.Maps.Count; i++)
            {
                float height = HEIGHT;

                if (i != 0)
                    height = Maps[i - 1].Height + Maps[i - 1].Y;

                Maps[i].Y = height;
            }
        }

        /// <summary>
        ///     Handles opening/closing the mapset when the map has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            if (!Item.Maps.Contains(e.Value))
                Deselect();
            else if (!IsSelected)
            {
                Select();
                Maps.ForEach(x => x.SetSize(false));
            }
        }
    }
}