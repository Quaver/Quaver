using System.Collections.Generic;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Containers;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class DrawableMultiplayerTable : PoolableScrollContainer<MultiplayerTableItem>, IMultiplayerGameComponent
    {
        /// <summary>
        /// </summary>
        public Bindable<MultiplayerGame> SelectedGame { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <param name="size"></param>
        public DrawableMultiplayerTable(Bindable<MultiplayerGame> game, ScalableVector2 size)
            : base(GetAvailableItems(game), int.MaxValue, 0, size, size)
        {
            SelectedGame = game;
            Alpha = 0;
            Scrollbar.Visible = false;

            CreatePool();
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
        private static List<MultiplayerTableItem> GetAvailableItems(Bindable<MultiplayerGame> game)
        {
            var items = new List<MultiplayerTableItem>()
            {
                new MultiplayerTableItemPlayers(game),
                new MultiplayerTableItemInProgress(game),
                new MultiplayerTableItemFreeMod(game),
                new MultiplayerTableItemFreeRate(game),
                new MultiplayerTableItemAutoHostRotation(game),
                new MultiplayerTableItemHealthType(game),
                new MultiplayerTableItemLifeCount(game),
                new MultiplayerTableItemAllowedGameModes(game),
                new MultiplayerTableItemSongLength(game),
                new MultiplayerTableItemDifficultyRange(game),
                new MultiplayerTableItemLongNotePercentageRange(game)
            };

            return items;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void UpdateState() => Pool.ForEach(x => x.UpdateContent(x.Item, x.Index));
    }
}