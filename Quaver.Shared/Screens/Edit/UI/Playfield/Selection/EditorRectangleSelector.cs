using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.UI.Footer;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Selection
{
    public class EditorRectangleSelector : Sprite
    {
        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private Bindable<EditorCompositionTool> Tool { get; }

        /// <summary>
        /// </summary>
        private EditorPlayfield Playfield { get; }

        /// <summary>
        /// </summary>
        private EditorFooter Footer { get; }

        /// <summary>
        /// </summary>
        private IAudioTrack Track { get; }

        /// <summary>
        /// </summary>
        private BindableList<HitObjectInfo> SelectedHitObjects { get; }

        /// <summary>
        ///     Returns if the user is currently dragging/selecting
        /// </summary>
        private bool IsSelecting { get; set; }

        /// <summary>
        ///     The initial mouse position of where the user began the selection
        /// </summary>
        private Vector2 StartingPoint { get; set; }

        /// <summary>
        ///     The time in the track that the user began dragging
        /// </summary>
        private double TimeDragStart { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="tool"></param>
        /// <param name="playfield"></param>
        /// <param name="footer"></param>
        /// <param name="track"></param>
        /// <param name="selectedHitObjects"></param>
        public EditorRectangleSelector(Qua workingMap, Bindable<EditorCompositionTool> tool, EditorPlayfield playfield,
            EditorFooter footer, IAudioTrack track, BindableList<HitObjectInfo> selectedHitObjects)
        {
            WorkingMap = workingMap;
            Tool = tool;
            Playfield = playfield;
            Footer = footer;
            Track = track;
            SelectedHitObjects = selectedHitObjects;

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
            if (Tool.Value != EditorCompositionTool.Select)
            {
                base.Update(gameTime);
                return;
            }

            HandleSelection();

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void HandleSelection()
        {
            if (MouseManager.CurrentState.LeftButton == ButtonState.Released && MouseManager.PreviousState.LeftButton == ButtonState.Released)
            {
                HandleButtonReleased();
                return;
            }

            if (IsSelecting)
                HandleDrag();
            else
                HandleButtonInitiallyPressed();
        }

        /// <summary>
        /// </summary>
        private void HandleButtonInitiallyPressed()
        {
            if (IsSelecting || !MouseManager.IsUniquePress(MouseButton.Left))
                return;

            if (!Button.IsGloballyClickable)
                return;

            if (Playfield.GetHoveredHitObject() != null)
                return;

            if (ButtonManager.Buttons.Any(x => x.IsHovered) && ! Playfield.Button.IsHovered)
                return;

            var clickArea = new RectangleF(Playfield.ScreenRectangle.X - 300, Playfield.ScreenRectangle.Y,
                Playfield.Width + 700, Playfield.Height);

            if (!GraphicsHelper.RectangleContains(clickArea, MouseManager.CurrentState.Position))
                return;

            if (KeyboardManager.CurrentState.IsKeyUp(Keys.LeftControl) && KeyboardManager.CurrentState.IsKeyUp(Keys.RightControl))
                SelectedHitObjects.Clear();

            IsSelecting = true;
            StartingPoint = MouseManager.CurrentState.Position;
            TimeDragStart = Playfield.GetTimeFromY(MouseManager.CurrentState.Y) / Playfield.TrackSpeed;
            X = MouseManager.CurrentState.X;
            Y = MouseManager.CurrentState.Y;
        }

        /// <summary>
        /// </summary>
        private void HandleDrag()
        {
            if (!IsSelecting)
                return;

            Width = Math.Abs(MouseManager.CurrentState.X - StartingPoint.X);
            Height = Math.Abs(MouseManager.CurrentState.Y - StartingPoint.Y);
            X = Math.Min(StartingPoint.X, MouseManager.CurrentState.X);
            Y = Math.Min(StartingPoint.Y, MouseManager.CurrentState.Y);

            double seekTime;

            // User is scrolling down past the navigation bar, continue to seek the map backwards
            if (MouseManager.CurrentState.Y >= Footer.ScreenRectangle.Y && !Track.IsPlaying)
            {
                if (MouseManager.CurrentState.Y - Footer.ScreenRectangle.Y <= 30)
                    seekTime = Track.Time - 2;
                else if (MouseManager.CurrentState.Y - Footer.ScreenRectangle.Y <= 60)
                    seekTime = Track.Time - 6;
                else
                    seekTime = Track.Time - 50;

                if (seekTime < 0 || seekTime > Track.Length)
                    return;

                Track.Seek(seekTime);
            }

            // User is scrolling down past the navigation bar, continue to seek the map backwards
            if (MouseManager.CurrentState.Y > 20 || Track.IsPlaying)
                return;

            if (20 - MouseManager.CurrentState.Y <= 30)
                seekTime = Track.Time + 2;
            else if (20 - MouseManager.CurrentState.Y <= 60)
                seekTime = Track.Time + 6;
            else
                seekTime = Track.Time + 50;

            if (seekTime < 0 || seekTime > Track.Length)
                return;

            Track.Seek(seekTime);
        }

        /// <summary>
        /// </summary>
        private void HandleButtonReleased()
        {
            if (IsSelecting && StartingPoint -  new Vector2(MouseManager.CurrentState.X, MouseManager.CurrentState.Y) != Vector2.Zero)
            {
                var timeDragEnd = Playfield.GetTimeFromY(MouseManager.CurrentState.Y) / Playfield.TrackSpeed;

                var startLane = Playfield.GetLaneFromX(StartingPoint.X);
                var endLane = Playfield.GetLaneFromX(MouseManager.CurrentState.X);

                var foundObjects = WorkingMap.HitObjects.FindAll(x =>
                {
                    var yInbetween = TimeDragStart > timeDragEnd ?
                        IsBetween(x.StartTime, timeDragEnd, TimeDragStart)
                        : IsBetween(x.StartTime, TimeDragStart, timeDragEnd);

                    return yInbetween && (startLane < endLane ? IsBetween(x.Lane, startLane, endLane) :
                               IsBetween(x.Lane, endLane, startLane));
                });

                foreach (var ho in foundObjects)
                {
                    if (!SelectedHitObjects.Value.Contains(ho))
                        SelectedHitObjects.Add(ho);
                }
            }

            IsSelecting = false;
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            StartingPoint = Vector2.Zero;
            TimeDragStart = 0;
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