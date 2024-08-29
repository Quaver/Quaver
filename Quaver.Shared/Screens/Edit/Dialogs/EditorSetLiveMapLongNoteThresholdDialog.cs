using System;
using System.Text.RegularExpressions;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorSetLiveMapLongNoteThresholdDialog : YesNoDialog
    {
        private EditScreen Screen { get; }

        /// <summary>
        /// </summary>
        protected Textbox Textbox { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorSetLiveMapLongNoteThresholdDialog(EditScreen screen) : base("SET LIVEMAP LN THRESHOLD",
            "Enter the minimum length of a long note during live-mapping...")
        {
            Screen = screen;
            CreateTextbox();

            Panel.Height += 50;
            YesButton.Y = -30;
            NoButton.Y = YesButton.Y;

            YesButton.Clicked += (sender, args) => OnSubmit(Textbox.RawText);
        }

        /// <summary>
        /// </summary>
        private void CreateTextbox()
        {
            Textbox = new Textbox(new ScalableVector2(Panel.Width * 0.90f, 50),
                FontManager.GetWobbleFont(Fonts.LatoBlack),
                20, ConfigManager.EditorLiveMapLongNoteThreshold.Value.ToString(),
                "Enter a threshold of LNs during livemapping...", OnSubmit)
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Y = -100,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                AlwaysFocused = true,
                AllowedCharacters = new Regex(@"^[0-9]*$")
            };

            Textbox.AddBorder(ColorHelper.HexToColor("#363636"), 2);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Close()
        {
            Textbox.Visible = false;
            base.Close();
        }

        private void OnSubmit(string s)
        {
            if (!int.TryParse(s, out var val))
            {
                NotificationManager.Show(NotificationLevel.Error, $"Invalid value: '{s}'");
                return;
            }

            ConfigManager.EditorLiveMapLongNoteThreshold.Value = val;
        }
    }
}