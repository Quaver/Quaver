using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs
{
    public class CopyPlaylistDialog : YesNoDialog
    {
        /// <summary>
        ///     The playlist being copied.
        /// </summary>
        private Playlist Playlist { get; }

        /// <summary>
        ///     The selected playlist type for the copy.
        /// </summary>
        private LabelledDropdown PlaylistTypeDropdown { get; set; }

        public CopyPlaylistDialog(Playlist playlist) : base("Copy Playlist", "Choose what type this playlist should be copied as.")
        {
            Playlist = playlist;

            Panel.Height = 330;
            Panel.Image = UserInterface.BlankBox;
            Panel.Tint = ColorHelper.HexToColor("#242424");
            Panel.AddBorder(ColorHelper.HexToColor("#0FBAE5"), 2);

            CreatePlaylistTypeDropdown();

            YesButton.Y = -30;
            YesButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "COPY", 20, Color.White);
            NoButton.Y = YesButton.Y;

            YesAction += OnCopyClicked;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            YesButton.Depth = PlaylistTypeDropdown.Dropdown.Opened ? 1 : 0;
            NoButton.Depth = YesButton.Depth;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (DialogManager.Dialogs.First() == this && KeyboardManager.IsUniqueKeyPress(Keys.Escape))
            {
                NoAction?.Invoke();
                Close();
                return;
            }

            base.HandleInput(gameTime);
        }

        /// <summary>
        ///     Creates <see cref="PlaylistTypeDropdown"/>.
        /// </summary>
        private void CreatePlaylistTypeDropdown()
        {
            PlaylistTypeDropdown = new LabelledDropdown("Type", 21, new Dropdown(new List<string> { "Normal", "Tournament" }, new ScalableVector2(166, 35), 20, selectedIndex: (int)Playlist.Type))
            {
                Parent = Panel,
                Alignment = Alignment.TopLeft,
                X = 60,
                Y = Banner.Height + 20
            };
        }

        /// <summary>
        ///     Copies the playlist as the selected playlist type.
        /// </summary>
        private void OnCopyClicked() => PlaylistManager.CopyPlaylist(Playlist, (PlaylistType)PlaylistTypeDropdown.Dropdown.SelectedIndex);
    }
}
