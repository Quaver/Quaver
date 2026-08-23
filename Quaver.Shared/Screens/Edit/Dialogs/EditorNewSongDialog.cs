using System;
using Quaver.Shared.Graphics;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorNewSongDialog : YesNoDialog
    {
        public EditorNewSongDialog() : base(LocalizationManager.Get("Screen_Editor_CreateNewMapset"),
            LocalizationManager.Get("Screen_Editor_CreateNewMapsetMessage"))
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
            var file = e.ToLower();

            if (!file.EndsWith(".mp3") && !file.EndsWith(".ogg"))
                return;

            EditScreen.CreateNewMapset(e);
            DialogManager.Dismiss(this);
        }
    }
}
