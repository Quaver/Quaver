using System;
using System.Collections.Generic;
using System.IO;
using IniFileParser.Model;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public class TournamentPlayerWinnerDisplay : TournamentPlayerScoreValue
    {
        /// <summary>
        /// </summary>
        public static string Directory => TournamentOverlay.Directory;

        private Sprite Icon { get; set; }

        private bool IsWinning { get; set; }

        public TournamentPlayerWinnerDisplay(TournamentDrawableSettings settings, TournamentPlayer player, List<TournamentPlayer> players)
            : base(settings, player, players)
        {
            InitializeIcon();
            UpdateState();
        }

        public sealed override void UpdateState()
        {
            base.UpdateState();

            if (Icon != null)
            {
                Text = " ";

                if (Icon.Parent != this)
                    Icon.Parent = this;

                Icon.Alignment = Settings.Alignment.Value;
                Icon.Visible = Settings.Visible.Value;
                Icon.Position = new ScalableVector2(Settings.Position.Value.X, Settings.Position.Value.Y);

                var settings = (TournamentPlayerWinnerDisplaySettings) Settings;
                var scale = settings.Scale.Value / 100f;

                Icon.Size = new ScalableVector2(Icon.Image.Width * scale, Icon.Image.Height * scale);
            }
        }

        public override void Destroy()
        {
            DisposeIcon();
            base.Destroy();
        }

        protected override void SetValue()
        {
        }

        private void InitializeIcon()
        {
            var player = Players.IndexOf(Player) + 1;

            try
            {
                var path = $"{Directory}/winner-{player}.png";

                if (!File.Exists(path))
                {
                    Logger.Warning($"Skipping load on `Player {player}` winner display due to the file: `{path}` not being found.", LogType.Runtime);
                    return;
                }

                var tex = AssetLoader.LoadTexture2DFromFile(path);

                Icon = new Sprite
                {
                    Parent = this,
                    Image = tex,
                };
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        private void DisposeIcon()
        {
            if (Icon != null && Icon.Image != UserInterface.BlankBox)
                Icon.Image?.Dispose();

            Icon?.Destroy();
        }

        protected override void OnWinning()
        {
            if (Icon == null)
                return;

            Icon.Parent = this;

            if (!IsWinning)
            {
                Icon.ClearAnimations();
                Icon.FadeTo(1, Easing.Linear, 250);
            }

            IsWinning = true;
        }

        protected override void OnLosing()
        {
            if (Icon == null)
                return;

            Icon.Parent = this;

            if (IsWinning)
            {
                Icon.ClearAnimations();
                Icon.FadeTo(0, Easing.Linear, 250);
            }

            IsWinning = false;
        }
    }

    public class TournamentPlayerWinnerDisplaySettings : TournamentDrawableSettings
    {
        public BindableInt Scale { get; } = new BindableInt(100, 0, int.MaxValue);

        public TournamentPlayerWinnerDisplaySettings(string name) : base(name)
        {
        }

        public override void Load(KeyDataCollection ini)
        {
            Scale.Value = ConfigHelper.ReadInt32(Scale.Default, ini[$"{Name}Scale"]);
            base.Load(ini);
        }

        public override void Dispose()
        {
            Scale.Dispose();
            base.Dispose();
        }
    }
}