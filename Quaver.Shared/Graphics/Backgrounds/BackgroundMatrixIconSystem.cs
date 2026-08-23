using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Backgrounds
{
    /// <summary>
    ///     Matrix-like icon rain using background animation icons.
    /// </summary>
    public class BackgroundMatrixIconSystem : Sprite
    {
        private struct Column
        {
            public float BaseX;
            public float X;
            public float Y;
            public float Speed;
            public float Opacity;
            public int TrailLength;
            public float Spacing;
            public int HeadIcon;
            public float IconChangeTimer;
            public float IconChangeInterval;
            public int PulseSegment;
            public float PulseTimeRemaining;
            public float PulseDuration;
            public float NextPulseTime;
            public Color Color;
            public bool WaitingForSafeRelease;
            public float PendingSpawnY;
        }

        private const int ColumnCount = 190;
        private const int ChainsPerLane = 2;
        private const int MinEffectiveColumnCount = 100;
        private const int MaxEffectiveColumnCount = 107;
        private const float TargetColumnStep = 18f;
        private const float MinSpeed = 40f;
        private const float MaxSpeed = 110f;
        private const float MinSpacing = 22f;
        private const float MaxSpacing = 26f;
        private const float WrapMargin = 120f;
        private const float VerticalSpreadScreens = 1.6f;
        private const int IconDrawSize = 16; // ~0.75x of previous size
        private const float MinIconChangeInterval = 0.11f;
        private const float MaxIconChangeInterval = 0.40f;
        private const float UpdateStep = 1f / 30f;
        private const float MaxUpdateDelta = 0.25f;
        private const int MaxTrailLength = 18;
        private const float MinPulseDelay = 0.35f;
        private const float MaxPulseDelay = 1.25f;
        private const float MinPulseDuration = 0.22f;
        private const float MaxPulseDuration = 0.42f;
        private const float GoldenRatioConjugate = 0.61803398875f;
        private const float ChainSafetyGap = 8f;

        private readonly Random _random = new Random();
        private readonly Column[] _columns = new Column[ColumnCount];
        private readonly Color _primaryColor;
        private readonly Color _secondaryColor;
        private float _updateAccumulator;
        private int _effectiveColumnCount;
        private readonly Texture2D[] _icons;
        private Texture2D _iconAtlas;
        private readonly Rectangle[] _iconSourceRects = new Rectangle[3];
        private bool _atlasReady;
        private readonly float[] _trailAlphas = new float[MaxTrailLength];
        private Rectangle _drawRect;

        public BackgroundMatrixIconSystem(Texture2D[] icons, Color primaryColor, Color secondaryColor)
        {
            _icons = icons ?? throw new ArgumentNullException(nameof(icons));
            if (_icons.Length == 0)
                throw new ArgumentException("At least one background icon is required.", nameof(icons));

            _primaryColor = primaryColor;
            _secondaryColor = secondaryColor;
            Alpha = 0f;
            Size = new Wobble.Graphics.ScalableVector2(WindowManager.VirtualScreen.X, WindowManager.VirtualScreen.Y);

            for (var i = 0; i < MaxTrailLength; i++)
            {
                var progress = i / (float)(MaxTrailLength - 1);
                _trailAlphas[i] = 1f - progress;
            }

            InitializeColumns();
        }

        private void EnsureAtlasBuilt()
        {
            if (_atlasReady || _icons == null || _icons.Length == 0)
                return;

            var cell = IconDrawSize;
            _iconAtlas = new Texture2D(GameBase.Game.GraphicsDevice, cell * _icons.Length, cell);
            var atlasData = new Color[cell * _icons.Length * cell];

            for (var iconIndex = 0; iconIndex < _icons.Length; iconIndex++)
            {
                var src = _icons[iconIndex];
                var srcData = new Color[src.Width * src.Height];
                src.GetData(srcData);

                for (var y = 0; y < cell; y++)
                {
                    for (var x = 0; x < cell; x++)
                    {
                        var sx = (int)(x / (float)cell * src.Width);
                        var sy = (int)(y / (float)cell * src.Height);
                        if (sx >= src.Width) sx = src.Width - 1;
                        if (sy >= src.Height) sy = src.Height - 1;

                        atlasData[y * (cell * _icons.Length) + iconIndex * cell + x] = srcData[sy * src.Width + sx];
                    }
                }

                _iconSourceRects[iconIndex] = new Rectangle(iconIndex * cell, 0, cell, cell);
            }

            _iconAtlas.SetData(atlasData);
            _atlasReady = true;
        }

        private Color GetRandomColor()
        {
            return _random.NextDouble() < 0.5 ? _primaryColor : _secondaryColor;
        }

        private static float Fract(float x) => x - (float)Math.Floor(x);

        private void InitializeColumns()
        {
            var w = WindowManager.VirtualScreen.X;
            var h = WindowManager.VirtualScreen.Y;
            var minX = IconDrawSize / 2f;
            var usableWidth = Math.Max(0f, w - IconDrawSize);
            var minColumnStep = IconDrawSize + 2f; // hard no-overlap in X
            var targetStep = Math.Max(TargetColumnStep, minColumnStep);
            var laneCount = Math.Clamp((int)Math.Round(usableWidth / targetStep) + 1, MinEffectiveColumnCount, MaxEffectiveColumnCount);
            var maxNoOverlapLanes = Math.Max(1, (int)Math.Floor(usableWidth / minColumnStep) + 1);
            laneCount = Math.Min(laneCount, maxNoOverlapLanes);
            laneCount = Math.Min(laneCount, ColumnCount / ChainsPerLane);

            _effectiveColumnCount = laneCount * ChainsPerLane;
            var step = laneCount <= 1 ? 0f : usableWidth / (laneCount - 1);

            for (var lane = 0; lane < laneCount; lane++)
            {
                var baseX = minX + lane * step;
                for (var chain = 0; chain < ChainsPerLane; chain++)
                {
                    var i = lane * ChainsPerLane + chain;
                    var speed = GetBandSpeed();
                    // Two chains per lane: deterministic phase shift keeps lane visually filled.
                    var yT = Fract(lane * GoldenRatioConjugate + chain * 0.5f);
                    var yJitter = ((float)_random.NextDouble() - 0.5f) * 0.35f;
                    yT = Fract(yT + yJitter);
                    var verticalSpread = h * VerticalSpreadScreens;

                    var trailLength = 14 + _random.Next(5); // 14..18
                    var spacing = MinSpacing + (float)_random.NextDouble() * (MaxSpacing - MinSpacing);
                    _columns[i] = new Column
                    {
                        BaseX = baseX,
                        X = baseX,
                        Y = yT * verticalSpread - verticalSpread * 0.5f,
                        Speed = speed,
                        Opacity = 0.50f + (float)_random.NextDouble() * 0.50f,
                        TrailLength = trailLength,
                        Spacing = spacing,
                        HeadIcon = _random.Next(_icons.Length),
                        IconChangeTimer = 0f,
                        IconChangeInterval = MinIconChangeInterval + (float)_random.NextDouble() * (MaxIconChangeInterval - MinIconChangeInterval),
                        PulseSegment = _random.Next(MaxTrailLength),
                        PulseTimeRemaining = 0f,
                        PulseDuration = MinPulseDuration + (float)_random.NextDouble() * (MaxPulseDuration - MinPulseDuration),
                        NextPulseTime = MinPulseDelay + (lane % 9) * 0.06f + (float)_random.NextDouble() * 0.12f,
                        Color = GetRandomColor(),
                        WaitingForSafeRelease = chain == 1,
                        PendingSpawnY = 0f
                    };

                    if (chain == 1)
                    {
                        ref var col = ref _columns[i];
                        col.PendingSpawnY = GetSpawnY(in col, h);
                        col.Y = -h * 4f;
                    }
                }
            }
        }

        private float GetBandSpeed()
        {
            // Continuous spread: more variation, fewer visible same-speed groups.
            var t = (float)_random.NextDouble();
            t = t * t * (3f - 2f * t); // smoothstep
            return MinSpeed + t * (MaxSpeed - MinSpeed);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible)
            {
                base.Update(gameTime);
                return;
            }

            // Keep the fixed-step simulation inexpensive, while preventing a long frame from
            // creating a large catch-up loop that can prolong a hitch.
            _updateAccumulator += Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, MaxUpdateDelta);
            var h = WindowManager.VirtualScreen.Y;

            while (_updateAccumulator >= UpdateStep)
            {
                _updateAccumulator -= UpdateStep;

                for (var i = 0; i < _effectiveColumnCount; i++)
                {
                    ref var c = ref _columns[i];
                    var chain = i % ChainsPerLane;
                    var lane = i / ChainsPerLane;

                    if (c.WaitingForSafeRelease)
                    {
                        var blockerIndex = lane * ChainsPerLane + (1 - chain);
                        ref var blocker = ref _columns[blockerIndex];

                        var canTryRelease = !blocker.WaitingForSafeRelease || chain == 0;
                        if (canTryRelease && CanReleaseChain(in blocker, in c, c.PendingSpawnY, h))
                        {
                            c.Y = c.PendingSpawnY;
                            c.WaitingForSafeRelease = false;
                        }
                        else
                        {
                            c.IconChangeTimer += UpdateStep;
                            c.NextPulseTime -= UpdateStep;
                            continue;
                        }
                    }

                    c.Y += c.Speed * UpdateStep;
                    c.IconChangeTimer += UpdateStep;
                    c.NextPulseTime -= UpdateStep;

                    if (c.IconChangeTimer >= c.IconChangeInterval)
                    {
                        c.IconChangeTimer = 0f;
                        c.IconChangeInterval = MinIconChangeInterval + (float)_random.NextDouble() * (MaxIconChangeInterval - MinIconChangeInterval);
                        c.HeadIcon = _random.Next(_icons.Length);
                    }

                    if (c.PulseTimeRemaining > 0f)
                    {
                        c.PulseTimeRemaining -= UpdateStep;
                    }
                    else if (c.NextPulseTime <= 0f)
                    {
                        c.PulseDuration = MinPulseDuration + (float)_random.NextDouble() * (MaxPulseDuration - MinPulseDuration);
                        c.PulseTimeRemaining = c.PulseDuration;
                        c.PulseSegment = _random.Next(c.TrailLength);
                        c.NextPulseTime = MinPulseDelay + (float)_random.NextDouble() * (MaxPulseDelay - MinPulseDelay);
                    }

                    if (c.Y - c.TrailLength * c.Spacing > h + WrapMargin)
                    {
                        c.WaitingForSafeRelease = true;
                        c.PendingSpawnY = GetSpawnY(in c, h);
                        c.Y = -h * 4f;

                        c.Speed = GetBandSpeed();
                        c.Opacity = 0.50f + (float)_random.NextDouble() * 0.50f;
                        c.HeadIcon = _random.Next(_icons.Length);
                        c.PulseSegment = _random.Next(c.TrailLength);
                        c.NextPulseTime = MinPulseDelay + (i % 9) * 0.06f + (float)_random.NextDouble() * 0.12f;
                        c.Color = GetRandomColor();
                    }
                }
            }

            base.Update(gameTime);
        }

        private float GetSpawnY(in Column c, float screenHeight)
        {
            var trailSpan = c.TrailLength * c.Spacing;
            return -trailSpan - (float)_random.NextDouble() * (screenHeight * (VerticalSpreadScreens - 1f));
        }

        private bool CanReleaseChain(in Column blocker, in Column candidate, float candidateSpawnY, float screenHeight)
        {
            if (blocker.WaitingForSafeRelease)
                return true;

            var blockerTailY = blocker.Y - (blocker.TrailLength - 1) * blocker.Spacing;
            var initialHeadGap = blockerTailY - candidateSpawnY - ChainSafetyGap;

            // Candidate head starts too close to blocker tail.
            if (initialHeadGap <= 0f)
                return false;

            // If candidate is not faster, safe forever once initial gap is safe.
            if (candidate.Speed <= blocker.Speed)
                return true;

            var relativeSpeed = candidate.Speed - blocker.Speed;
            if (relativeSpeed <= 0f)
                return true;

            // Time until candidate head reaches blocker tail (minus safety gap).
            var catchTime = initialHeadGap / relativeSpeed;

            // Time until blocker tail fully leaves active area.
            var timeToBlockerExit = (screenHeight + WrapMargin - blockerTailY) / Math.Max(blocker.Speed, 0.0001f);

            return catchTime > timeToBlockerExit + 0.25f;
        }


        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            EnsureAtlasBuilt();
            if (!_atlasReady)
                return;

            var sb = GameBase.Game.SpriteBatch;
            var screenHeight = WindowManager.VirtualScreen.Y;
            var topCull = -IconDrawSize - WrapMargin;
            var bottomCull = screenHeight + IconDrawSize + WrapMargin;

            for (var i = 0; i < _effectiveColumnCount; i++)
            {
                ref var c = ref _columns[i];
                if (c.WaitingForSafeRelease)
                    continue;
                var x = c.BaseX;
                // The simulation intentionally runs at 30 Hz. Interpolate its continuous motion
                // at render time so the rain remains smooth on 60 Hz and faster displays.
                var y = c.Y + c.Speed * _updateAccumulator;
                var spacing = c.Spacing;
                var invSpacing = 1f / spacing;
                var minVisible = (int)Math.Ceiling((y - bottomCull) * invSpacing);
                var maxVisible = (int)Math.Floor((y - topCull) * invSpacing);
                if (maxVisible < 0 || minVisible >= c.TrailLength)
                    continue;

                if (minVisible < 0)
                    minVisible = 0;
                if (maxVisible >= c.TrailLength)
                    maxVisible = c.TrailLength - 1;

                for (var n = minVisible; n <= maxVisible; n++)
                {
                    var alpha = _trailAlphas[n];
                    if (alpha <= 0.02f)
                        continue;

                    if (n == c.PulseSegment && c.PulseTimeRemaining > 0f)
                    {
                        var interpolatedPulseTime = Math.Max(0f, c.PulseTimeRemaining - _updateAccumulator);
                        var pulseStrength = MathHelper.Clamp(interpolatedPulseTime / c.PulseDuration, 0f, 1f);
                        alpha = MathHelper.Lerp(alpha, 1f, pulseStrength);
                    }

                    alpha *= c.Opacity;

                    var iconIndex = (c.HeadIcon + n) % _icons.Length;
                    var iconY = y - n * spacing;

                    _drawRect.X = (int)(x - IconDrawSize / 2f);
                    _drawRect.Y = (int)(iconY - IconDrawSize / 2f);
                    _drawRect.Width = IconDrawSize;
                    _drawRect.Height = IconDrawSize;

                    sb.Draw(_iconAtlas, _drawRect, _iconSourceRects[iconIndex], c.Color * alpha);
                }
            }
        }

        public override void Destroy()
        {
            _iconAtlas?.Dispose();
            base.Destroy();
        }
    }
}
