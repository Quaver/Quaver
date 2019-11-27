using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Games
{
    public class MultiplayerGameScrollContainer : SongSelectContainer<MultiplayerGame>
    {
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
        public MultiplayerGameScrollContainer(Bindable<MultiplayerGame> selectedGame,
            Bindable<List<MultiplayerGame>> visibleGames) : base(new List<MultiplayerGame>(), int.MaxValue)
        {
            Alpha = 0f;
            SelectedGame = selectedGame;
            VisibleGames = visibleGames;

            VisibleGames.ValueChanged += OnVisibleGamesChanged;
            VisibleGames.Value = GetTestGames();
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
        /// <returns></returns>
        private static List<MultiplayerGame> GetTestGames()
        {
            var list = new List<MultiplayerGame>();

            for (var i = 0; i < 50; i++)
            {
                var game = new MultiplayerGame()
                {
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