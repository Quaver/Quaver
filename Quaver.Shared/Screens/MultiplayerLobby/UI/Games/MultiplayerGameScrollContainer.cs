using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Server.Common.Objects.Twitch;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Filter;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Input;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Games
{
    public class MultiplayerGameScrollContainer : SongSelectContainer<MultiplayerGame>
    {
        /// <summary>
        /// </summary>
        private Bindable<string> SearchQuery { get; }

        /// <summary>
        /// </summary>
        public override SelectScrollContainerType Type { get; }

        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> SelectedGame { get; }

        /// <summary>
        /// </summary>
        private Bindable<List<MultiplayerGame>> VisibleGames { get; }

        /// <summary>
        /// </summary>
        /// <param name="selectedGame"></param>
        /// <param name="visibleGames"></param>
        /// <param name="searchQuery"></param>
        public MultiplayerGameScrollContainer(Bindable<MultiplayerGame> selectedGame,
            Bindable<List<MultiplayerGame>> visibleGames, Bindable<string> searchQuery)
            : base(new List<MultiplayerGame>(), int.MaxValue)
        {
            Alpha = 0f;

            SelectedGame = selectedGame;
            VisibleGames = visibleGames;
            SearchQuery = searchQuery;

            ScrollbarBackground.Alignment = Alignment.MidLeft;
            ScrollbarBackground.X = -ScrollbarBackground.X;
            VisibleGames.ValueChanged += OnVisibleGamesChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            VisibleGames.ValueChanged -= OnVisibleGamesChanged;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override float GetSelectedPosition() => 0;

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<MultiplayerGame> CreateObject(MultiplayerGame item, int index)
            => new DrawableMultiplayerGame(SelectedGame, this, item, index);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void HandleInput(GameTime gameTime)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void SetSelectedIndex()
        {
        }

        /// <summary>
        /// </summary>
        private void RecreatePool()
        {
            ScheduleUpdate(() =>
            {
                ContentContainer.Y = 0;
                PreviousContentContainerY = 0;
                TargetY = 0;
                PreviousTargetY = 0;

                Pool.ForEach(x => x.Destroy());
                Pool.Clear();

                CreatePool();
                PositionAndContainPoolObjects();
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVisibleGamesChanged(object sender, BindableValueChangedEventArgs<List<MultiplayerGame>> e)
        {
            AvailableItems = e.Value;
            RecreatePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public void Add(MultiplayerGame game)
        {
            if (!MultiplayerLobbyFilterPanel.GameMeetsFilterRequirements(game, SearchQuery.Value))
                return;

            if (AvailableItems.Any(x => x.Id == game.Id))
                return;

            if (Pool.Any(x => x.Item.Id == game.Id))
                return;

            AddObjectToBottom(game, false);
        }

        /// <summary>
        /// </summary>
        public void Remove(MultiplayerGame game)
        {
            var item = Pool.Find(x => x.Item == game);
            AvailableItems.Remove(game);

            // Remove the item if it exists in the pool.
            if (item != null)
            {
                item.Destroy();
                RemoveContainedDrawable(item);
                Pool.Remove(item);
            }

            RecalculateContainerHeight();

            // Reset the pool item index
            for (var i = 0; i < Pool.Count; i++)
            {
                Pool[i].Index = i;
                Pool[i].ClearAnimations();
                Pool[i].MoveToY((PoolStartingIndex + i) * Pool[i].HEIGHT, Easing.OutQuint, 400);
                Pool[i].UpdateContent(Pool[i].Item, i);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static List<MultiplayerGame> GetTestGames()
        {
            var list = new List<MultiplayerGame>();

            for (var i = 0; i < 15; i++)
            {
                var game = new MultiplayerGame()
                {
                    Id = $"{i}",
                    Name = $"Example Game #{i}",
                    Map = $"Example Artist - Title [Map]",
                    PlayerIds = new List<int>() {1, 2, 3, 4, 5},
                    MaxPlayers = 16,
                };

                game.GameMode = i % 2 == 0 ? (byte) 1 : (byte) 2;
                game.Ruleset = MultiplayerGameRuleset.Team;

                list.Add(game);
            }

            return list;
        }
    }
}