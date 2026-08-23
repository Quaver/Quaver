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
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorBookmarkDialog : YesNoDialog
    {
        private const int ContentHorizontalPadding = 24;

        private const int ControlHeight = 50;

        private EditorActionManager ActionManager { get; }

        private IAudioTrack Track { get; }

        /// <summary>
        ///     The bookmark that's currently being edited. If none is provided in the constructor,
        ///     then the purpose of the dialog will be to add a new one.
        /// </summary>
        private BookmarkInfo EditingBookmark { get; }

        protected Textbox Textbox { get; set; }

        private RoundedButton ColorButton { get; set; }

        private Sprite ColorSwatch { get; set; }

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
            CreateColorButton();
            UpdateColor(BookmarkColor);

            Panel.Height += 110;
            YesButton.Y = -30;
            NoButton.Y = YesButton.Y;

            YesAction += () => OnSubmit(Textbox.RawText);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Close()
        {
            Textbox.Visible = false;
            ColorButton.Visible = false;
            base.Close();
        }

        private void CreateTextbox()
        {
            Textbox = new Textbox(new ScalableVector2(Panel.Width - ContentHorizontalPadding * 2, ControlHeight),
                FontManager.GetWobbleFont(Fonts.InterSemiBold),
                20, EditingBookmark?.Note ?? "", LocalizationManager.Get("Screen_Editor_BookmarkNotePlaceholder"),
                OnSubmit)
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

        private void CreateColorButton()
        {
            ColorButton = new RoundedButton((sender, args) =>
            {
                DialogManager.Show(new BookmarkColorDialog(BookmarkColor, UpdateColor));
            })
            {
                Parent = Panel,
                Alignment = Alignment.BotLeft,
                Position = new ScalableVector2(ContentHorizontalPadding, -100),
                Size = new ScalableVector2(Panel.Width - ContentHorizontalPadding * 2, ControlHeight),
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                CornerRadius = 6
            };

            ColorButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterSemiBold),
                LocalizationManager.Get("Screen_Editor_ChangeColor"), 20, Color.White);

            ColorSwatch = new Sprite
            {
                Parent = ColorButton,
                Alignment = Alignment.MidRight,
                X = -12,
                Size = new ScalableVector2(30, 30),
                Image = UserInterface.BlankBox,
                Tint = BookmarkColor
            };
        }

        private void UpdateColor(Color color)
        {
            BookmarkColor = color;
            ColorSwatch.Tint = color;
        }

        private void OnSubmit(string note)
        {
            var normalizedColorRgb = $"{BookmarkColor.R},{BookmarkColor.G},{BookmarkColor.B}";

            if (EditingBookmark != null)
            {
                ActionManager.EditBookmark(EditingBookmark, note);

                if (EditingBookmark.ColorRgb != normalizedColorRgb)
                    ActionManager.ChangeBookmarkColorBatch(new List<BookmarkInfo> { EditingBookmark }, BookmarkColor);

                return;
            }

            ActionManager.AddBookmark(new BookmarkInfo
            {
                StartTime = (int)Track.Time,
                Note = note,
                ColorRgb = normalizedColorRgb
            });
        }

        private sealed class BookmarkColorDialog : ColorDialog
        {
            private readonly System.Action<Color> changed;

            public BookmarkColorDialog(Color initialColor, System.Action<Color> onChanged)
                : base(LocalizationManager.Get("Screen_Editor_SelectColor"))
            {
                changed = onChanged;
                UpdateColor(initialColor);
            }

            protected override void OnColorChange(Color newColor) => changed(newColor);
        }
    }
}
