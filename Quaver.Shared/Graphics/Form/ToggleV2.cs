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
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Form
{
    /// <summary>
    ///     Groups toggle visuals by texture type so repeated toggles can be drawn in batches
    /// </summary>
    internal sealed class ToggleV2LayerSet
    {
        internal Container TrackLayer { get; }
        internal Container AccentLayer { get; }
        internal Container KnobLayer { get; }
        internal Container TextLayer { get; }

        internal ToggleV2LayerSet(Container parent)
        {
            TrackLayer = CreateLayer(parent);
            AccentLayer = CreateLayer(parent);
            KnobLayer = CreateLayer(parent);
            TextLayer = CreateLayer(parent);
        }

        private static Container CreateLayer(Container parent) => new()
        {
            Parent = parent,
            UsePreviousSpriteBatchOptions = true
        };

        internal void Resize(ScalableVector2 size)
        {
            TrackLayer.Size = size;
            AccentLayer.Size = size;
            KnobLayer.Size = size;
            TextLayer.Size = size;
        }
    }

    public class ToggleV2 : Container
    {
        public event EventHandler ValueEdited;

        private Bindable<bool> Enabled { get; }

        private static Texture2D? SharedGradientTexture { get; set; }

        private Texture2D GradientTexture { get; }

        private bool UsesExternalLayers { get; }
        private Container TrackHost { get; }
        private Container AccentHost { get; }
        private Container KnobHost { get; }
        private Container TextHost { get; }
        private Vector2 VisualPosition { get; set; }
        private RoundedButton ToggleArea { get; }
        private Sprite OffAccent { get; }
        private Sprite OnAccent { get; }
        private RoundedButton MovingButton { get; }
        private SpriteTextPlus? ToggleTextLabel { get; }
        private SharedTextAtlasSprite? ToggleAtlasLabel { get; }
        private Drawable ToggleLabel => ToggleAtlasLabel != null ? ToggleAtlasLabel  : ToggleTextLabel!;
        private string? EnabledTextKey { get; }
        private string? DisabledTextKey { get; }

        private bool DisplayedValue { get; set; }

        public bool IsClickable
        {
            get => ToggleArea.IsClickable;
            set
            {
                ToggleArea.IsClickable = value;
                ToggleArea.IsInteractionEnabled = value;
            }
        }

        public ToggleV2(Bindable<bool> enabled) : this(enabled, null, null, null, null)
        {
        }

        internal ToggleV2(Bindable<bool> enabled, ToggleV2LayerSet? layers)
            : this(enabled, layers, null, null, null)
        {
        }

        internal ToggleV2(Bindable<bool> enabled, ToggleV2LayerSet? layers, SharedTextAtlas? textAtlas, string? enabledTextKey, string? disabledTextKey)
        {
            Enabled = enabled ?? new Bindable<bool>(false);
            Size = new ScalableVector2(58, 20);

            UsesExternalLayers = layers != null;
            TrackHost = layers?.TrackLayer ?? this;
            AccentHost = layers?.AccentLayer ?? this;
            KnobHost = layers?.KnobLayer ?? this;
            TextHost = layers?.TextLayer ?? this;

            ToggleArea = new RoundedButton
            {
                Parent = TrackHost,
                Size = Size,
                CornerRadius = Height / 2f,
                Tint = ColorHelper.FromHex("#273038"),
                PerformHoverFade = false,
                UsePreviousSpriteBatchOptions = true
            };

            GradientTexture = SharedGradientTexture ??= CreateGradientTexture();

            OffAccent = new Sprite
            {
                Parent = AccentHost,
                Size = Size,
                Image = GradientTexture,
                Tint = ColorHelper.FromHex("#FF3A6F"),
                Alpha = Enabled.Value ? 0 : 0.8f,
                Visible = !Enabled.Value,
                UsePreviousSpriteBatchOptions = true
            };

            OnAccent = new Sprite
            {
                Parent = AccentHost,
                Size = Size,
                Image = GradientTexture,
                SpriteEffect = SpriteEffects.FlipHorizontally,
                Tint = ColorHelper.FromHex("#25C88C"),
                Alpha = Enabled.Value ? 0.8f : 0,
                Visible = Enabled.Value,
                UsePreviousSpriteBatchOptions = true
            };

            MovingButton = new RoundedButton
            {
                Parent = KnobHost,
                Size = new ScalableVector2(31.5f, Height - 2.5f * 2),
                Y = 2.5f,
                CornerRadius = 9.23f, // Ask figma, not me
                Tint = Color.White,
                IsClickable = false,
                IsInteractionEnabled = false,
                UsePreviousSpriteBatchOptions = true
            };
            MovingButton.X = GetButtonX(Enabled.Value);

            EnabledTextKey = enabledTextKey;
            DisabledTextKey = disabledTextKey;
            if (textAtlas != null)
            {
                ToggleAtlasLabel = new SharedTextAtlasSprite(textAtlas, GetTextKey(Enabled.Value))
                {
                    Parent = TextHost,
                    Alignment = Alignment.TopLeft,
                    Tint = GetAccentColor(Enabled.Value),
                    UsePreviousSpriteBatchOptions = true
                };
            }
            else
            {
                ToggleTextLabel = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), GetLabel(Enabled.Value), 11, true)
                {
                    Parent = TextHost,
                    Alignment = Alignment.TopLeft,
                    Tint = GetAccentColor(Enabled.Value),
                    UsePreviousSpriteBatchOptions = true
                };
            }

            ToggleLabel.X = GetLabelX(Enabled.Value);
            ToggleLabel.Y = GetLabelY();
            DisplayedValue = Enabled.Value;

            ToggleArea.Clicked += OnClicked;
            Enabled.ValueChanged += OnValueChanged;
        }

        internal void SetVisualPosition(ScalableVector2 position)
        {
            if (!UsesExternalLayers)
                return;

            VisualPosition = new Vector2(position.X.Value, position.Y.Value);
            ToggleArea.Position = position;
            OffAccent.Position = position;
            OnAccent.Position = position;

            MovingButton.Position = new ScalableVector2(GetButtonX(DisplayedValue), VisualPosition.Y + 2.5f);
            ToggleLabel.Position = new ScalableVector2(GetLabelX(DisplayedValue), GetLabelY());
        }

        private float GetButtonX(bool enabled) => VisualPosition.X + (enabled ? Width - 2.5f - MovingButton.Width : 2.5f);

        private float GetLabelX(bool enabled) => GetButtonX(enabled) + (MovingButton.Width - GetLabelWidth()) / 2f - GetLabelInset().X;

        private float GetLabelY() => VisualPosition.Y + (Height - GetLabelHeight()) / 2f - GetLabelInset().Y;

        private float GetLabelWidth() => ToggleAtlasLabel?.ContentWidth ?? ToggleTextLabel!.Width;

        private float GetLabelHeight() => ToggleAtlasLabel?.ContentHeight ?? ToggleTextLabel!.Height;

        private Vector2 GetLabelInset() => ToggleAtlasLabel?.ContentInset ?? Vector2.Zero;

        private static Color GetAccentColor(bool enabled) => enabled
            ? ColorHelper.FromHex("#25C88C") : ColorHelper.FromHex("#FF3A6F");

        private static string GetLabel(bool enabled) => enabled ? "ON" : "OFF";

        private string GetTextKey(bool enabled) => (enabled ? EnabledTextKey : DisabledTextKey)
            ?? throw new InvalidOperationException("Atlas-backed toggles require both text keys.");

        private void SetLabel(bool value)
        {
            if (ToggleAtlasLabel != null)
            {
                ToggleAtlasLabel.SetKey(GetTextKey(value));
                ToggleAtlasLabel.Tint = GetAccentColor(value);
                return;
            }

            ToggleTextLabel!.Text = GetLabel(value);
            ToggleTextLabel.Tint = GetAccentColor(value);
        }

        private void OnClicked(object sender, EventArgs e)
        {
            var previousValue = Enabled.Value;
            Enabled.Value = !Enabled.Value;
            ApplyVisualState(Enabled.Value);

            if (Enabled.Value != previousValue)
                ValueEdited?.Invoke(this, EventArgs.Empty);
        }

        private void OnValueChanged(object sender, BindableValueChangedEventArgs<bool> e)
            => ApplyVisualState(Enabled.Value);

        private void ApplyVisualState(bool value)
        {
            if (DisplayedValue == value)
                return;

            DisplayedValue = value;
            OffAccent.ClearAnimations();
            OffAccent.Visible = true;
            OffAccent.FadeTo(value ? 0 : 0.8f, Easing.OutCubic, 180);

            OnAccent.ClearAnimations();
            OnAccent.Visible = true;
            OnAccent.FadeTo(value ? 0.8f : 0, Easing.OutCubic, 180);

            MovingButton.ClearAnimations();
            MovingButton.MoveToX(GetButtonX(value), Easing.OutCubic, 180);
            ToggleLabel.ClearAnimations();
            SetLabel(value);
            ToggleLabel.MoveToX(GetLabelX(value), Easing.OutCubic, 180);
        }

        public override void Update(GameTime gameTime)
        {
            if (DisplayedValue != Enabled.Value)
                ApplyVisualState(Enabled.Value);

            OffAccent.Visible = OffAccent.Alpha > 0.001f || OffAccent.Animations.Count > 0;
            OnAccent.Visible = OnAccent.Alpha > 0.001f || OnAccent.Animations.Count > 0;

            base.Update(gameTime);
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

            var texture = new Texture2D(GameBase.Game.GraphicsDevice, width, height, false,
                SurfaceFormat.Color);
            texture.SetData(pixels);
            return texture;
        }

        public override void Destroy()
        {
            ToggleArea.Clicked -= OnClicked;
            Enabled.ValueChanged -= OnValueChanged;

            ToggleArea.Destroy();
            OffAccent.Destroy();
            OnAccent.Destroy();
            MovingButton.Destroy();
            ToggleLabel.Destroy();

            base.Destroy();
        }
    }
}
