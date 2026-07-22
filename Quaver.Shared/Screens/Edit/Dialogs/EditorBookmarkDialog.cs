using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Audio.Tracks;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorBookmarkDialog : YesNoDialog
    {
        private const int ContentHorizontalPadding = 24;

        private const int ColorControlSpacing = 10;

        private const int ControlHeight = 50;

        private EditorActionManager ActionManager { get; }

        private IAudioTrack Track { get; }

        /// <summary>
        ///     The bookmark that's currently being edited. If none is provided in the constructor,
        ///     then the purpose of the dialog will be to add a new one.
        /// </summary>
        private BookmarkInfo EditingBookmark { get; }

        protected Textbox Textbox { get; set; }

        private Textbox ColorTextbox { get; set; }

        private ImageButton ColorBox { get; set; }

        private Random RNG { get; } = new Random();

        private Color BookmarkColor { get; set; }

        public EditorBookmarkDialog(EditorActionManager manager, IAudioTrack track, BookmarkInfo editingBookmark)
            : base(LocalizationManager.Get(editingBookmark == null
                    ? "Screen_Editor_AddBookmark"
                    : "Screen_Editor_EditBookmark"),
                LocalizationManager.Get("Screen_Editor_BookmarkDialogMessage"))
        {
            ActionManager = manager;
            Track = track;
            EditingBookmark = editingBookmark;
            BookmarkColor = EditingBookmark == null
                ? Color.Yellow
                : ColorHelper.ToXnaColor(EditingBookmark.GetColor());

            CreateTextbox();
            CreateColorTextbox();
            CreateColorBox();
            CreateRandomButton();
            UpdateColor(BookmarkColor);

            Panel.Height += 110;
            YesButton.Y = -30;
            NoButton.Y = YesButton.Y;

            YesAction += () => OnSubmit(Textbox.RawText, ColorTextbox.RawText);
            ValidateBeforeClosing = () => ParseColor(ColorTextbox.RawText).HasValue;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Close()
        {
            Textbox.Visible = false;
            ColorTextbox.Visible = false;
            ColorBox.Visible = false;
            base.Close();
        }

        private void CreateTextbox()
        {
            Textbox = new Textbox(new ScalableVector2(Panel.Width - ContentHorizontalPadding * 2, ControlHeight),
                FontManager.GetWobbleFont(Fonts.InterBold),
                20, EditingBookmark?.Note ?? "", LocalizationManager.Get("Screen_Editor_BookmarkNotePlaceholder"),
                note => OnSubmit(note, ColorTextbox.RawText))
            {
                Parent = Panel,
                Alignment = Alignment.BotLeft,
                Y = -160,
                X = ContentHorizontalPadding,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                Focused = true,
                AllowSubmission = false
            };

            Textbox.AddBorder(ColorHelper.HexToColor("#363636"), 2);
        }

        private void CreateColorTextbox()
        {
            var width = Panel.Width - ContentHorizontalPadding * 2 - ColorControlSpacing - ControlHeight;

            ColorTextbox = new Textbox(new ScalableVector2(width, ControlHeight),
                FontManager.GetWobbleFont(Fonts.InterBold), 20, "",
                LocalizationManager.Get("Screen_Editor_RgbColorPlaceholder"), null)
            {
                Parent = Panel,
                Alignment = Alignment.BotLeft,
                Y = -100,
                X = ContentHorizontalPadding,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                StoppedTypingActionCalltime = 100,
                AllowSubmission = false
            };

            ColorTextbox.OnStoppedTyping += value =>
            {
                var color = ParseColor(value);
                if (color.HasValue)
                    UpdateColor(color.Value);
            };

            ColorTextbox.AddBorder(ColorHelper.HexToColor("#363636"), 2);
        }

        private void CreateColorBox()
        {
            ColorBox = new IconButton(UserInterface.BlankBox)
            {
                Parent = Panel,
                Alignment = Alignment.BotRight,
                Y = ColorTextbox.Y,
                X = -ContentHorizontalPadding,
                Tint = BookmarkColor,
                Size = new ScalableVector2(ControlHeight, ControlHeight)
            };

            ColorBox.Clicked += (sender, args) => UpdateColor(new Color(
                RNG.Next(byte.MaxValue), RNG.Next(byte.MaxValue), RNG.Next(byte.MaxValue)));
        }

        private void CreateRandomButton()
        {
            new Sprite
            {
                Parent = ColorBox,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(0, 0, 0.45f, 0.45f),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_refresh_arrow),
                Tint = Color.Black
            };
        }

        private void UpdateColor(Color color)
        {
            BookmarkColor = color;
            ColorBox.Tint = color;
            ColorTextbox.RawText = $"{color.R},{color.G},{color.B}";
            ColorTextbox.InputText.Text = ColorTextbox.RawText;
        }

        private static Color? ParseColor(string value)
        {
            var split = value.Split(',');

            try
            {
                return new Color(byte.Parse(split[0]), byte.Parse(split[1]), byte.Parse(split[2]));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void OnSubmit(string note, string colorRgb)
        {
            var color = ParseColor(colorRgb);
            if (!color.HasValue)
                return;

            var normalizedColorRgb = $"{color.Value.R},{color.Value.G},{color.Value.B}";

            if (EditingBookmark != null)
            {
                ActionManager.EditBookmark(EditingBookmark, note);

                if (EditingBookmark.ColorRgb != normalizedColorRgb)
                    ActionManager.ChangeBookmarkColorBatch(new List<BookmarkInfo> { EditingBookmark }, color.Value);

                return;
            }

            ActionManager.AddBookmark(new BookmarkInfo
            {
                StartTime = (int)Track.Time,
                Note = note,
                ColorRgb = normalizedColorRgb
            });
        }
    }
}
