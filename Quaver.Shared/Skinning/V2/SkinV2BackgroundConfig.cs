using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.UI.Navigation;

namespace Quaver.Shared.Skinning.V2
{
    /// <summary>
    ///     Reusable solid-color, image, or gradient background configuration for Skin V2 surfaces.
    /// </summary>
    public sealed class SkinV2BackgroundConfig
    {
        [EnumDataType(typeof(NavigationBarBackgroundType))]
        public NavigationBarBackgroundType Type { get; set; } = NavigationBarBackgroundType.SolidColor;

        [SkinColor] public string SolidColor { get; set; } = "#00000000";
        [Required] public SkinV2BackgroundImageConfig Image { get; set; } = new SkinV2BackgroundImageConfig();
        [Required] public SkinV2BackgroundGradientConfig Gradient { get; set; } =
            new SkinV2BackgroundGradientConfig();
    }

    public sealed class SkinV2BackgroundImageConfig
    {
        [SkinAssetPath] public string Path { get; set; } = "";

        [EnumDataType(typeof(NavigationBarImageFit))]
        public NavigationBarImageFit Fit { get; set; } = NavigationBarImageFit.Stretch;
    }

    public sealed class SkinV2BackgroundGradientConfig
    {
        [EnumDataType(typeof(NavigationBarGradientType))]
        public NavigationBarGradientType Type { get; set; } = NavigationBarGradientType.Linear;

        [SkinV2GradientStops]
        public List<SkinV2GradientStopConfig> Stops { get; set; } = new List<SkinV2GradientStopConfig>
        {
            new SkinV2GradientStopConfig(0, "#080D13FF"),
            new SkinV2GradientStopConfig(1, "#24384AFF")
        };

        [Range(-360, 360)] public float AngleDegrees { get; set; }
        [Required] public SkinV2GradientOriginConfig RadialOrigin { get; set; } =
            new SkinV2GradientOriginConfig();
        [Range(0.01, 100)] public float RadialRadius { get; set; } = 1;
    }

    public sealed class SkinV2GradientStopConfig
    {
        [Range(0, 1)] public float Position { get; set; }
        [SkinColor] public string Color { get; set; } = "#FFFFFFFF";

        public SkinV2GradientStopConfig()
        {
        }

        public SkinV2GradientStopConfig(float position, string color)
        {
            Position = position;
            Color = color;
        }
    }

    public sealed class SkinV2GradientOriginConfig
    {
        [Range(0, 1)] public float X { get; set; } = 0.5f;
        [Range(0, 1)] public float Y { get; set; } = 0.5f;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SkinV2GradientStopsAttribute : ValidationAttribute
    {
        private static readonly SkinColorAttribute ColorValidator = new SkinColorAttribute();

        public override bool IsValid(object value)
        {
            if (!(value is IReadOnlyList<SkinV2GradientStopConfig> stops) || stops.Count < 2)
                return false;

            var previousPosition = float.NegativeInfinity;
            foreach (var stop in stops)
            {
                if (stop == null || float.IsNaN(stop.Position) || float.IsInfinity(stop.Position) ||
                    stop.Position < 0 || stop.Position > 1 || stop.Position <= previousPosition ||
                    !ColorValidator.IsValid(stop.Color))
                    return false;

                previousPosition = stop.Position;
            }

            return true;
        }

        public override string FormatErrorMessage(string name) =>
            $"{name} must contain at least two color stops with unique, increasing positions from 0 to 1.";
    }

    internal static class SkinV2Background
    {
        public static NavigationBarBackgroundOptions Create(SkinStoreV2Lease skin, SkinV2BackgroundConfig config,
            Texture2D fallbackImage = null)
        {
            switch (config.Type)
            {
                case NavigationBarBackgroundType.Image:
                    return new NavigationBarBackgroundOptions
                    {
                        Type = NavigationBarBackgroundType.Image,
                        Image = skin.LoadTexture(config.Image.Path, fallbackImage),
                        ImageFit = config.Image.Fit
                    };
                case NavigationBarBackgroundType.Gradient:
                    return new NavigationBarBackgroundOptions
                    {
                        Type = NavigationBarBackgroundType.Gradient,
                        Gradient = new NavigationBarGradientOptions
                        {
                            Type = config.Gradient.Type,
                            Stops = config.Gradient.Stops.Select(stop => new NavigationBarGradientStop
                            {
                                Position = stop.Position,
                                Color = SkinV2Color.Parse(stop.Color)
                            }).ToArray(),
                            AngleDegrees = config.Gradient.AngleDegrees,
                            RadialOrigin = new Vector2(config.Gradient.RadialOrigin.X,
                                config.Gradient.RadialOrigin.Y),
                            RadialRadius = config.Gradient.RadialRadius
                        }
                    };
                default:
                    return new NavigationBarBackgroundOptions
                    {
                        Type = NavigationBarBackgroundType.SolidColor,
                        SolidColor = SkinV2Color.Parse(config.SolidColor)
                    };
            }
        }
    }
}
