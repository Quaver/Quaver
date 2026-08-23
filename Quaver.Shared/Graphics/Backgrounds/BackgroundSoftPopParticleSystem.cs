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
    ///     Spawns soft particles at random locations that fade out over a few seconds.
    /// </summary>
    public class BackgroundSoftPopParticleSystem : Sprite
    {
        private struct Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Size;
            public float BaseAlpha;
            public Color BaseColor;
            public float Lifetime;
            public float Age;
        }

        private const int MaxParticles = 220;
        private const float SpawnRatePerSecond = 20f;
        private const float MinLifetime = 6.0f;
        private const float MaxLifetime = 10.0f;
        private const float MinSize = 2f;
        private const float MaxSize = 5f;
        private const float MaxDriftSpeed = 3.5f;
        private const float FlashDuration = 2.0f;
        private const float FlashSizeMultiplier = 2.0f;

        private readonly Random _random = new Random();
        private readonly Particle[] _particles = new Particle[MaxParticles];
        private readonly Color _primaryColor;
        private readonly Color _secondaryColor;
        private int _aliveCount;
        private float _spawnAccumulator;
        private Texture2D _glowTexture;
        private Rectangle _drawRect;

        public BackgroundSoftPopParticleSystem(Color primaryColor, Color secondaryColor)
        {
            _primaryColor = primaryColor;
            _secondaryColor = secondaryColor;
            Alpha = 0f;
            Size = new Wobble.Graphics.ScalableVector2(WindowManager.VirtualScreen.X, WindowManager.VirtualScreen.Y);
            CreateGlowTexture();
        }

        private void CreateGlowTexture()
        {
            const int textureSize = 32;
            _glowTexture = new Texture2D(GameBase.Game.GraphicsDevice, textureSize, textureSize);
            var data = new Color[textureSize * textureSize];

            var center = textureSize / 2f;
            var maxRadius = textureSize / 2f;

            for (var y = 0; y < textureSize; y++)
            {
                for (var x = 0; x < textureSize; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    var t = MathHelper.Clamp(distance / maxRadius, 0f, 1f);
                    var alpha = 1f - t * t;
                    data[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            _glowTexture.SetData(data);
        }

        private void SpawnParticle()
        {
            if (_aliveCount >= _particles.Length)
                return;

            var width = WindowManager.VirtualScreen.X;
            var height = WindowManager.VirtualScreen.Y;
            var speed = (float)_random.NextDouble() * MaxDriftSpeed;
            var angle = (float)(_random.NextDouble() * Math.PI * 2);

            _particles[_aliveCount++] = new Particle
            {
                Position = new Vector2((float)_random.NextDouble() * width, (float)_random.NextDouble() * height),
                Velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed),
                Size = MinSize + (float)_random.NextDouble() * (MaxSize - MinSize),
                BaseAlpha = 0.35f + (float)_random.NextDouble() * 0.55f,
                BaseColor = GetRandomParticleColor(),
                Lifetime = MinLifetime + (float)_random.NextDouble() * (MaxLifetime - MinLifetime),
                Age = 0f
            };
        }

        private Color GetRandomParticleColor()
        {
            return _random.NextDouble() < 0.5 ? _primaryColor : _secondaryColor;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible)
            {
                base.Update(gameTime);
                return;
            }

            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _spawnAccumulator += SpawnRatePerSecond * dt;
            while (_spawnAccumulator >= 1f)
            {
                _spawnAccumulator -= 1f;
                SpawnParticle();
            }

            for (var i = _aliveCount - 1; i >= 0; i--)
            {
                _particles[i].Age += dt;
                _particles[i].Position += _particles[i].Velocity * dt;

                if (_particles[i].Age < _particles[i].Lifetime)
                    continue;

                _particles[i] = _particles[_aliveCount - 1];
                _aliveCount--;
            }

            base.Update(gameTime);
        }

        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            var sb = GameBase.Game.SpriteBatch;

            for (var i = 0; i < _aliveCount; i++)
            {
                var p = _particles[i];
                float alpha;
                if (p.Age <= FlashDuration)
                {
                    // Strong spawn flash at full opacity, then quickly settles.
                    var t = MathHelper.Clamp(p.Age / FlashDuration, 0f, 1f);
                    alpha = MathHelper.Lerp(1f, p.BaseAlpha, t);
                }
                else
                {
                    var fadeProgress = MathHelper.Clamp((p.Age - FlashDuration) / (p.Lifetime - FlashDuration), 0f, 1f);
                    alpha = p.BaseAlpha * (1f - fadeProgress);
                }

                if (alpha <= 0.01f)
                    continue;

                if (p.Age <= FlashDuration)
                {
                    var flashT = MathHelper.Clamp(p.Age / FlashDuration, 0f, 1f);
                    var flashAlpha = 1f - flashT;
                    var flashSize = (int)Math.Ceiling(p.Size * FlashSizeMultiplier);
                    var flashHalf = flashSize / 2;
                    _drawRect.X = (int)p.Position.X - flashHalf;
                    _drawRect.Y = (int)p.Position.Y - flashHalf;
                    _drawRect.Width = flashSize;
                    _drawRect.Height = flashSize;
                    sb.Draw(_glowTexture, _drawRect, p.BaseColor * flashAlpha);
                }

                var size = (int)Math.Ceiling(p.Size);
                var half = size / 2;
                _drawRect.X = (int)p.Position.X - half;
                _drawRect.Y = (int)p.Position.Y - half;
                _drawRect.Width = size;
                _drawRect.Height = size;

                sb.Draw(_glowTexture, _drawRect, p.BaseColor * alpha);
            }
        }

        public override void Destroy()
        {
            _glowTexture?.Dispose();
            base.Destroy();
        }
    }
}
