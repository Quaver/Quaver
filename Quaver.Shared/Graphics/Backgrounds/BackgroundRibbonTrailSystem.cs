using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Backgrounds
{
    /// <summary>
    ///     Renders slow emitters with fading ribbon trails.
    /// </summary>
    public class BackgroundRibbonTrailSystem : Sprite
    {
        private struct Emitter
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Angle;
            public float AngularVelocity;
            public float SegmentSpacingAccumulator;
            public Color Color;
        }

        // Legacy particles = 315. Ribbon target: roughly half visible points (~157).
        private const int EmitterCount = 56;
        private const int MaxTrailPoints = 10;
        private const float SegmentSpacing = 10f;
        private const float MinSpeed = 14f;
        private const float MaxSpeed = 34f;
        private const float WrapMargin = 60f;
        private const float BaseSize = 4.5f;

        private readonly Random _random = new Random();
        private readonly Emitter[] _emitters = new Emitter[EmitterCount];
        private readonly Vector2[,] _trailPoints = new Vector2[EmitterCount, MaxTrailPoints];
        private readonly int[] _trailCounts = new int[EmitterCount];
        private readonly Color _primaryColor;
        private readonly Color _secondaryColor;
        private Texture2D _glowTexture;
        private Rectangle _drawRect;

        public BackgroundRibbonTrailSystem(Color primaryColor, Color secondaryColor)
        {
            _primaryColor = primaryColor;
            _secondaryColor = secondaryColor;
            Alpha = 0f;
            Size = new Wobble.Graphics.ScalableVector2(WindowManager.VirtualScreen.X, WindowManager.VirtualScreen.Y);
            CreateGlowTexture();
            InitializeEmitters();
        }

        private void CreateGlowTexture()
        {
            const int size = 32;
            _glowTexture = new Texture2D(GameBase.Game.GraphicsDevice, size, size);
            var data = new Color[size * size];

            var center = size / 2f;
            var radius = size / 2f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var d = (float)Math.Sqrt(dx * dx + dy * dy);
                    var t = MathHelper.Clamp(d / radius, 0f, 1f);
                    var a = 1f - t * t;
                    data[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }

            _glowTexture.SetData(data);
        }

        private void InitializeEmitters()
        {
            var w = WindowManager.VirtualScreen.X;
            var h = WindowManager.VirtualScreen.Y;

            for (var i = 0; i < _emitters.Length; i++)
            {
                var angle = (float)(_random.NextDouble() * Math.PI * 2);
                var speed = MinSpeed + (float)_random.NextDouble() * (MaxSpeed - MinSpeed);
                _emitters[i] = new Emitter
                {
                    Position = new Vector2((float)_random.NextDouble() * w, (float)_random.NextDouble() * h),
                    Angle = angle,
                    AngularVelocity = (float)(_random.NextDouble() - 0.5f) * 0.2f,
                    Velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed),
                    SegmentSpacingAccumulator = 0f,
                    Color = GetBalancedColor(i)
                };

                _trailCounts[i] = 1;
                _trailPoints[i, 0] = _emitters[i].Position;
            }
        }

        private Color GetBalancedColor(int emitterIndex)
        {
            return emitterIndex % 2 == 0 ? _primaryColor : _secondaryColor;
        }

        private void PushTrailPoint(int emitterIndex, Vector2 pos)
        {
            var count = _trailCounts[emitterIndex];

            if (count < MaxTrailPoints)
            {
                for (var i = count; i > 0; i--)
                    _trailPoints[emitterIndex, i] = _trailPoints[emitterIndex, i - 1];
                _trailPoints[emitterIndex, 0] = pos;
                _trailCounts[emitterIndex]++;
                return;
            }

            for (var i = MaxTrailPoints - 1; i > 0; i--)
                _trailPoints[emitterIndex, i] = _trailPoints[emitterIndex, i - 1];
            _trailPoints[emitterIndex, 0] = pos;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible)
            {
                base.Update(gameTime);
                return;
            }

            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var w = WindowManager.VirtualScreen.X;
            var h = WindowManager.VirtualScreen.Y;

            for (var i = 0; i < _emitters.Length; i++)
            {
                var e = _emitters[i];

                e.Angle += e.AngularVelocity * dt;
                var speed = e.Velocity.Length();
                e.Velocity = new Vector2((float)Math.Cos(e.Angle) * speed, (float)Math.Sin(e.Angle) * speed);
                e.Position += e.Velocity * dt;

                if (e.Position.X > w + WrapMargin) e.Position.X = -WrapMargin;
                else if (e.Position.X < -WrapMargin) e.Position.X = w + WrapMargin;
                if (e.Position.Y > h + WrapMargin) e.Position.Y = -WrapMargin;
                else if (e.Position.Y < -WrapMargin) e.Position.Y = h + WrapMargin;

                e.SegmentSpacingAccumulator += e.Velocity.Length() * dt;
                if (e.SegmentSpacingAccumulator >= SegmentSpacing)
                {
                    e.SegmentSpacingAccumulator = 0f;
                    PushTrailPoint(i, e.Position);
                }
                else
                {
                    _trailPoints[i, 0] = e.Position;
                }

                _emitters[i] = e;
            }

            base.Update(gameTime);
        }

        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            var sb = GameBase.Game.SpriteBatch;

            for (var e = 0; e < _emitters.Length; e++)
            {
                var count = _trailCounts[e];
                var color = _emitters[e].Color;

                for (var i = 0; i < count; i++)
                {
                    var t = count <= 1 ? 0f : i / (float)(count - 1);
                    var alpha = MathHelper.Lerp(1f, 0.04f, t);
                    var size = MathHelper.Lerp(BaseSize, 1.8f, t);
                    if (alpha <= 0.01f)
                        continue;

                    var px = _trailPoints[e, i];
                    var s = (int)Math.Ceiling(size);
                    var half = s / 2;
                    _drawRect.X = (int)px.X - half;
                    _drawRect.Y = (int)px.Y - half;
                    _drawRect.Width = s;
                    _drawRect.Height = s;
                    sb.Draw(_glowTexture, _drawRect, color * alpha);
                }
            }
        }

        public override void Destroy()
        {
            _glowTexture?.Dispose();
            base.Destroy();
        }
    }
}
