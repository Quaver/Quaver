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
    ///     A particle system that renders small drifting particles with glow effect.
    ///     Particles move in random directions with curved paths and fade in/out over their lifetime.
    /// </summary>
    public class BackgroundParticleSystem : Sprite
    {
        /// <summary>
        ///     Internal particle data structure (struct for performance - no GC allocations).
        /// </summary>
        private struct Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Size;
            public Color BaseColor;       // Color without alpha (for fading)
            public float BaseAlpha;       // Target alpha at full visibility
            public float AngularVelocity; // For curved paths
            public float Angle;           // Current direction angle
            public float Speed;           // Base speed
            public float WavePhase;       // For sine wave offset
            public float WaveAmplitude;   // Amplitude of sine wave
            public float Lifetime;        // Total lifetime in seconds
            public float Age;             // Current age in seconds
        }

        /// <summary>
        ///     Array of all particles (fixed size, no allocations during updates).
        /// </summary>
        private Particle[] particles;

        /// <summary>
        ///     Random number generator for particle initialization.
        /// </summary>
        private readonly Random random;

        private readonly Color color1;

        private readonly Color color2;

        /// <summary>
        ///     Soft circle texture with glow effect (generated at runtime).
        /// </summary>
        private Texture2D glowTexture;
        private Rectangle _particleDrawRect;

        /// <summary>
        ///     Number of particles to render.
        /// </summary>
        public int ParticleCount { get; }

        /// <summary>
        ///     Minimum particle speed in pixels per second.
        /// </summary>
        private const float MinSpeed = 8f;

        /// <summary>
        ///     Maximum particle speed in pixels per second.
        /// </summary>
        private const float MaxSpeed = 40f;

        /// <summary>
        ///     Minimum particle size in pixels.
        /// </summary>
        private const float MinSize = 2f;

        /// <summary>
        ///     Maximum particle size in pixels.
        /// </summary>
        private const float MaxSize = 5f;

        /// <summary>
        ///     Minimum alpha (transparency) value.
        /// </summary>
        private const float MinAlpha = 0.5f;

        /// <summary>
        ///     Maximum alpha (transparency) value.
        /// </summary>
        private const float MaxAlpha = 1.0f;

        /// <summary>
        ///     Screen margin for wrap-around detection.
        /// </summary>
        private const float WrapMargin = 20f;

        /// <summary>
        ///     Size of the glow texture.
        /// </summary>
        private const int GlowTextureSize = 32;

        /// <summary>
        ///     Base particle lifetime in seconds.
        /// </summary>
        private const float ParticleLifetime = 6.0f;

        /// <summary>
        ///     Duration of fade in/out effect (portion of lifetime).
        /// </summary>
        private const float FadeDuration = 0.15f; // 15% of lifetime for fade in, 15% for fade out

        /// <summary>
        ///     Creates a new particle system.
        /// </summary>
        /// <param name="particleCount">Number of particles (default 315)</param>
        public BackgroundParticleSystem(Color color1, Color color2, int particleCount = 315)
        {
            this.color1 = color1;
            this.color2 = color2;
            ParticleCount = particleCount;
            random = new Random();

            // Make this sprite invisible (we draw particles manually)
            Alpha = 0f;

            // Set size to full screen
            Size = new Wobble.Graphics.ScalableVector2(WindowManager.VirtualScreen.X, WindowManager.VirtualScreen.Y);

            CreateGlowTexture();
            InitializeParticles();
        }

        /// <summary>
        ///     Creates a soft circular glow texture procedurally.
        /// </summary>
        private void CreateGlowTexture()
        {
            glowTexture = new Texture2D(GameBase.Game.GraphicsDevice, GlowTextureSize, GlowTextureSize);
            var colorData = new Color[GlowTextureSize * GlowTextureSize];

            var center = GlowTextureSize / 2f;
            var maxRadius = GlowTextureSize / 2f;

            for (int y = 0; y < GlowTextureSize; y++)
            {
                for (int x = 0; x < GlowTextureSize; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    // Soft falloff from center to edge
                    float alpha;
                    if (distance <= maxRadius * 0.15f)
                    {
                        // Core: bright center (smaller - 15% of radius)
                        alpha = 1.0f;
                    }
                    else if (distance <= maxRadius)
                    {
                        // Glow: smooth falloff (larger - 85% of radius)
                        var t = (distance - maxRadius * 0.15f) / (maxRadius * 0.85f);
                        alpha = 1.0f - (t * t); // Quadratic falloff for soft glow
                    }
                    else
                    {
                        alpha = 0f;
                    }

                    colorData[y * GlowTextureSize + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            glowTexture.SetData(colorData);
        }

        /// <summary>
        ///     Initializes all particles with random positions, velocities, sizes, and colors.
        ///     Stagger initial ages so particles don't all die at once.
        /// </summary>
        private void InitializeParticles()
        {
            particles = new Particle[ParticleCount];

            var screenWidth = WindowManager.VirtualScreen.X;
            var screenHeight = WindowManager.VirtualScreen.Y;

            for (int i = 0; i < ParticleCount; i++)
            {
                // Stagger initial age so particles die at different times
                float initialAge = (float)random.NextDouble() * ParticleLifetime;
                particles[i] = CreateParticle(screenWidth, screenHeight, initialAge);
            }
        }

        /// <summary>
        ///     Creates a single particle with random properties.
        /// </summary>
        private Particle CreateParticle(float screenWidth, float screenHeight, float initialAge = 0f)
        {
            // Random speed (slower = smaller/darker, faster = larger/brighter for depth effect)
            float speed = MinSpeed + (float)random.NextDouble() * (MaxSpeed - MinSpeed);
            float speedRatio = (speed - MinSpeed) / (MaxSpeed - MinSpeed); // 0 to 1

            // Size based on speed with stronger curve to favor smaller particles
            // Using power of 0.3 for even stronger bias toward small
            float sizeRatio = (float)Math.Pow(speedRatio, 0.3);
            float size = MinSize + sizeRatio * (MaxSize - MinSize);

            // Alpha based on speed (depth effect)
            float alpha = MinAlpha + speedRatio * (MaxAlpha - MinAlpha);

            // Random position across screen
            Vector2 position = new Vector2(
                (float)random.NextDouble() * screenWidth,
                (float)random.NextDouble() * screenHeight
            );

            // Random direction (0 to 2*PI)
            float angle = (float)(random.NextDouble() * Math.PI * 2);

            // Angular velocity for curved paths (small random value)
            float angularVelocity = (float)(random.NextDouble() - 0.5) * 0.3f;

            // Sine wave parameters for additional curvature
            float wavePhase = (float)(random.NextDouble() * Math.PI * 2);
            float waveAmplitude = (float)(random.NextDouble() * 15f + 5f);

            // Velocity based on angle
            Vector2 velocity = new Vector2(
                (float)Math.Cos(angle) * speed,
                (float)Math.Sin(angle) * speed
            );

            // Get random particle color
            Color baseColor = GetRandomParticleColor();

            // Randomize lifetime slightly (±20%)
            float lifetime = ParticleLifetime * (0.8f + (float)random.NextDouble() * 0.4f);

            return new Particle
            {
                Position = position,
                Velocity = velocity,
                Size = size,
                BaseColor = baseColor,
                BaseAlpha = alpha,
                Angle = angle,
                AngularVelocity = angularVelocity,
                Speed = speed,
                WavePhase = wavePhase,
                WaveAmplitude = waveAmplitude,
                Lifetime = lifetime,
                Age = initialAge
            };
        }

        /// <summary>
        ///     Calculates the fade multiplier based on particle age.
        /// </summary>
        private float GetFadeMultiplier(float age, float lifetime)
        {
            float fadeInEnd = lifetime * FadeDuration;
            float fadeOutStart = lifetime * (1f - FadeDuration);

            if (age < fadeInEnd)
            {
                // Fade in
                return age / fadeInEnd;
            }
            else if (age > fadeOutStart)
            {
                // Fade out
                return (lifetime - age) / (lifetime - fadeOutStart);
            }
            else
            {
                // Full visibility
                return 1f;
            }
        }

        /// <summary>
        ///     Returns a random particle color between user-configured Color 1 and Color 2.
        /// </summary>
        private Color GetRandomParticleColor()
        {
            if (random.NextDouble() < 0.5)
                return color1;
            else
                return color2;
        }

        /// <summary>
        ///     Re-assigns colors for all existing particles based on current config values.
        /// </summary>
        public void RefreshConfiguredColors()
        {
            if (particles == null)
                return;

            for (var i = 0; i < particles.Length; i++)
                particles[i].BaseColor = GetRandomParticleColor();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Updates all particles (movement, wrap-around, and lifecycle).
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Skip particle updates when not visible to save CPU
            if (!Visible)
            {
                base.Update(gameTime);
                return;
            }

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var screenWidth = WindowManager.VirtualScreen.X;
            var screenHeight = WindowManager.VirtualScreen.Y;
            var totalTime = (float)gameTime.TotalGameTime.TotalSeconds;

            for (int i = 0; i < particles.Length; i++)
            {
                // Update age
                particles[i].Age += deltaTime;

                // Check if particle has died
                if (particles[i].Age >= particles[i].Lifetime)
                {
                    // Respawn particle at random position
                    particles[i] = CreateParticle(screenWidth, screenHeight, 0f);
                    continue;
                }

                // Update angle for curved path
                particles[i].Angle += particles[i].AngularVelocity * deltaTime;

                // Update velocity based on new angle
                particles[i].Velocity = new Vector2(
                    (float)Math.Cos(particles[i].Angle) * particles[i].Speed,
                    (float)Math.Sin(particles[i].Angle) * particles[i].Speed
                );

                // Move particle
                particles[i].Position += particles[i].Velocity * deltaTime;

                // Add sine wave offset perpendicular to direction
                float perpAngle = particles[i].Angle + (float)(Math.PI / 2);
                float waveOffset = (float)Math.Sin(totalTime * 0.5f + particles[i].WavePhase) * particles[i].WaveAmplitude * deltaTime;
                particles[i].Position += new Vector2(
                    (float)Math.Cos(perpAngle) * waveOffset,
                    (float)Math.Sin(perpAngle) * waveOffset
                );

                // Wrap-around logic (all edges)
                if (particles[i].Position.X > screenWidth + WrapMargin)
                    particles[i].Position.X = -WrapMargin;
                else if (particles[i].Position.X < -WrapMargin)
                    particles[i].Position.X = screenWidth + WrapMargin;

                if (particles[i].Position.Y > screenHeight + WrapMargin)
                    particles[i].Position.Y = -WrapMargin;
                else if (particles[i].Position.Y < -WrapMargin)
                    particles[i].Position.Y = screenHeight + WrapMargin;
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Draws all particles to the SpriteBatch.
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            var spriteBatch = GameBase.Game.SpriteBatch;

            for (int i = 0; i < particles.Length; i++)
            {
                var p = particles[i];
                var halfSize = (int)Math.Ceiling(p.Size / 2);
                var particleSize = (int)Math.Ceiling(p.Size);

                // Calculate fade multiplier
                float fadeMultiplier = GetFadeMultiplier(p.Age, p.Lifetime);
                float finalAlpha = p.BaseAlpha * fadeMultiplier;

                // Skip nearly invisible particles
                if (finalAlpha < 0.01f)
                    continue;

                // Apply alpha to color
                Color finalColor = p.BaseColor * finalAlpha;

                // Draw particle with glow texture
                _particleDrawRect.X = (int)p.Position.X - halfSize;
                _particleDrawRect.Y = (int)p.Position.Y - halfSize;
                _particleDrawRect.Width = particleSize;
                _particleDrawRect.Height = particleSize;

                spriteBatch.Draw(
                    glowTexture,
                    _particleDrawRect,
                    finalColor
                );
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Cleans up resources.
        /// </summary>
        public override void Destroy()
        {
            glowTexture?.Dispose();
            base.Destroy();
        }
    }
}
