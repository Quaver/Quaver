using System;
using System.Collections.Generic;
using System.Linq;
using IniFileParser.Model;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Modifiers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public class TournamentPlayerModifiers : Sprite
    {
        protected TournamentSettingsPlayerModifiers Settings { get; }

        protected TournamentPlayer Player { get; }

        private List<Sprite> Icons { get; set; }

        public TournamentPlayerModifiers(TournamentSettingsPlayerModifiers settings, TournamentPlayer player)
        {
            Settings = settings;
            Player = player;
            Alpha = 0;

            UpdateState();

            Settings.Visible.ValueChanged += (sender, args) => UpdateState();
            Settings.Position.ValueChanged += (sender, args) => UpdateState();
            Settings.Alignment.ValueChanged += (sender, args) => UpdateState();
            Settings.Scale.ValueChanged += (sender, args) => UpdateState();
            Settings.Spacing.ValueChanged += (sender, args) => UpdateState();
        }

        private void UpdateState()
        {
            Visible = Settings.Visible.Value;
            Position = new ScalableVector2(Settings.Position.Value.X, Settings.Position.Value.Y);
            Alignment = Settings.Alignment.Value;

            InitializeIcons();

            var totalSpacing = 0f;
            var scale = Settings.Scale.Value / 100f;

            for (var i = 0; i < Icons.Count; i++)
            {
                var icon = Icons[i];

                icon.Y = totalSpacing;

                icon.Size = new ScalableVector2(icon.Image.Width * scale, icon.Image.Height * scale);
                totalSpacing += icon.Height;

                if (i != Icons.Count - 1)
                    totalSpacing += Settings.Spacing.Value;
            }

            Size = new ScalableVector2(Icons.Count != 0 ? Icons.First().Width : 0, totalSpacing);
        }

        private void InitializeIcons()
        {
            if (Icons != null)
                return;

            Icons = new List<Sprite>();

            var mods = Player.Scoring.Mods;

            for (var i = 0; i <= Math.Log((long)mods, 2); i++)
            {
                var mod = (ModIdentifier)(long)Math.Pow(2, i);

                if (!mods.HasFlag(mod))
                    continue;

                // ReSharper disable once ObjectCreationAsStatement
                Icons.Add(new Sprite
                {
                    Parent = this,
                    Image = ModManager.GetTexture(mod)
                });
            }
        }
    }

    public class TournamentSettingsPlayerModifiers : TournamentDrawableSettings
    {
        public BindableInt Scale { get; } = new BindableInt(100, 0, int.MaxValue);

        public BindableInt Spacing { get; } = new BindableInt(20, int.MinValue, int.MaxValue);

        public TournamentSettingsPlayerModifiers(string name) : base(name)
        {
        }

        public override void Load(KeyDataCollection ini)
        {
            Scale.Value = ConfigHelper.ReadInt32(Scale.Default, ini[$"{Name}Scale"]);
            Spacing.Value = ConfigHelper.ReadInt32(Spacing.Default, ini[$"{Name}Spacing"]);

            base.Load(ini);
        }

        public override void Dispose()
        {
            Scale.Dispose();
            Spacing.Dispose();

            base.Dispose();
        }
    }
}