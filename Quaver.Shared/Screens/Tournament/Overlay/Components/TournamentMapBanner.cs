using System;
using IniFileParser.Model;
using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Music.UI.Controller;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public class TournamentMapBanner : MusicControllerBackground
    {
        protected TournamentSettingsBanner Settings { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="size"></param>
        public TournamentMapBanner(TournamentSettingsBanner settings, ScalableVector2 size) : base(size, false)
        {
            Settings = settings;
            DarknessVisibleAlpha = 0;
            UpdateState();

            Settings.Visible.ValueChanged += (sender, args) => UpdateState();
            Settings.Position.ValueChanged += (sender, args) => UpdateState();
            Settings.Alignment.ValueChanged += (sender, args) => UpdateState();
            Settings.Tint.ValueChanged += (sender, args) => UpdateState();
            Settings.Size.ValueChanged += (sender, args) => UpdateState();
            Settings.DarknessAlpha.ValueChanged += (sender, args) => UpdateState();
            Settings.BackgroundSize.ValueChanged += (sender, args) => UpdateState();
        }

        public void UpdateState()
        {
            var settings = Settings;

            Visible = Settings.Visible.Value;
            Position = new ScalableVector2(Settings.Position.Value.X, Settings.Position.Value.Y);
            Alignment = Settings.Alignment.Value;
            Tint = Settings.Tint.Value;

            Size = new ScalableVector2(settings.Size.Value.X, settings.Size.Value.Y);
            Darkness.Size = Size;
            ContentContainer.Size = Size;

            Background.Size = new ScalableVector2(Settings.BackgroundSize.Value.X, Settings.BackgroundSize.Value.Y);
            Background.Y = Settings.BackgroundOffsetY.Value;
            Darkness.Alpha = settings.DarknessAlpha.Value;
        }
    }

    public class TournamentSettingsBanner : TournamentDrawableSettings
    {
        public Bindable<Vector2> Size { get; } = new Bindable<Vector2>(new Vector2(300, 100));

        public Bindable<Vector2> BackgroundSize { get; } = new Bindable<Vector2>(new Vector2(1920, 1080));

        public BindableFloat DarknessAlpha { get; } = new BindableFloat(0, 0, 1);

        public BindableInt BackgroundOffsetY { get; } = new BindableInt(-100, 0, int.MaxValue);

        public TournamentSettingsBanner(string name) : base(name)
        {
        }

        public override void Load(KeyDataCollection ini)
        {
            Size.Value = ConfigHelper.ReadVector2(Size.Default, ini[$"{Name}Size"]);
            BackgroundSize.Value = ConfigHelper.ReadVector2(BackgroundSize.Default, ini[$"{Name}BackgroundSize"]);
            DarknessAlpha.Value = ConfigHelper.ReadFloat(DarknessAlpha.Default, ini[$"{Name}DarknessAlpha"]);
            BackgroundOffsetY.Value = ConfigHelper.ReadInt32(BackgroundOffsetY.Default, ini[$"{Name}BackgroundOffsetY"]);
            base.Load(ini);
        }

        public override void Dispose()
        {
            Size.Dispose();
            DarknessAlpha.Dispose();
            BackgroundSize.Dispose();
            BackgroundOffsetY.Dispose();
            base.Dispose();
        }
    }
}