using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multi.UI.Footer
{
    public class IconTextButtonMultiplayerCommands : IconTextButton
    {
        public IconTextButtonMultiplayerCommands() : base(
            FontAwesome.Get(FontAwesomeIcon.fa_speech_bubbles_comment_option),
            FontManager.GetWobbleFont(Fonts.LatoBlack), "Commands",
            (sender, args) => { BrowserHelper.OpenURL($"https://quavergame.com/wiki/Multiplayer/Commands"); })
        {
        }
    }
}