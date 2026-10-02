using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Form
{
    public class ToggleV2 : Container
    {
        public event EventHandler ValueEdited;

        private Bindable<bool> Enabled { get; }

        private Texture2D GradientTexture { get; }

        private Sprite ToggleArea { get; }
        private Sprite OffAccent { get; }
        private Sprite OnAccent { get; }
        private RoundedButton MovingButton { get; }

        public ToggleV2(Bindable<bool> enabled)
        {
            Enabled = enabled ?? new Bindable<bool>(false);
            Size = new ScalableVector2(58, 20);

            ToggleArea = new Sprite
            {
                Parent = this,
                Size = Size,
                Image = RoundedRectTextureCache.Get(58, Height, Height / 2),
                Tint = ColorHelper.FromHex("#273038")
            };

            GradientTexture = CreateGradientTexture();

            OffAccent = new Sprite
            {
                Parent = this,
                Size = Size,
                Image = GradientTexture,
                Tint = ColorHelper.FromHex("#FF3A6F"),
                Alpha = Enabled.Value ? 0 : 0.8f
            };

            OnAccent = new Sprite
            {
                Parent = this,
                Size = Size,
                Image = GradientTexture,
                SpriteEffect = SpriteEffects.FlipHorizontally,
                Tint = ColorHelper.FromHex("#25C88C"),
                Alpha = Enabled.Value ? 0.8f : 0
            };

            MovingButton = new RoundedButton
            {
                Parent = this,
                Size = new ScalableVector2(31.5f, Height - 2.5f * 2),
                Y = 2.5f,
                CornerRadius = 9.23f, // Ask figma, not me
                Tint = Color.White
            };
            MovingButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterSemiBold), GetLabel(Enabled.Value), 11, GetAccentColor(Enabled.Value));
            MovingButton.X = GetButtonX(Enabled.Value);

            MovingButton.Clicked += OnClicked;
            Enabled.ValueChanged += OnValueChanged;
        }

        private float GetButtonX(bool enabled) => enabled ? Width - 2.5f - MovingButton.Width : 2.5f;

        private static Color GetAccentColor(bool enabled) => enabled ? ColorHelper.FromHex("#25C88C") : ColorHelper.FromHex("#FF3A6F");

        private static string GetLabel(bool enabled) => enabled ? "ON" : "OFF";

        private void OnClicked(object sender, EventArgs e)
        {
            Enabled.Value = !Enabled.Value;
            ValueEdited?.Invoke(this, EventArgs.Empty);
        }

        private void OnValueChanged(object sender, BindableValueChangedEventArgs<bool> e)
        {
            OffAccent.ClearAnimations();
            OffAccent.FadeTo(e.Value ? 0 : 0.8f, Easing.OutCubic, 180);

            OnAccent.ClearAnimations();
            OnAccent.FadeTo(e.Value ? 0.8f : 0, Easing.OutCubic, 180);

            MovingButton.ClearAnimations();
            MovingButton.MoveToX(GetButtonX(e.Value), Easing.OutCubic, 180);
            MovingButton.Label.Text = GetLabel(e.Value);
            MovingButton.Label.Tint = GetAccentColor(e.Value);
        }

        private Texture2D CreateGradientTexture()
        {
            var width = (int) Width;
            var height = (int) Height;
            var halfWidth = width / 2f;
            var halfHeight = height / 2f;
            var radius = ToggleArea.Height / 2f;

            var pixels = new Color[width * height];
            for (var x = 0; x < width; x++)
            {
                // Fading effect
                var opacity = MathHelper.Clamp(1 - x / (width / 2f), 0, 1);

                for (var y = 0; y < height; y++)
                {
                    // Match the rounded edge
                    var qx = Math.Abs(x + 0.5f - halfWidth) - (halfWidth - radius);
                    var qy = Math.Abs(y + 0.5f - halfHeight) - (halfHeight - radius);
                    var outsideDistance = (float) Math.Sqrt(Math.Max(qx, 0) * Math.Max(qx, 0) + Math.Max(qy, 0) * Math.Max(qy, 0));
                    var distance = outsideDistance + Math.Min(Math.Max(qx, qy), 0) - radius;
                    var edge = MathHelper.Clamp(distance + 0.5f, 0, 1);
                    var coverage = 1 - edge * edge * (3 - 2 * edge);

                    pixels[y * width + x] = new Color((byte) 255, (byte) 255, (byte) 255, (byte) (opacity * coverage * 255));
                }
            }

            var texture = new Texture2D(GameBase.Game.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            texture.SetData(pixels);
            return texture;
        }

        public override void Destroy()
        {
            MovingButton.Clicked -= OnClicked;
            Enabled.ValueChanged -= OnValueChanged;

            base.Destroy();
            GradientTexture.Dispose();
        }
    }
}
