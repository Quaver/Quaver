using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Settings;
using Quaver.Shared.Screens.Theater;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonTheater: IconTextButton
    {
        public IconTextButtonTheater() : base(FontAwesome.Get(FontAwesomeIcon.fa_photo_camera),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Theater", (sender, args) =>
            {
                var game = (QuaverGame) GameBase.Game;
                game.CurrentScreen.Exit(() => new TheaterScreen());
            })
        {
        }
    }
}