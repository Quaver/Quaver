using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public class TournamentPlayerUsername : Container
    {
        private TournamentPlayer Player { get; }

        private TournamentDrawableSettings Settings { get; }

        private SpriteTextPlus TextUsername { get; set; }

        private Sprite Flag { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="settings"></param>
        public TournamentPlayerUsername(TournamentPlayer player, TournamentDrawableSettings settings)
        {
            Player = player;
            Settings = settings;

            CreateFlag();
            CreateUsername();
            Align();

            Settings.Visible.ValueChanged += (sender, args) => Align();
            Settings.Font.ValueChanged += (sender, args) => Align();
            Settings.FontSize.ValueChanged += (sender, args) => Align();
            Settings.Position.ValueChanged += (sender, args) => Align();
            Settings.Alignment.ValueChanged += (sender, args) => Align();
            Settings.Tint.ValueChanged += (sender, args) => Align();
            Settings.Inverted.ValueChanged += (sender, args) => Align();
        }

        private void CreateFlag() => Flag = new Sprite
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            Image = Flags.Get(Player.User.OnlineUser.CountryFlag)
        };

        private void CreateUsername() => TextUsername = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
            Player.User.OnlineUser.Username)
        {
            Parent = this,
            Alignment = Alignment.MidLeft
        };

        private void Align() => ScheduleUpdate(() =>
        {
            Visible = Settings.Visible.Value;

            if (!string.IsNullOrEmpty(Settings.Font.Value) && FontManager.WobbleFonts.ContainsKey(Settings.Font.Value))
                TextUsername.Font = FontManager.GetWobbleFont(Settings.Font.Value);
            else
                TextUsername.Font = FontManager.GetWobbleFont(Settings.Font.Default);

            const int spacing = 12;
            TextUsername.FontSize = Settings.FontSize.Value;
            Flag.Size = new ScalableVector2(Settings.FontSize.Value, Settings.FontSize.Value);

            if (!Settings.Inverted.Value)
            {
                Flag.Alignment = Alignment.MidLeft;
                TextUsername.Alignment = Alignment.MidLeft;
                TextUsername.X = Flag.Width + spacing;
            }
            else
            {
                Flag.Alignment = Alignment.MidRight;
                TextUsername.Alignment = Alignment.MidRight;
                TextUsername.X = -Flag.Width - spacing;
            }

            Height = Math.Max(Flag.Height, TextUsername.Height);
            Width = Flag.Width + spacing + TextUsername.Width;

            Position = new ScalableVector2(Settings.Position.Value.X, Settings.Position.Value.Y);
            Alignment = Settings.Alignment.Value;
            TextUsername.Tint = Settings.Tint.Value;
        });
    }
}