using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    /// <summary>
    ///     Flattens the visible SV and SF graphs into one buffered sprite.
    /// </summary>
    internal sealed class EditorPlayfieldScrollGraphCache
    {
        private const float DefaultLineWidth = 40;
        private const float MinimumLineWidth = 10;
        private const float MaximumLineWidth = 150;
        private const float LineHeight = 2;
        private const float SvRightPadding = 2;

        private readonly EditorPlayfield _playfield;
        private readonly Qua _map;
        private readonly Sprite _cachedGraph;

        private RenderTarget2D _renderTarget;
        private double _cacheStartTime;
        private double _cacheEndTime;
        private float _cacheWidth;
        private float _cacheHeight;
        private int _revision;
        private bool _cacheBoundsValid;
        private bool _dirty = true;
        private bool _buildQueued;
        private bool _destroyed;

        private string _observedSelectedGroupId;
        private bool _observedColorByTimingGroup;
        private float _observedTrackSpeed;
        private float _observedPlayfieldWidth;
        private float _observedPlayfieldHeight;
        private Vector2 _observedScreenScale;

        public EditorPlayfieldScrollGraphCache(EditorPlayfield playfield, Qua map)
        {
            _playfield = playfield;
            _map = map;
            _observedSelectedGroupId = playfield.ActionManager.EditScreen.SelectedScrollGroupId;
            _observedColorByTimingGroup = ConfigManager.EditorColorSvLineByTimingGroup.Value;
            _observedTrackSpeed = playfield.TrackSpeed;
            _observedPlayfieldWidth = playfield.Width;
            _observedPlayfieldHeight = playfield.Height;
            _observedScreenScale = WindowManager.ScreenScale;

            _cachedGraph = new Sprite
            {
                Image = UserInterface.BlankBox,
                Visible = false,
                DrawIfOffScreen = true,
                UsePreviousSpriteBatchOptions = true
            };
        }

        public void Update(GameTime gameTime)
        {
            if (_destroyed)
                return;

            ObserveCacheInputs();

            var (visibleStart, visibleEnd) = GetVisibleTimeRange();
            var viewportOutsideCache = !_cacheBoundsValid || visibleStart < _cacheStartTime || visibleEnd > _cacheEndTime;

            if (viewportOutsideCache)
            {
                _cachedGraph.Visible = false;

                if (!_dirty)
                    Invalidate();
            }

            if (!RenderTargetIsValid())
            {
                _cachedGraph.Visible = false;

                if (!_dirty)
                    Invalidate();
            }

            UpdateCachedGraphPosition();
            _cachedGraph.Update(gameTime);

            if (_dirty)
                QueueBuild();
        }

        public void Draw(GameTime gameTime)
        {
            if (!_destroyed && _cachedGraph.Visible)
                _cachedGraph.Draw(gameTime);
        }

        public void Invalidate(bool hideUntilRebuilt = false)
        {
            if (_destroyed)
                return;

            _revision++;
            _dirty = true;

            if (hideUntilRebuilt)
                _cachedGraph.Visible = false;
        }

        public void Destroy()
        {
            if (_destroyed)
                return;

            _destroyed = true;
            _revision++;
            _buildQueued = false;
            _cachedGraph.Destroy();

            if (_renderTarget != null && !_renderTarget.IsDisposed)
                _renderTarget.Dispose();

            _renderTarget = null;
        }

        private void ObserveCacheInputs()
        {
            var selectedGroupId = _playfield.ActionManager.EditScreen.SelectedScrollGroupId;
            var colorByTimingGroup = ConfigManager.EditorColorSvLineByTimingGroup.Value;
            var trackSpeed = _playfield.TrackSpeed;
            var playfieldWidth = _playfield.Width;
            var playfieldHeight = _playfield.Height;
            var screenScale = WindowManager.ScreenScale;

            if (_observedSelectedGroupId == selectedGroupId &&
                _observedColorByTimingGroup == colorByTimingGroup &&
                _observedTrackSpeed == trackSpeed &&
                _observedPlayfieldWidth == playfieldWidth &&
                _observedPlayfieldHeight == playfieldHeight &&
                _observedScreenScale == screenScale)
                return;

            _observedSelectedGroupId = selectedGroupId;
            _observedColorByTimingGroup = colorByTimingGroup;
            _observedTrackSpeed = trackSpeed;
            _observedPlayfieldWidth = playfieldWidth;
            _observedPlayfieldHeight = playfieldHeight;
            _observedScreenScale = screenScale;
            _cacheBoundsValid = false;
            Invalidate(true);
        }

        private void QueueBuild()
        {
            if (_buildQueued || _destroyed)
                return;

            _buildQueued = true;
            var revision = _revision;

            GameBase.Game.ScheduleRenderTargetDraw(() =>
            {
                _buildQueued = false;

                if (_destroyed || revision != _revision)
                    return;

                try
                {
                    BuildCache();

                    if (_destroyed || revision != _revision)
                        return;

                    _dirty = false;
                    _cachedGraph.Visible = true;
                }
                catch (Exception e)
                {
                    _dirty = true;
                    _cachedGraph.Visible = false;
                    Logger.Error(e, LogType.Runtime);
                }
            });
        }

        private void BuildCache()
        {
            var trackSpeed = Math.Max(_playfield.TrackSpeed, float.Epsilon);
            var (visibleStart, visibleEnd) = GetVisibleTimeRange();
            var visibleDuration = Math.Max(visibleEnd - visibleStart, double.Epsilon);
            var cacheStart = visibleStart - visibleDuration;
            var cacheEnd = visibleEnd + visibleDuration;
            var cacheWidth = MaximumLineWidth * 2 + _playfield.Width + SvRightPadding;
            var cacheHeight = (float)((cacheEnd - cacheStart) * trackSpeed);
            var lines = CreateLineSnapshot(cacheStart, cacheEnd);

            EnsureRenderTarget(cacheWidth, cacheHeight);

            var graphicsDevice = GameBase.Game.GraphicsDevice;
            var previousTargets = graphicsDevice.GetRenderTargets();

            try
            {
                _ = GameBase.Game.TryEndBatch();
                graphicsDevice.SetRenderTarget(_renderTarget);
                graphicsDevice.Clear(Color.Transparent);

                GameBase.Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied,
                    SamplerState.PointClamp, null, RasterizerState.CullNone);

                DrawLines(lines, cacheEnd, trackSpeed, cacheWidth, cacheHeight);
                _ = GameBase.Game.TryEndBatch();
            }
            finally
            {
                _ = GameBase.Game.TryEndBatch();
                graphicsDevice.SetRenderTargets(previousTargets);
            }

            _cacheStartTime = cacheStart;
            _cacheEndTime = cacheEnd;
            _cacheWidth = cacheWidth;
            _cacheHeight = cacheHeight;
            _cacheBoundsValid = true;
            _cachedGraph.Image = _renderTarget;
            _cachedGraph.Size = new ScalableVector2(cacheWidth, cacheHeight);
            UpdateCachedGraphPosition();
        }

        private List<GraphLine> CreateLineSnapshot(double cacheStart, double cacheEnd)
        {
            var lines = new List<GraphLine>();
            var selectedGroup = _playfield.ActionManager.EditScreen.SelectedScrollGroup;
            var globalGroup = _map.GlobalScrollGroup;

            AddGroupLines(lines, selectedGroup, cacheStart, cacheEnd);

            if (!ReferenceEquals(selectedGroup, globalGroup))
                AddGroupLines(lines, globalGroup, cacheStart, cacheEnd);

            lines.Sort((left, right) => left.StartTime.CompareTo(right.StartTime));
            return lines;
        }

        private static void AddGroupLines(List<GraphLine> lines, ScrollGroup group, double cacheStart,
            double cacheEnd)
        {
            if (group == null)
                return;

            var color = ConfigManager.EditorColorSvLineByTimingGroup.Value
                ? ColorHelper.ToXnaColor(group.GetColor())
                : ColorHelper.HexToColor("#56FE6E");

            var firstSv = FindFirstIndex(group.ScrollVelocities, cacheStart, point => point.StartTime);
            for (var i = firstSv; i < group.ScrollVelocities.Count; i++)
            {
                var point = group.ScrollVelocities[i];

                if (point.StartTime > cacheEnd)
                    break;

                lines.Add(new GraphLine(point.StartTime, point.Multiplier, true, color));
            }

            var firstSf = FindFirstIndex(group.ScrollSpeedFactors, cacheStart, point => point.StartTime);
            for (var i = firstSf; i < group.ScrollSpeedFactors.Count; i++)
            {
                var point = group.ScrollSpeedFactors[i];

                if (point.StartTime > cacheEnd)
                    break;

                lines.Add(new GraphLine(point.StartTime, point.Multiplier, false, color));
            }
        }

        private void DrawLines(IReadOnlyList<GraphLine> lines, double cacheEnd, float trackSpeed,
            float cacheWidth, float cacheHeight)
        {
            var scaleX = _renderTarget.Width / cacheWidth;
            var scaleY = _renderTarget.Height / cacheHeight;

            foreach (var line in lines)
            {
                var width = MathHelper.Clamp(Math.Abs(line.Multiplier) * DefaultLineWidth,
                    MinimumLineWidth, MaximumLineWidth);
                var x = line.IsScrollVelocity
                    ? MaximumLineWidth + _playfield.Width + SvRightPadding
                    : MaximumLineWidth - width;
                var y = (float)((cacheEnd - line.StartTime) * trackSpeed) -
                        (line.IsScrollVelocity ? LineHeight / 2 : LineHeight * 1.5f);
                var destination = ToRenderTargetRectangle(x, y, width, LineHeight, scaleX, scaleY);

                if (destination.Width > 0 && destination.Height > 0)
                    GameBase.Game.SpriteBatch.Draw(UserInterface.BlankBox, destination, line.Color);
            }
        }

        private Rectangle ToRenderTargetRectangle(float x, float y, float width, float height, float scaleX,
            float scaleY)
        {
            var left = Math.Clamp((int)Math.Floor(x * scaleX), 0, _renderTarget.Width);
            var top = Math.Clamp((int)Math.Floor(y * scaleY), 0, _renderTarget.Height);
            var right = Math.Clamp((int)Math.Ceiling((x + width) * scaleX), 0, _renderTarget.Width);
            var bottom = Math.Clamp((int)Math.Ceiling((y + height) * scaleY), 0, _renderTarget.Height);
            return new Rectangle(left, top, right - left, bottom - top);
        }

        private void EnsureRenderTarget(float cacheWidth, float cacheHeight)
        {
            var width = Math.Max(1, (int)Math.Ceiling(cacheWidth * WindowManager.ScreenScale.X));
            var height = Math.Max(1, (int)Math.Ceiling(cacheHeight * WindowManager.ScreenScale.Y));

            if (_renderTarget != null && !_renderTarget.IsDisposed && !_renderTarget.IsContentLost &&
                _renderTarget.GraphicsDevice == GameBase.Game.GraphicsDevice &&
                _renderTarget.Width == width && _renderTarget.Height == height)
                return;

            _cachedGraph.Visible = false;

            if (_renderTarget != null && !_renderTarget.IsDisposed)
                _renderTarget.Dispose();

            _renderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, width, height, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        private bool RenderTargetIsValid() =>
            _renderTarget != null && !_renderTarget.IsDisposed && !_renderTarget.IsContentLost &&
            _renderTarget.GraphicsDevice == GameBase.Game.GraphicsDevice;

        private void UpdateCachedGraphPosition()
        {
            if (!_cacheBoundsValid)
                return;

            _cachedGraph.Position = new ScalableVector2(
                _playfield.AbsolutePosition.X - MaximumLineWidth,
                _playfield.HitPositionY - (float)(_cacheEndTime * _playfield.TrackSpeed));

            if (_cachedGraph.Width != _cacheWidth || _cachedGraph.Height != _cacheHeight)
                _cachedGraph.Size = new ScalableVector2(_cacheWidth, _cacheHeight);
        }

        private (double Start, double End) GetVisibleTimeRange()
        {
            var trackSpeed = Math.Max(_playfield.TrackSpeed, float.Epsilon);
            var currentTime = _playfield.Track.Time;
            var pastWindow = Math.Max(0, _playfield.Height - _playfield.HitPositionY) / trackSpeed;
            var futureWindow = Math.Max(0, _playfield.HitPositionY) / trackSpeed;
            return (currentTime - pastWindow, currentTime + futureWindow);
        }

        private static int FindFirstIndex<T>(IReadOnlyList<T> points, double startTime, Func<T, float> getTime)
        {
            var low = 0;
            var high = points.Count;

            while (low < high)
            {
                var middle = low + (high - low) / 2;

                if (getTime(points[middle]) < startTime)
                    low = middle + 1;
                else
                    high = middle;
            }

            return low;
        }

        private readonly record struct GraphLine(float StartTime, float Multiplier, bool IsScrollVelocity,
            Color Color);
    }
}
