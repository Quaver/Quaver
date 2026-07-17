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
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    /// <summary>
    ///     Flattens the visible timing, SV, and SF graphs into a compact buffered atlas.
    /// </summary>
    internal sealed class EditorPlayfieldScrollGraphCache
    {
        private const float DefaultLineWidth = 40;
        private const float MinimumLineWidth = 10;
        private const float MaximumLineWidth = 150;
        private const float ScrollLineHeight = 2;
        private const float TimingLineHeight = 4;
        private const float SvRightPadding = 2;

        private readonly EditorPlayfield _playfield;
        private readonly Qua _map;

        private RenderTarget2D _renderTarget;
        private double _cacheStartTime;
        private double _cacheEndTime;
        private float _cacheHeight;
        private int _stripPixelWidth;
        private int _revision;
        private bool _cacheBoundsValid;
        private bool _visible;
        private bool _dirty = true;
        private bool _buildQueued;
        private bool _destroyed;

        private string _observedSelectedGroupId;
        private bool _observedColorByTimingGroup;
        private float _observedTrackSpeed;
        private float _observedPlayfieldHeight;
        private Vector2 _observedScreenScale;

        public EditorPlayfieldScrollGraphCache(EditorPlayfield playfield, Qua map)
        {
            _playfield = playfield;
            _map = map;
            _observedSelectedGroupId = playfield.ActionManager.EditScreen.SelectedScrollGroupId;
            _observedColorByTimingGroup = ConfigManager.EditorColorSvLineByTimingGroup.Value;
            _observedTrackSpeed = playfield.TrackSpeed;
            _observedPlayfieldHeight = playfield.Height;
            _observedScreenScale = WindowManager.ScreenScale;
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
                _visible = false;

                if (!_dirty)
                    Invalidate();
            }

            if (!RenderTargetIsValid())
            {
                _visible = false;

                if (!_dirty)
                    Invalidate();
            }

            if (_dirty)
                QueueBuild();
        }

        /// <summary>
        ///     Draws only the visible vertical section from each narrow atlas strip.
        /// </summary>
        public void Draw(GameTime gameTime)
        {
            if (_destroyed || !_visible || !RenderTargetIsValid() || !_cacheBoundsValid)
                return;

            var trackSpeed = _playfield.TrackSpeed;
            var cacheTop = _playfield.HitPositionY - (float)(_cacheEndTime * trackSpeed);
            var cacheBottom = cacheTop + _cacheHeight;
            var visibleTop = -_playfield.TrackPositionY;
            var visibleBottom = visibleTop + _playfield.Height;
            var drawTop = Math.Max(cacheTop, visibleTop);
            var drawBottom = Math.Min(cacheBottom, visibleBottom);

            if (drawBottom <= drawTop)
                return;

            var scaleY = _renderTarget.Height / _cacheHeight;
            var sourceTop = Math.Clamp((int)Math.Floor((drawTop - cacheTop) * scaleY), 0,
                _renderTarget.Height);
            var sourceBottom = Math.Clamp((int)Math.Ceiling((drawBottom - cacheTop) * scaleY), sourceTop,
                _renderTarget.Height);
            var sourceHeight = sourceBottom - sourceTop;

            if (sourceHeight <= 0)
                return;

            var actualDrawTop = cacheTop + sourceTop / scaleY;
            var actualDrawHeight = sourceHeight / scaleY;
            var scale = new Vector2(MaximumLineWidth / _stripPixelWidth, actualDrawHeight / sourceHeight);
            var leftSource = new Rectangle(0, sourceTop, _stripPixelWidth, sourceHeight);
            var rightSource = new Rectangle(_stripPixelWidth, sourceTop, _stripPixelWidth, sourceHeight);

            GameBase.Game.SpriteBatch.Draw(_renderTarget,
                new Vector2(_playfield.AbsolutePosition.X - MaximumLineWidth, actualDrawTop), leftSource,
                Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            GameBase.Game.SpriteBatch.Draw(_renderTarget,
                new Vector2(_playfield.AbsolutePosition.X + _playfield.Width + SvRightPadding, actualDrawTop),
                rightSource, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        public void Invalidate(bool hideUntilRebuilt = false)
        {
            if (_destroyed)
                return;

            _revision++;
            _dirty = true;

            if (hideUntilRebuilt)
                _visible = false;
        }

        public void Destroy()
        {
            if (_destroyed)
                return;

            _destroyed = true;
            _revision++;
            _buildQueued = false;
            _visible = false;

            if (_renderTarget != null && !_renderTarget.IsDisposed)
                _renderTarget.Dispose();

            _renderTarget = null;
        }

        private void ObserveCacheInputs()
        {
            var selectedGroupId = _playfield.ActionManager.EditScreen.SelectedScrollGroupId;
            var colorByTimingGroup = ConfigManager.EditorColorSvLineByTimingGroup.Value;
            var trackSpeed = _playfield.TrackSpeed;
            var playfieldHeight = _playfield.Height;
            var screenScale = WindowManager.ScreenScale;

            if (_observedSelectedGroupId == selectedGroupId &&
                _observedColorByTimingGroup == colorByTimingGroup &&
                _observedTrackSpeed == trackSpeed &&
                _observedPlayfieldHeight == playfieldHeight &&
                _observedScreenScale == screenScale)
                return;

            _observedSelectedGroupId = selectedGroupId;
            _observedColorByTimingGroup = colorByTimingGroup;
            _observedTrackSpeed = trackSpeed;
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
                    _visible = true;
                }
                catch (Exception e)
                {
                    _dirty = true;
                    _visible = false;
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
            var cacheHeight = (float)((cacheEnd - cacheStart) * trackSpeed);
            var lines = CreateLineSnapshot(cacheStart, cacheEnd);

            EnsureRenderTarget(cacheHeight);

            var graphicsDevice = GameBase.Game.GraphicsDevice;
            var previousTargets = graphicsDevice.GetRenderTargets();

            try
            {
                _ = GameBase.Game.TryEndBatch();
                graphicsDevice.SetRenderTarget(_renderTarget);
                graphicsDevice.Clear(Color.Transparent);

                GameBase.Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied,
                    SamplerState.PointClamp, null, RasterizerState.CullNone);

                DrawLines(lines, cacheEnd, trackSpeed, cacheHeight);
                _ = GameBase.Game.TryEndBatch();
            }
            finally
            {
                _ = GameBase.Game.TryEndBatch();
                graphicsDevice.SetRenderTargets(previousTargets);
            }

            _cacheStartTime = cacheStart;
            _cacheEndTime = cacheEnd;
            _cacheHeight = cacheHeight;
            _cacheBoundsValid = true;
        }

        private List<GraphLine> CreateLineSnapshot(double cacheStart, double cacheEnd)
        {
            var lines = new List<GraphLine>();
            var selectedGroup = _playfield.ActionManager.EditScreen.SelectedScrollGroup;
            var globalGroup = _map.GlobalScrollGroup;

            AddGroupLines(lines, selectedGroup, cacheStart, cacheEnd);

            if (!ReferenceEquals(selectedGroup, globalGroup))
                AddGroupLines(lines, globalGroup, cacheStart, cacheEnd);

            var timingColor = ColorHelper.HexToColor("#FE5656");
            var firstTimingPoint = FindFirstIndex(_map.TimingPoints, cacheStart, point => point.StartTime);
            for (var i = firstTimingPoint; i < _map.TimingPoints.Count; i++)
            {
                var point = _map.TimingPoints[i];

                if (point.StartTime > cacheEnd)
                    break;

                lines.Add(new GraphLine(point.StartTime, DefaultLineWidth, TimingLineHeight,
                    GraphLineType.TimingPoint, timingColor, lines.Count));
            }

            lines.Sort((left, right) =>
            {
                var layerComparison = GetDrawLayer(left.Type).CompareTo(GetDrawLayer(right.Type));

                if (layerComparison != 0)
                    return layerComparison;

                var timeComparison = left.StartTime.CompareTo(right.StartTime);
                return timeComparison != 0 ? timeComparison : left.DrawOrder.CompareTo(right.DrawOrder);
            });
            return lines;
        }

        private static int GetDrawLayer(GraphLineType type) => type == GraphLineType.TimingPoint ? 1 : 0;

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

                var width = MathHelper.Clamp(Math.Abs(point.Multiplier) * DefaultLineWidth,
                    MinimumLineWidth, MaximumLineWidth);
                lines.Add(new GraphLine(point.StartTime, width, ScrollLineHeight, GraphLineType.ScrollVelocity,
                    color, lines.Count));
            }

            var firstSf = FindFirstIndex(group.ScrollSpeedFactors, cacheStart, point => point.StartTime);
            for (var i = firstSf; i < group.ScrollSpeedFactors.Count; i++)
            {
                var point = group.ScrollSpeedFactors[i];

                if (point.StartTime > cacheEnd)
                    break;

                var width = MathHelper.Clamp(Math.Abs(point.Multiplier) * DefaultLineWidth,
                    MinimumLineWidth, MaximumLineWidth);
                lines.Add(new GraphLine(point.StartTime, width, ScrollLineHeight,
                    GraphLineType.ScrollSpeedFactor, color, lines.Count));
            }
        }

        private void DrawLines(IReadOnlyList<GraphLine> lines, double cacheEnd, float trackSpeed,
            float cacheHeight)
        {
            var scaleX = _stripPixelWidth / MaximumLineWidth;
            var scaleY = _renderTarget.Height / cacheHeight;

            foreach (var line in lines)
            {
                var x = line.Type switch
                {
                    GraphLineType.ScrollSpeedFactor => MaximumLineWidth - line.Width,
                    _ => MaximumLineWidth
                };
                var verticalOffset = line.Type switch
                {
                    GraphLineType.ScrollVelocity => ScrollLineHeight / 2,
                    GraphLineType.ScrollSpeedFactor => ScrollLineHeight * 1.5f,
                    _ => TimingLineHeight / 2
                };
                var y = (float)((cacheEnd - line.StartTime) * trackSpeed) - verticalOffset;
                var destination = ToRenderTargetRectangle(x, y, line.Width, line.Height, scaleX, scaleY);

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

        private void EnsureRenderTarget(float cacheHeight)
        {
            var stripWidth = Math.Max(1,
                (int)Math.Ceiling(MaximumLineWidth * WindowManager.ScreenScale.X));
            var width = stripWidth * 2;
            var height = Math.Max(1, (int)Math.Ceiling(cacheHeight * WindowManager.ScreenScale.Y));

            if (_renderTarget != null && !_renderTarget.IsDisposed && !_renderTarget.IsContentLost &&
                _renderTarget.GraphicsDevice == GameBase.Game.GraphicsDevice &&
                _renderTarget.Width == width && _renderTarget.Height == height)
            {
                _stripPixelWidth = stripWidth;
                return;
            }

            _visible = false;

            if (_renderTarget != null && !_renderTarget.IsDisposed)
                _renderTarget.Dispose();

            _renderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, width, height, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _stripPixelWidth = stripWidth;
        }

        private bool RenderTargetIsValid() =>
            _renderTarget != null && !_renderTarget.IsDisposed && !_renderTarget.IsContentLost &&
            _renderTarget.GraphicsDevice == GameBase.Game.GraphicsDevice;

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

        private enum GraphLineType
        {
            ScrollVelocity,
            ScrollSpeedFactor,
            TimingPoint
        }

        private readonly record struct GraphLine(float StartTime, float Width, float Height, GraphLineType Type,
            Color Color, int DrawOrder);
    }
}
