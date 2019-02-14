/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps;
using Quaver.Shared.Audio;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Editor.UI.Graphing.Graphs;
using Quaver.Shared.Screens.Editor.UI.Rulesets;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Window;
using IDrawable = Wobble.Graphics.IDrawable;

namespace Quaver.Shared.Screens.Editor.UI.Graphing
{
    public class EditorVisualizationGraphContainer : IDrawable, IDisposable
    {
        /// <summary>
        /// </summary>
        public EditorVisualizationGraphType Type { get; }

        /// <summary>
        /// </summary>
        private EditorRuleset Ruleset { get; }

        /// <summary>
        ///     The sprite that holds the currently cached one.
        /// </summary>
        public ImageButton Graph { get; private set; }

        /// <summary>
        ///     The raw graph that needs to be cached to a RenderTarget2D
        /// </summary>
        public EditorVisualizationGraph GraphRaw { get; }

        /// <summary>
        ///     If on the next draw call we'll be force recaching the graph.
        /// </summary>
        private bool ForceRecaching { get; set; }

        /// <summary>
        ///     Keeps track of if the user is dragging the seek bar in the last frame.
        /// </summary>
        private bool DraggingInLastFrame { get; set; }

        /// <summary>
        /// </summary>
        private Sprite SeekBarLine { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ruleset"></param>
        /// <param name="map"></param>
        public EditorVisualizationGraphContainer(EditorVisualizationGraphType type, EditorRuleset ruleset, Qua map)
        {
            Type = type;
            Ruleset = ruleset;

            switch (type)
            {
                case EditorVisualizationGraphType.Tick:
                    GraphRaw = new EditorTickGraph(this, map, ruleset);
                    break;
                case EditorVisualizationGraphType.Density:
                    GraphRaw = new EditorNoteDensityGraph(this, map, ruleset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            HandleDragging();

            if (SeekBarLine != null)
                SeekBarLine.Y = Graph.Height - (float) ( AudioEngine.Track.Time / AudioEngine.Track.Length ) * Graph.Height;

            Graph?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime) => CacheGraph(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Destroy()
        {
            GraphRaw.Destroy();
            Graph.Destroy();
        }

        /// <summary>
        ///     Caches the graph to a RenderTarget2D and updates the sprite's image.
        /// </summary>
        private void CacheGraph(GameTime gameTime)
        {
            if (Graph != null && !ForceRecaching)
                return;

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception e)
            {
                // ignored
            }

            var game = GameBase.Game as QuaverGame;

            game?.ScheduledRenderTargetDraws.Add(() =>
            {
                var (pixelWidth, pixelHeight) = GraphRaw.AbsoluteSize * WindowManager.ScreenScale;

                var renderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, (int) pixelWidth, (int) pixelHeight, false,
                    GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

                GameBase.Game.GraphicsDevice.SetRenderTarget(renderTarget);
                GameBase.Game.GraphicsDevice.Clear(Color.Transparent);

                GraphRaw.SpriteBatchOptions = new SpriteBatchOptions {BlendState = BlendState.Opaque};
                GraphRaw.Draw(gameTime);
                GameBase.Game.SpriteBatch.End();

                Texture2D outputTexture = renderTarget;

                GameBase.Game.GraphicsDevice.SetRenderTarget(null);

                if (ForceRecaching && Graph != null)
                {
                    Graph.Image = outputTexture;
                    ForceRecaching = false;
                }
                else
                {
                    Graph = new ImageButton(outputTexture)
                    {
                        Parent = Ruleset.Screen.View.Container,
                        Size = GraphRaw.Size,
                        SpriteBatchOptions = new SpriteBatchOptions {BlendState = BlendState.AlphaBlend},
                        DestroyIfParentIsNull = false
                    };

                    var view = Ruleset.Screen.View as EditorScreenView;

                    Graph.Height = GraphRaw.Height - view.MenuBar.Height;


                    var children = Ruleset.Screen.View.Container.Children;

                    // Make sure the navbar appears over the graph, so that the hover tooltips are on top.
                    // ListHelper.Swap(Ruleset.Screen.View.Container.Children, children.IndexOf(view?.NavigationBar), children.IndexOf(Graph));

                    CreateProgressSeekBar();
                }

                SetGraphXPos();
            });
        }

        /// <summary>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetGraphXPos()
        {
            switch (Ruleset)
            {
                case EditorRulesetKeys keys:
                    var view = (EditorScreenView) keys.Screen.View;

                    if (Graph != null)
                    {
                        Graph.Y = view.MenuBar.Height;

                        keys.ScrollContainer.Update(new GameTime(TimeSpan.Zero, TimeSpan.Zero));
                        Graph.X = keys.ScrollContainer.AbsolutePosition.X - Graph.Width - 10;
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            Graph?.Destroy();
            GraphRaw?.Destroy();
        }

        /// <summary>
        ///     Forces a reaching of the graph.
        /// </summary>
        public void ForceRecache() => ForceRecaching = true;

        /// <summary>
        ///     Handles the dragging of the mouse to change the song position.
        /// </summary>
        private void HandleDragging()
        {
            if (Graph != null && Graph.IsHeld && !AudioEngine.Track.IsDisposed)
            {
                var percentage = (MouseManager.CurrentState.Y - Graph.AbsolutePosition.Y) / Graph.AbsoluteSize.Y;
                var targetPos = (1 - percentage) * AudioEngine.Track.Length;

                if ((int) targetPos != (int) AudioEngine.Track.Time && targetPos >= 0 && targetPos <= AudioEngine.Track.Length)
                {
                    if (AudioEngine.Track.IsPlaying)
                        AudioEngine.Track.Pause();

                    AudioEngine.Track.Seek(targetPos);

                    var screen = Ruleset.Screen;
                    screen.SetHitSoundObjectIndex();

                    if (screen.Ruleset is EditorRulesetKeys ruleset)
                        ruleset.ScrollContainer.CheckIfObjectsOnScreen();
                }

                DraggingInLastFrame = true;
            }
            else if (DraggingInLastFrame)
            {
                if (AudioEngine.Track.IsPaused || (AudioEngine.Track.IsStopped && !AudioEngine.Track.HasPlayed))
                    AudioEngine.Track.Play();

                DraggingInLastFrame = false;
            }
        }

        /// <summary>
        /// </summary>
        private void CreateProgressSeekBar() => SeekBarLine = new Sprite()
        {
            Parent = Graph,
            Size = new ScalableVector2(Graph.Width, 4),
            Tint = Color.White,
            Y = (float) ( AudioEngine.Track.Time / AudioEngine.Track.Length) * Graph.Height
        };
    }
}
