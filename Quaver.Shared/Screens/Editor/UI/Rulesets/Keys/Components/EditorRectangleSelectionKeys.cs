/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Components
{
    public class EditorRectangleSelection : Sprite
    {
        /// <summary>
        /// </summary>
        private EditorRulesetKeys Ruleset { get; }

        /// <summary>
        /// </summary>
        public bool IsSelecting { get; private set; }

        /// <summary>
        /// </summary>
        private Vector2 StartingPoint { get; set; }

        /// <summary>
        /// </summary>
        private double TimeDragStart { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorRectangleSelection(EditorRulesetKeys ruleset)
        {
            Ruleset = ruleset;
            Tint = Color.White;
            Alpha = 0.35f;
            AddBorder(Color.White, 2);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Ruleset.CompositionTool.Value != EditorCompositionTool.Select)
            {
                base.Update(gameTime);
                return;
            }

            if (MouseManager.CurrentState.LeftButton == ButtonState.Pressed &&
                MouseManager.PreviousState.LeftButton == ButtonState.Pressed)
            {
                if (IsSelecting)
                {
                    Width = Math.Abs(MouseManager.CurrentState.X - StartingPoint.X);
                    Height = Math.Abs(MouseManager.CurrentState.Y - StartingPoint.Y);
                    X = Math.Min(StartingPoint.X, MouseManager.CurrentState.X);
                    Y = Math.Min(StartingPoint.Y, MouseManager.CurrentState.Y);

                    var view = (EditorScreenView) Ruleset.Screen.View;

                    // User is scrolling down past the navigation bar, continue to seek the map backwards
                    if (MouseManager.CurrentState.Y >= view.ControlBar.ScreenRectangle.Y && AudioEngine.Track.IsPaused)
                    {
                        if (MouseManager.CurrentState.Y - view.ControlBar.ScreenRectangle.Y <=  5)
                            AudioEngine.Track.Seek(AudioEngine.Track.Time - 10);
                        else if (MouseManager.CurrentState.Y - view.ControlBar.ScreenRectangle.Y <= 10)
                            AudioEngine.Track.Seek(AudioEngine.Track.Time - 30);
                        else
                            AudioEngine.Track.Seek(AudioEngine.Track.Time - 100);

                        Ruleset.Screen.SetHitSoundObjectIndex();
                    }

                    // User is scrolling down past the navigation bar, continue to seek the map backwards
                    if (MouseManager.CurrentState.Y <= 20 && AudioEngine.Track.IsPaused)
                    {
                        if (20 - MouseManager.CurrentState.Y <=  5)
                            AudioEngine.Track.Seek(AudioEngine.Track.Time + 10);
                        else if (20 - MouseManager.CurrentState.Y <= 10)
                            AudioEngine.Track.Seek(AudioEngine.Track.Time + 30);
                        else
                            AudioEngine.Track.Seek(AudioEngine.Track.Time + 100);

                        Ruleset.Screen.SetHitSoundObjectIndex();
                    }
                }
                else
                {
                    var rect = Ruleset.ScrollContainer.ScreenRectangle;
                    rect.Width += 150;

                    var graph = Ruleset.VisualizationGraphs[ConfigManager.EditorVisualizationGraph.Value];
                    var view = (EditorScreenView) Ruleset.Screen.View;

                    if (!GraphicsHelper.RectangleContains(graph.Graph.ScreenRectangle, MouseManager.CurrentState.Position)
                        && GraphicsHelper.RectangleContains(rect, Ruleset.MouseInitialClickPosition)
                        && !GraphicsHelper.RectangleContains(view.ControlBar.ScreenRectangle, Ruleset.MouseInitialClickPosition))
                    {
                        IsSelecting = true;
                        StartingPoint = new Vector2(MouseManager.CurrentState.X, MouseManager.CurrentState.Y);
                        TimeDragStart = (int) Ruleset.ScrollContainer.GetTimeFromY(MouseManager.CurrentState.Y) / Ruleset.ScrollContainer.TrackSpeed;
                        X = MouseManager.CurrentState.X;
                        Y = MouseManager.CurrentState.Y;
                    }
                }
            }
            else
            {
                // Before fully stopping the drag, make sure to select all objects within the box and range.
                if (IsSelecting)
                {
                    var timeDragEnd = (int) Ruleset.ScrollContainer.GetTimeFromY(MouseManager.CurrentState.Y) / Ruleset.ScrollContainer.TrackSpeed;

                    var startLane = (int) (( StartingPoint.X / (Ruleset.ScrollContainer.ScreenRectangle.X - Ruleset.ScrollContainer.Width +
                                                           Ruleset.ScrollContainer.LaneSize * Ruleset.WorkingMap.GetKeyCount()) - 1) * 10 + 1);

                    var endLane = (int) (( MouseManager.CurrentState.X / (Ruleset.ScrollContainer.ScreenRectangle.X - Ruleset.ScrollContainer.Width +
                                                                Ruleset.ScrollContainer.LaneSize * Ruleset.WorkingMap.GetKeyCount()) - 1) * 10 + 1);

                    // Select all objects within the given range.
                    Ruleset.ScrollContainer.HitObjects.FindAll(x =>
                    {
                        var yInbetween = TimeDragStart > timeDragEnd ?
                            IsBetween(x.Info.StartTime, timeDragEnd, TimeDragStart)
                            : IsBetween(x.Info.StartTime, TimeDragStart, timeDragEnd);

                        return yInbetween && (startLane < endLane ? IsBetween(x.Info.Lane, startLane, endLane) :
                                   IsBetween(x.Info.Lane, endLane, startLane));
                    }).ForEach(x => Ruleset.SelectHitObject(x));
                }

                Size = new ScalableVector2(0, 0);
                IsSelecting = false;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static bool IsBetween<T>(T item, T start, T end) => Comparer<T>.Default.Compare(item, start) >= 0
                                                                   && Comparer<T>.Default.Compare(item, end) <= 0;
    }
}