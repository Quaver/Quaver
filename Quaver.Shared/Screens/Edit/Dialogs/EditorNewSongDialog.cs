using System;
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

            GameBase.Game.Window.FileDropped += OnFileDropped;
        }

        public override void Destroy()
        {
            GameBase.Game.Window.FileDropped -= OnFileDropped;
            base.Destroy();
        }

        private void OnFileDropped(object sender, string e)
        {
            // If e is a file:// URI (for example, on Wayland), it needs to be converted to a local path. If it's
            // already a local path, this function leaves it as is.
            e = new Uri(e).LocalPath;

            var file = e.ToLower();

            if (!file.EndsWith(".mp3") && !file.EndsWith(".ogg"))
                return;

            EditScreen.CreateNewMapset(e);
            DialogManager.Dismiss(this);
        }
    }
}
