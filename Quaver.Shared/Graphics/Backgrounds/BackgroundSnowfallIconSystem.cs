using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Backgrounds
{
    /// <summary>
    ///     Snowfall animation that uses the same three icon textures as Matrix.
    /// </summary>
    public class BackgroundSnowfallIconSystem : Sprite
    {
        private struct Flake
        {
            public float BaseX;
            public float X;
            public float Y;
            public float Speed;
            public float Scale;
            public float BaseOpacity;
            public int IconIndex;
            public Color Color;
            public float Age;
            public float Lifetime;
            public float FadeStart;
            public float WindAmplitude;
            public float WindFrequency;
            public float WindPhase;
            public float PixelSize;
            public float InvLifetime;
            public float InvFadeDuration;
            public byte Layer;
            public float Rotation;
            public float RotationSpeed;
        }

        private const int MaxFlakeCount = 220;
        private const int MinEffectiveFlakeCount = 120;
        private const int MaxEffectiveFlakeCount = 180;
        private const float TargetFlakeStep = 14f;
        private const float MinSpeed = 38f;
        private const float MaxSpeed = 102f;
        private const float MinScale = 0.40f;
        private const float MaxScale = 1.00f;
        private const float MinOpacity = 0.50f;
        private const float MaxOpacity = 0.90f;
        private const float MinLifetime = 12f;
        private const float MaxLifetime = 24f;
        private const float MinFadeStart = 0.80f;
        private const float MaxFadeStart = 0.94f;
        private const float MinWindAmplitude = 4f;
        private const float MaxWindAmplitude = 22f;
        private const float MinWindFrequency = 0.30f;
        private const float MaxWindFrequency = 0.90f;
        private const float WrapMargin = 80f;

        private readonly Random _random = new Random();
        private readonly Flake[] _flakes = new Flake[MaxFlakeCount];
        private readonly Color _primaryColor;
        private readonly Color _secondaryColor;
        private int _effectiveFlakeCount;
        private float _globalWindTime;

        private readonly Texture2D[] _icons;
        private float _iconBaseSize = 32f;
        private float _resolutionScale = 1f;

        public BackgroundSnowfallIconSystem(Texture2D[] icons, Color primaryColor, Color secondaryColor)
        {
            _icons = icons ?? throw new ArgumentNullException(nameof(icons));
            if (_icons.Length == 0)
                throw new ArgumentException("At least one background icon is required.", nameof(icons));

            _primaryColor = primaryColor;
            _secondaryColor = secondaryColor;
            Alpha = 0f;
            Size = new Wobble.Graphics.ScalableVector2(WindowManager.VirtualScreen.X, WindowManager.VirtualScreen.Y);
            if (_icons != null && _icons.Length > 0)
                _iconBaseSize = Math.Max(_icons[0].Width, _icons[0].Height);

            _resolutionScale = MathHelper.Clamp(WindowManager.VirtualScreen.Y / 1080f, 0.75f, 1.25f);

            InitializeFlakes();
        }

        private Color GetRandomColor()
        {
            return _random.NextDouble() < 0.5 ? _primaryColor : _secondaryColor;
        }

        private void InitializeFlakes()
        {
            var w = WindowManager.VirtualScreen.X;
            var h = WindowManager.VirtualScreen.Y;
            var minX = _iconBaseSize / 2f;
            var usableWidth = Math.Max(0f, w - _iconBaseSize);

            _effectiveFlakeCount = Math.Clamp((int)Math.Round(usableWidth / TargetFlakeStep) + 1,
                MinEffectiveFlakeCount, MaxEffectiveFlakeCount);
            _effectiveFlakeCount = Math.Min(_effectiveFlakeCount, MaxFlakeCount);

            for (var i = 0; i < _effectiveFlakeCount; i++)
            {
                var x = minX + ((i + (float)_random.NextDouble() * 0.33f) / _effectiveFlakeCount) * usableWidth;
                var y = (float)_random.NextDouble() * (h * 2.0f) - h * 0.85f;
                InitializeFlake(ref _flakes[i], x, y);
            }
        }

        private void InitializeFlake(ref Flake f, float x, float y)
        {
            var layerRoll = (float)_random.NextDouble();
            var layer = layerRoll < 0.46f ? (byte)0 : (layerRoll < 0.80f ? (byte)1 : (byte)2); // back/mid/front
            f.Layer = layer;

            var speedMul = layer == 0 ? 0.78f : (layer == 1 ? 1.0f : 1.24f);
            var sizeMul = layer == 0 ? 0.72f : (layer == 1 ? 1.0f : 1.22f);
            var alphaMul = layer == 0 ? 0.68f : (layer == 1 ? 0.88f : 1.0f);

            f.BaseX = x;
            f.X = x;
            f.Y = y;
            f.Speed = (MinSpeed + (float)_random.NextDouble() * (MaxSpeed - MinSpeed)) * speedMul;
            f.Scale = MinScale + (float)_random.NextDouble() * (MaxScale - MinScale);
            var maxPixelSize = Math.Min(35f, 35f * _resolutionScale);
            var minPixelSize = MathHelper.Clamp(10f * _resolutionScale, 8f, maxPixelSize * 0.65f);
            f.PixelSize = (minPixelSize + (float)_random.NextDouble() * (maxPixelSize - minPixelSize)) * sizeMul;
            if (f.PixelSize > 35f)
                f.PixelSize = 35f;

            var sizeT = (f.PixelSize - minPixelSize) / Math.Max(maxPixelSize - minPixelSize, 0.0001f);
            var maxAlphaForSize = MathHelper.Lerp(MaxOpacity, MaxOpacity * 0.65f, MathHelper.Clamp(sizeT, 0f, 1f));
            var minAlphaForSize = MinOpacity * alphaMul;
            f.BaseOpacity = minAlphaForSize + (float)_random.NextDouble() * Math.Max(maxAlphaForSize - minAlphaForSize, 0.02f);
            f.IconIndex = _random.Next(_icons.Length);
            f.Color = GetRandomColor();
            f.Age = (float)_random.NextDouble() * 1.5f;
            f.Lifetime = MinLifetime + (float)_random.NextDouble() * (MaxLifetime - MinLifetime);
            f.FadeStart = MinFadeStart + (float)_random.NextDouble() * (MaxFadeStart - MinFadeStart);
            f.InvLifetime = 1f / Math.Max(f.Lifetime, 0.0001f);
            f.InvFadeDuration = 1f / Math.Max(1f - f.FadeStart, 0.0001f);

            var windMul = layer == 0 ? 0.75f : (layer == 1 ? 1.0f : 1.2f);
            f.WindAmplitude = (MinWindAmplitude + (float)_random.NextDouble() * (MaxWindAmplitude - MinWindAmplitude)) * windMul;
            f.WindFrequency = MinWindFrequency + (float)_random.NextDouble() * (MaxWindFrequency - MinWindFrequency);
            f.WindPhase = (float)_random.NextDouble() * MathHelper.TwoPi;
            f.Rotation = (float)_random.NextDouble() * MathHelper.TwoPi;
            var rotMul = layer == 0 ? 0.55f : (layer == 1 ? 0.8f : 1.0f);
            var rotDir = _random.NextDouble() < 0.5 ? -1f : 1f;
            f.RotationSpeed = rotDir * (0.08f + (float)_random.NextDouble() * 0.34f) * rotMul;
        }

        private void RespawnFlake(ref Flake f, float width, float height)
        {
            var minX = _iconBaseSize / 2f;
            var usableWidth = Math.Max(0f, width - _iconBaseSize);
            var x = minX + (float)_random.NextDouble() * usableWidth;
            var y = -WrapMargin - (float)_random.NextDouble() * (height * 0.35f);
            InitializeFlake(ref f, x, y);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible)
            {
                base.Update(gameTime);
                return;
            }

            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f)
            {
                base.Update(gameTime);
                return;
            }

            dt = Math.Min(dt, 1f / 20f);
            var h = WindowManager.VirtualScreen.Y;
            var w = WindowManager.VirtualScreen.X;
            _globalWindTime += dt;
            var globalWindPhase = _globalWindTime * 0.30f;

            for (var i = 0; i < _effectiveFlakeCount; i++)
            {
                ref var f = ref _flakes[i];
                f.Age += dt;
                f.Y += f.Speed * dt;
                f.WindPhase += f.WindFrequency * dt;

                var localWind = (float)Math.Sin(f.WindPhase) * f.WindAmplitude;
                var globalWind = (float)Math.Sin(globalWindPhase + i * 0.17f) * 2.1f;
                f.X = f.BaseX + localWind + globalWind;
                f.Rotation += f.RotationSpeed * dt;

                var flakeSize = f.PixelSize;
                var shouldExpireByAge = f.Age >= f.Lifetime && f.Y > h * 0.52f;
                if (f.Y - flakeSize > h + WrapMargin || shouldExpireByAge)
                    RespawnFlake(ref f, w, h);
            }

            base.Update(gameTime);
        }

        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            if (_icons == null || _icons.Length == 0)
                return;

            var sb = GameBase.Game.SpriteBatch;
            var h = WindowManager.VirtualScreen.Y;
            var w = WindowManager.VirtualScreen.X;
            var topCull = -_iconBaseSize - WrapMargin;
            var bottomCull = h + _iconBaseSize + WrapMargin;

            for (var i = 0; i < _effectiveFlakeCount; i++)
            {
                ref var f = ref _flakes[i];

                if (f.Y < topCull || f.Y > bottomCull)
                    continue;

                var lifeProgress = MathHelper.Clamp(f.Age * f.InvLifetime, 0f, 1f);
                var lifeFade = lifeProgress <= f.FadeStart
                    ? 1f
                    : 1f - (lifeProgress - f.FadeStart) * f.InvFadeDuration;
                lifeFade = MathHelper.Clamp(lifeFade, 0f, 1f);

                var bottomFadeStart = h * 0.94f;
                var bottomFade = f.Y <= bottomFadeStart
                    ? 1f
                    : 1f - MathHelper.Clamp((f.Y - bottomFadeStart) / (h * 0.12f), 0f, 1f);

                var alpha = f.BaseOpacity * lifeFade * bottomFade;
                if (alpha <= 0.02f)
                    continue;

                var drawSize = Math.Min(35f, f.PixelSize);
                if (f.X < -drawSize || f.X > w + drawSize)
                    continue;
                
                var texture = _icons[f.IconIndex];
                var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                var scale = drawSize / Math.Max(texture.Width, texture.Height);
                var position = new Vector2(f.X, f.Y);
                sb.Draw(texture, position, null, f.Color * alpha, f.Rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}
