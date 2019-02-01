using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Maps.Structures;
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
    public class EditorLayerContainer : PoolableScrollContainer<EditorLayerInfo>
    {
        /// <summary>
        /// </summary>
        public EditorLayerCompositor Compositor { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="compositor"></param>
        /// <param name="poolSize"></param>
        /// <param name="poolStartingIndex"></param>
        /// <param name="size"></param>
        public EditorLayerContainer(EditorLayerCompositor compositor, int poolSize, int poolStartingIndex, ScalableVector2 size)
            : base(CreateLayersList(compositor.Screen.WorkingMap.EditorLayers), poolSize, poolStartingIndex, size, size)
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override PoolableSprite<EditorLayerInfo> CreateObject(EditorLayerInfo l, int index) => new EditorDrawableLayer(Compositor, l, index);

        /// <summary>
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        private static List<EditorLayerInfo> CreateLayersList(IEnumerable<EditorLayerInfo> layers)
        {
            var l = new List<EditorLayerInfo>(layers);
            l.Insert(0, new EditorLayerInfo { Name = "Default Layer", });

            return l;
        }
    }
}