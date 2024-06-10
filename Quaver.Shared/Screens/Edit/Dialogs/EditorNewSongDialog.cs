using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorNewSongDialog : YesNoDialog
    {
        public EditorNewSongDialog() : base("CREATE NEW MAPSET",
            "To create a new mapset, drag a .mp3 file into the game window...")
        {
            YesButton.Visible = false;
            YesButton.IsClickable = false;

            NoButton.Alignment = Alignment.BotCenter;
            NoButton.X = 0;

            GameBase.Game.Window.FileDrop += OnFileDropped;
        }

        public override void Destroy()
        {
            GameBase.Game.Window.FileDrop -= OnFileDropped;
            base.Destroy();
        }

        private void OnFileDropped(object sender, FileDropEventArgs e)
        {
            if (e.Files.Length < 1)
                return;

            var file = e.Files[0].ToLower();

            if (!file.EndsWith(".mp3") && !file.EndsWith(".ogg"))
                return;

            EditScreen.CreateNewMapset(e.Files[0]);
            DialogManager.Dismiss(this);
        }
    }
}
