using System;
using Quaver.Shared.Screens.V2.UI;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.Modal
{
    internal sealed class OptionsSkinConfigOverwriteDialog : ModalDialog
    {
        public OptionsSkinConfigOverwriteDialog(Action onConfirmed, Action onClosed = null) : base(LocalizationManager.Get("Screen_Options_OverwriteSkinConfig"), LocalizationManager.Get("Screen_Options_OverwriteSkinConfigDescription"), onClosed)
        {
            if (onConfirmed == null)
                throw new ArgumentNullException(nameof(onConfirmed));

            AddButton(LocalizationManager.Get("SkinEditor_Overwrite"), V2ModalButtonStyle.Confirm, modal =>
                {
                    modal.Close();
                    onConfirmed();
                });
            AddButton(LocalizationManager.Get("SkinEditor_Cancel"), V2ModalButtonStyle.Cancel, modal => modal.Close());
        }
    }
}
