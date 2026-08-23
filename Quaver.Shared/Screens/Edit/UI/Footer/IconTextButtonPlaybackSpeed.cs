using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Input;
using Quaver.Shared.Screens.Editor;
using Wobble.Audio.Tracks;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Footer
{
    public class IconTextButtonPlaybackSpeed : IconTextButton
    {
        public IconTextButtonPlaybackSpeed(EditScreen screen, IAudioTrack track) : base(FontAwesome.Get(FontAwesomeIcon.fa_time),
            FontManager.GetWobbleFont(Fonts.InterSemiBold), LocalizationManager.Get("Screen_Editor_PlaybackSpeed"), (sender, args) =>
            {
                screen?.ActivateRightClickOptions(new PlaybackSpeedRightClickOptions(track));
            })
        {
            Hovered += (sender, args) => screen?.ActivateTooltip(new Tooltip(
                LocalizationManager.Get("Screen_Editor_PlaybackSpeedTooltip",
                    screen.InputManager.InputConfig.GetOrDefault(EditorKeybindActions.DecreasePlaybackRate).ToDisplayString(),
                    screen.InputManager.InputConfig.GetOrDefault(EditorKeybindActions.IncreasePlaybackRate).ToDisplayString()),
                ColorHelper.HexToColor("#808080")));
            LeftHover += (sender, args) => screen?.DeactivateTooltip();
        }
    }
}
