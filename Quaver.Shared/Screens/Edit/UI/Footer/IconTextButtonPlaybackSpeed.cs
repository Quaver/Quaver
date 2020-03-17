using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Editor;
using Wobble.Audio.Tracks;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Footer
{
    public class IconTextButtonPlaybackSpeed : IconTextButton
    {
        public IconTextButtonPlaybackSpeed(EditScreen screen, IAudioTrack track) : base(FontAwesome.Get(FontAwesomeIcon.fa_time),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Playback Speed", (sender, args) =>
            {
                screen?.ActivateRightClickOptions(new PlaybackSpeedRightClickOptions(track));
            })
        {
            var tooltip = new Tooltip("Change the audio playback rate.\n" +
                                      "Hotkeys: CTRL + -/+", ColorHelper.HexToColor("#808080"));

            Hovered += (sender, args) => screen?.ActivateTooltip(tooltip);
            LeftHover += (sender, args) => screen?.DeactivateTooltip();
        }
    }
}