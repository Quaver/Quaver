using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Multiplayer.Table.Scrolling
{
    public sealed class ResultsMultiplayerScrollContainer : PoolableScrollContainer<ScoreProcessor>
    {
        /// <summary>
        /// </summary>
        private MultiplayerGame Game { get; }

        /// <summary>
        /// </summary>
        private Dictionary<string, SpriteTextPlus> Headers { get; }

        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="availableItems"></param>
        /// <param name="size"></param>
        /// <param name="game"></param>
        /// <param name="headers"></param>
        /// <param name="map"></param>
        public ResultsMultiplayerScrollContainer(ScalableVector2 size, List<ScoreProcessor> availableItems, MultiplayerGame game,
            Dictionary<string, SpriteTextPlus> headers, Map map) : base(availableItems, int.MaxValue, 0, size, size)
        {
            Game = game;
            Headers = headers;
            Map = map;

            InputEnabled = true;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 320;

            Scrollbar.Width = 4;
            Scrollbar.X = 6;
            Scrollbar.Tint = Color.White;

            Alpha = 0;
            CreatePool();
            RecalculateContainerHeight();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<ScoreProcessor> CreateObject(ScoreProcessor item, int index)
            => new ResultsMultiplayerPlayer(this, item, index, Game, Headers, Map);
    }
}