using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Server.Client.Events.Scores;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
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
        public static int MapsetHeight = 97;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = MapsetHeight;

        /// <summary>
        /// </summary>
        public static int WIDTH { get; } = 1188;

        /// <summary>
        ///     Contains the actual mapset
        /// </summary>
        public DrawableMapsetContainer DrawableContainer { get; }

        /// <summary>
        ///     If this mapset is currently selected
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableMapset(PoolableScrollContainer<Mapset> container, Mapset item, int index) : base(container, item, index)
        {
            Size = new ScalableVector2(WIDTH, HEIGHT);

            DrawableContainer = new DrawableMapsetContainer(this)
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                UsePreviousSpriteBatchOptions = true
            };

            Alpha = 0;
            UpdateContent(item, index);

            MapManager.Selected.ValueChanged += OnMapChanged;

            if (OnlineManager.Client != null)
                OnlineManager.Client.OnRetrievedOnlineScores += OnRetrievedOnlineScores;

            MapsetInfoRetriever.MapsetInfoRetrieved += OnMapsetInfoRetrieved;

            ModManager.ModsChanged += OnModsChanged;

            UsePreviousSpriteBatchOptions = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            if (OnlineManager.Client != null)
                OnlineManager.Client.OnRetrievedOnlineScores -= OnRetrievedOnlineScores;

            MapsetInfoRetriever.MapsetInfoRetrieved -= OnMapsetInfoRetrieved;

            ModManager.ModsChanged -= OnModsChanged;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(Mapset item, int index)
        {
            Item = item;
            Index = index;

            IsSelected = Item.Maps.Contains(MapManager.Selected.Value);

            ScheduleUpdate(() =>
            {
                // Make sure the mapset is properly selected/deselected when updating the content
                if (IsSelected)
                    Select();
                else
                    Deselect();

                // Update all the values in the mapset
                DrawableContainer.UpdateContent(Item, Index);
            });
        }

        /// <summary>
        ///     Selects the mapset and performs an animation
        /// </summary>
        public void Select()
        {
            IsSelected = true;
            DrawableContainer?.Select();
        }

        /// <summary>
        ///     Deselects the mapset and performs an animation
        /// </summary>
        public void Deselect()
        {
            IsSelected = false;
            DrawableContainer?.Deselect();
        }

        /// <summary>
        ///     Difficulty name is only visible on mapsets for playlists
        /// </summary>
        public bool IsPlaylistMapset() => DrawableContainer.DifficultyName.Visible;

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
                Select();
        }

        private void OnMapsetInfoRetrieved(object sender, EventArgs args) => UpdateContent(Item, Index);

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRetrievedOnlineScores(object sender, RetrievedOnlineScoresEventArgs e) => UpdateContent(Item, Index);

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e) => UpdateContent(Item, Index);
    }
}