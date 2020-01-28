using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Downloading.UI.Filter
{
    public class DownloadFilterTextbox : Textbox
    {
        public DownloadFilterTextbox(WobbleFontStore font, int fontSize, string initialText = "", string placeHolderText = "", Action<string> onSubmit = null, Action<string> onStoppedTyping = null)
            : base(new ScalableVector2(122, 32), font, fontSize, initialText, placeHolderText, onSubmit, onStoppedTyping)
        {
            AllowSubmission = false;

            Tint = ColorHelper.HexToColor("#2F2F2F");
            AddBorder(ColorHelper.HexToColor("#5B5B5B"), 2);
            StoppedTypingActionCalltime = 250;
        }
    }
}