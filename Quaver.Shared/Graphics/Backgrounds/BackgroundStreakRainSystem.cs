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
    ///     Renders thin angled streaks drifting across screen.
    /// </summary>
    public class BackgroundStreakRainSystem : Sprite
    {
        private struct Streak
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float BaseAngle;
            public float WavePhase;
            public float WaveAmplitudeDeg;
            public float Length;
            public float Thickness;
            public float Alpha;
            public Color Color;
        }

        private const int StreakCount = 120;
        private const float MinSpeed = 70f;
        private const float MaxSpeed = 150f;
        private const float MinLength = 26f;
        private const float MaxLength = 62f;
        private const float MinAlpha = 0.75f;
        private const float MaxAlpha = 1.0f;
        private const float WrapMargin = 80f;
        private const int StreakTextureWidth = 96;
        private const int StreakTextureHeight = 12;
        private const float DirectionWaveFrequency = 0.45f;

        private readonly Random _random = new Random();
        private readonly Streak[] _streaks = new Streak[StreakCount];
        private readonly Color _primaryColor;
        private readonly Color _secondaryColor;
        private Texture2D _streakTexture;
        private Rectangle _drawRect;

        public BackgroundStreakRainSystem(Color primaryColor, Color secondaryColor)
        {
            _primaryColor = primaryColor;
            _secondaryColor = secondaryColor;
            Alpha = 0f;
            Size = new Wobble.Graphics.ScalableVector2(WindowManager.VirtualScreen.X, WindowManager.VirtualScreen.Y);
            CreatePixelTexture();
            InitializeStreaks();
        }

        private void CreatePixelTexture()
        {
            _streakTexture = new Texture2D(GameBase.Game.GraphicsDevice, StreakTextureWidth, StreakTextureHeight);
            var data = new Color[StreakTextureWidth * StreakTextureHeight];

            var centerY = (StreakTextureHeight - 1) / 2f;
            var maxY = centerY <= 0 ? 1f : centerY;

            for (var y = 0; y < StreakTextureHeight; y++)
            {
                for (var x = 0; x < StreakTextureWidth; x++)
                {
                    var tx = x / (float)(StreakTextureWidth - 1);
                    var ty = Math.Abs(y - centerY) / maxY;

                    // Horizontal: full opacity at head -> transparent tail.
                    var horizontal = 1f - tx;
                    // Vertical: soft anti-aliased edges
                    var vertical = 1f - ty * ty;
                    var alpha = MathHelper.Clamp(horizontal * vertical, 0f, 1f);

                    data[y * StreakTextureWidth + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            _streakTexture.SetData(data);
        }

        private Color GetRandomColor()
        {
            return _random.NextDouble() < 0.5 ? _primaryColor : _secondaryColor;
        }

        private void InitializeStreaks()
        {
            var w = WindowManager.VirtualScreen.X;
            var h = WindowManager.VirtualScreen.Y;

            for (var i = 0; i < _streaks.Length; i++)
            {
                _streaks[i] = CreateStreak(
                    new Vector2((float)_random.NextDouble() * w, (float)_random.NextDouble() * h));
            }
        }

        private Streak CreateStreak(Vector2 startPos)
        {
            // Keep near rain feel but with broader spread.
            var angleDeg = 18f + (float)_random.NextDouble() * 26f;
            var angle = MathHelper.ToRadians(angleDeg);
            var speed = MinSpeed + (float)_random.NextDouble() * (MaxSpeed - MinSpeed);

            return new Streak
            {
                Position = startPos,
                Velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed),
                BaseAngle = angle,
                WavePhase = (float)(_random.NextDouble() * Math.PI * 2),
                WaveAmplitudeDeg = 5f + (float)_random.NextDouble() * 7f,
                Length = MinLength + (float)_random.NextDouble() * (MaxLength - MinLength),
                Thickness = _random.NextDouble() < 0.7 ? 2f : 3f,
                Alpha = MinAlpha + (float)_random.NextDouble() * (MaxAlpha - MinAlpha),
                Color = GetRandomColor()
            };
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
            var total = (float)gameTime.TotalGameTime.TotalSeconds;

            for (var i = 0; i < _streaks.Length; i++)
            {
                var speed = _streaks[i].Velocity.Length();
                var waveAngle = MathHelper.ToRadians(_streaks[i].WaveAmplitudeDeg) *
                                (float)Math.Sin(total * DirectionWaveFrequency + _streaks[i].WavePhase);
                var currentAngle = _streaks[i].BaseAngle + waveAngle;
                _streaks[i].Velocity = new Vector2((float)Math.Cos(currentAngle) * speed, (float)Math.Sin(currentAngle) * speed);
                _streaks[i].Position += _streaks[i].Velocity * dt;

                var p = _streaks[i].Position;
                if (p.X <= w + WrapMargin && p.Y <= h + WrapMargin)
                    continue;

                // Evenly spread respawn origin between top/left edges.
                var edge = _random.NextDouble();
                var start = edge < 0.5
                    ? new Vector2((float)_random.NextDouble() * (w + WrapMargin * 2) - WrapMargin, -WrapMargin)
                    : new Vector2(-WrapMargin, (float)_random.NextDouble() * (h + WrapMargin * 2) - WrapMargin);
                _streaks[i] = CreateStreak(start);
            }

            base.Update(gameTime);
        }

        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            var sb = GameBase.Game.SpriteBatch;

            for (var i = 0; i < _streaks.Length; i++)
            {
                var s = _streaks[i];
                var angle = (float)Math.Atan2(s.Velocity.Y, s.Velocity.X) + MathHelper.Pi;

                _drawRect.X = (int)s.Position.X;
                _drawRect.Y = (int)(s.Position.Y - s.Thickness / 2f);
                _drawRect.Width = (int)Math.Ceiling(s.Length);
                _drawRect.Height = (int)Math.Ceiling(s.Thickness);

                sb.Draw(_streakTexture, _drawRect, null, s.Color * s.Alpha, angle, Vector2.Zero, SpriteEffects.None, 0f);
            }
        }

        public override void Destroy()
        {
            _streakTexture?.Dispose();
            base.Destroy();
        }
    }
}
