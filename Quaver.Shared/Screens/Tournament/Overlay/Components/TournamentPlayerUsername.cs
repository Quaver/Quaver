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
        /// <summary>
        /// </summary>
        private TournamentPlayer Player { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> DisplayPlayerNames { get; }

        /// <summary>
        /// </summary>
        private BindableInt FontSize { get; }

        /// <summary>
        /// </summary>
        private Bindable<Vector2> UsernamePosition { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> InvertFlagAndUsername { get; }

        /// <summary>
        /// </summary>
        private Bindable<Alignment> Aligning { get; }

        /// <summary>
        /// </summary>
        private Bindable<Color> Color { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus TextUsername { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Flag { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="displayPlayerNames"></param>
        /// <param name="fontSize"></param>
        /// <param name="position"></param>
        /// <param name="invertFlagAndUsername"></param>
        /// <param name="textAlignment"></param>
        /// <param name="col"></param>
        public TournamentPlayerUsername(TournamentPlayer player, Bindable<bool> displayPlayerNames, BindableInt fontSize,
            Bindable<Vector2> position, Bindable<bool> invertFlagAndUsername, Bindable<Alignment> textAlignment, Bindable<Color> col)
        {
            Player = player;
            DisplayPlayerNames = displayPlayerNames;
            FontSize = fontSize;
            UsernamePosition = position;
            InvertFlagAndUsername = invertFlagAndUsername;
            Aligning = textAlignment;
            Color = col;

            CreateFlag();
            CreateUsername();
            Align();

            DisplayPlayerNames.ValueChanged += (sender, args) => Align();
            FontSize.ValueChanged += (sender, args) => Align();
            UsernamePosition.ValueChanged += (sender, args) => Align();
            InvertFlagAndUsername.ValueChanged += (sender, args) => Align();
            Aligning.ValueChanged += (sender, args) => Align();
            Color.ValueChanged += (sender, args) => Align();
        }

        /// <summary>
        /// </summary>
        private void CreateFlag()
        {
            Flag = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(FontSize.Value, FontSize.Value),
                Image = Flags.Get(Player.User.OnlineUser.CountryFlag)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateUsername()
        {
            TextUsername = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), Player.User.OnlineUser.Username, FontSize.Value)
            {
                Parent = this,
                Alignment = Alignment.MidLeft
            };
        }

        /// <summary>
        /// </summary>
        private void Align() => ScheduleUpdate(() =>
        {
            Visible = DisplayPlayerNames.Value;

            const int spacing = 12;
            TextUsername.FontSize = FontSize.Value;
            Flag.Size = new ScalableVector2(FontSize.Value, FontSize.Value);

            if (!InvertFlagAndUsername.Value)
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

            Position = new ScalableVector2(UsernamePosition.Value.X, UsernamePosition.Value.Y);
            Alignment = Aligning.Value;
            TextUsername.Tint = Color.Value;
        });
    }
}