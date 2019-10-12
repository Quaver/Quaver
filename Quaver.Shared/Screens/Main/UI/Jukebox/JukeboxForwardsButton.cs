using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Assets;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Main.UI.Jukebox
{
    public class JukeboxForwardsButton: IconButton
    {
        public JukeboxForwardsButton(FooterJukebox jukebox) : base(UserInterface.JukeboxBackwardButton,(o, e) => jukebox.SelectNextTrack(Direction.Forward))
        {
            SpriteEffect = SpriteEffects.FlipHorizontally;
        }
    }
}