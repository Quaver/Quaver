using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class DrawableMultiplayerTable : PoolableScrollContainer<MultiplayerTableItem>, IMultiplayerGameComponent
    {
        /// <summary>
        /// </summary>
        public bool IsMultiplayer { get; }

        /// <summary>
        /// </summary>
        public Bindable<MultiplayerGame> SelectedGame { get; set; }

        /// <summary>
        /// </summary>
        private Sprite ScrollbarBackground { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <param name="isMultiplayer"></param>
        /// <param name="size"></param>
        public DrawableMultiplayerTable(Bindable<MultiplayerGame> game, bool isMultiplayer, ScalableVector2 size)
            : base(GetAvailableItems(game, isMultiplayer), int.MaxValue, 0, size, size)
        {
            SelectedGame = game;
            IsMultiplayer = isMultiplayer;

            Alpha = 0;
            CreateScrollbar();
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 800;
            ScrollSpeed = 320;

            CreatePool();

            var ruleset = Pool.Find(x => x.Item is MultiplayerTableItemRuleset);

            if (IsMultiplayer && ruleset != null && ruleset.Item.Selector != null)
                ruleset.Item.Selector.Parent = this;

            var players = Pool.Find(x => x.Item is MultiplayerTableItemPlayers);

            if (IsMultiplayer && players != null && players.Item.Selector != null)
                players.Item.Selector.Parent = this;

            for (var i = 0; i < Pool.Count; i++)
            {
                var item = Pool[i];

                item.Size = new ScalableVector2(Width, Height / Pool.Count);
                item.Y = item.Height * i;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            ScrollbarBackground.Visible = false;
            Scrollbar.Visible = false;
            /*ScrollbarBackground.Alpha = MathHelper.Lerp(ScrollbarBackground.Alpha, InputEnabled ? 1 : 0,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 30, 1));

            Scrollbar.Alpha = ScrollbarBackground.Alpha;*/

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<MultiplayerTableItem> CreateObject(MultiplayerTableItem item, int index)
            => new DrawableMultiplayerTableItem(this, item, index);

        /// <summary>
        /// </summary>
        private static List<MultiplayerTableItem> GetAvailableItems(Bindable<MultiplayerGame> game, bool isMultiplayer)
        {
            var items = new List<MultiplayerTableItem>();

            if (!isMultiplayer)
                items.Add(new MultiplayerTableItemInProgress(game, false));
            
            items.AddRange(new List<MultiplayerTableItem>()
            {
                new MultiplayerTableItemFreeMod(game, isMultiplayer),
                new MultiplayerTableItemFreeRate(game, isMultiplayer),
                new MultiplayerTableItemAutoHost(game, isMultiplayer),
                new MultiplayerTableItemAutoHostRotation(game, isMultiplayer),
            });

            if (isMultiplayer)
                items.Add(new MultiplayerTableItemInProgress(game, false));

            items.AddRange(new List<MultiplayerTableItem>()
            {
                new MultiplayerTableItemSongLength(game, isMultiplayer),
                new MultiplayerTableItemDifficultyRange(game, isMultiplayer),
                new MultiplayerTableItemLongNotePercentageRange(game, isMultiplayer)
            });

            items.Insert(0, new MultiplayerTableItemPlayers(game, isMultiplayer));
            items.Insert(0, new MultiplayerTableItemRuleset(game, isMultiplayer));

            return items;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void UpdateState() => Pool.ForEach(x => x.UpdateContent(x.Item, x.Index));

        /// <summary>
        /// </summary>
        private void CreateScrollbar()
        {
            ScrollbarBackground = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                X = 26,
                Size = new ScalableVector2(4, Height),
                Tint = ColorHelper.HexToColor("#474747"),
                Alpha = 0
            };

            Scrollbar.Width = ScrollbarBackground.Width;
            Scrollbar.Parent = ScrollbarBackground;
            Scrollbar.Alignment = Alignment.BotCenter;
            Scrollbar.Tint = Color.White;
            Scrollbar.Alpha = 0;
        }
    }
}