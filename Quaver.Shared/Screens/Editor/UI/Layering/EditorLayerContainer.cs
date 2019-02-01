using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorLayerContainer : PoolableScrollContainer<int>
    {
        /// <summary>
        /// </summary>
        public EditorLayerCompositor Compositor { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="compositor"></param>
        /// <param name="availableItems"></param>
        /// <param name="poolSize"></param>
        /// <param name="poolStartingIndex"></param>
        /// <param name="size"></param>
        public EditorLayerContainer(EditorLayerCompositor compositor, List<int> availableItems, int poolSize, int poolStartingIndex, ScalableVector2 size)
            : base(availableItems, poolSize, poolStartingIndex, size, size)
        {
            Compositor = compositor;
            Tint = Color.Transparent;
            Scrollbar.Tint = ColorHelper.HexToColor("#CCCCCC");
            Scrollbar.Width = 3;
            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;

            CreatePool();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position) && DialogManager.Dialogs.Count == 0;
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override PoolableSprite<int> CreateObject() => new EditorDrawableLayer(Compositor, "None");
    }
}