using System;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Modifiers;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Components
{
    public class DrawableModifier : Sprite
    {
        public DrawableModifier(ModIdentifier mod)
        {
            if (mod == ModIdentifier.LongNoteAdjust)
            {
                Image = TextureManager.Load($@"Quaver.Resources/Textures/UI/Mods/N-JW.png");
                return;
            }

            Image = ModManager.GetTexture(mod);
        }
    }
}