using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Wobble;
using Wobble.Assets;

namespace Quaver.Resources
{
    public static class Flags
    {
        public static Texture2D Get(string countryName)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            return AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get($"Quaver.Resources/Textures/UI/Flags/{countryName.Replace(" ", "-")}.png"));
        }
    }
}
