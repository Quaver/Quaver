using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public abstract class ColorDialog : YesNoDialog
    {
        protected Textbox Textbox { get; set; }

        protected ImageButton ColorBox { get; set; }

        protected Sprite RandomButton { get; set; }

        private Random RNG { get; } = new Random();

        private Color NewColor { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ColorDialog(string header = "SELECT COLOR", string confirmationText = "Enter a new RGB color...") : base(header, confirmationText)
        {
            CreateTextbox();
            CreateColorBox();
            CreateRandomButton();

            Panel.Height += 50;
            YesButton.Y = -30;
            NoButton.Y = YesButton.Y;

            YesButton.Clicked += (sender, args) => OnSubmit(Textbox.RawText);
        }

        /// <summary>
        /// </summary>
        private void CreateTextbox()
        {
            var color = NewColor;
            var val = $"{color.R},{color.G},{color.B}";

            Textbox = new Textbox(new ScalableVector2(Panel.Width * 0.86f, 50), FontManager.GetWobbleFont(Fonts.LatoBlack),
                20, val, "Enter RGB color (ex: 255,255,255)...", OnSubmit)
            {
                Parent = Panel,
                Alignment = Alignment.BotLeft,
                Y = -100,
                X = 24,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                AlwaysFocused = true,
                StoppedTypingActionCalltime = 100
            };

            Textbox.OnStoppedTyping += s =>
            {
                var col = ParseColor(s);
                if (col.HasValue)
                    UpdateColor(col.Value);
            };

            Textbox.AddBorder(ColorHelper.HexToColor("#363636"), 2);
        }

        private void CreateColorBox()
        {
            ColorBox = new IconButton(UserInterface.BlankBox)
            {
                Parent = Panel,
                Alignment = Alignment.BotRight,
                Y = Textbox.Y,
                X = -Textbox.X,
                Tint = NewColor,
                Size = new ScalableVector2(Textbox.Height, Textbox.Height)
            };

            ColorBox.Clicked += (sender, args) =>
            {
                var col = new Color(RNG.Next(255), RNG.Next(255), RNG.Next(255));
                UpdateColor(col);
            };
        }

        private void CreateRandomButton()
        {
            RandomButton = new Sprite()
            {
                Parent = ColorBox,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(0, 0, 0.45f, 0.45f),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_refresh_arrow),
                Tint = Color.Black
            };
        }

        public void UpdateColor(Color c)
        {
            NewColor = c;
            ColorBox.Tint = c;

            Textbox.RawText = $"{c.R},{c.G},{c.B}";
            Textbox.InputText.Text = Textbox.RawText;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Close()
        {
            Textbox.Visible = false;
            ColorBox.Visible = false;

            base.Close();
        }

        private Color? ParseColor(string c)
        {
            var split = c.Split(',');

            try
            {
                return new Color(byte.Parse(split[0]), byte.Parse(split[1]), byte.Parse(split[2]));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void OnSubmit(string s)
        {
            var color = ParseColor(s);
            if (!color.HasValue)
                return;

            OnColorChange(color.Value);
        }

        protected abstract void OnColorChange(Color newColor);
    }
}
