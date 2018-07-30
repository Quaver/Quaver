using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Resources;
using Wobble;
using Wobble.Assets;

namespace Quaver.Assets
{
    public static class Fonts
    {
        public static SpriteFont AssistantLight16 { get; set; }
        public static SpriteFont AssistantRegular16 { get; set; }
        public static SpriteFont RationalInteger16 { get; set; }
        public static SpriteFont GoodTimes16 { get; set; }
        public static SpriteFont AllerRegular16 { get; set; }
        public static SpriteFont AllerLight16 { get; set; }
        public static SpriteFont AllerBold16 { get; set; }
        public static SpriteFont Exo2Regular24 { get; set; }

        /// <summary>
        ///     Loads all the fonts for the game.
        /// </summary>
        internal static void Load()
        {
            AssistantLight16 = AssetLoader.LoadFont(QuaverResources.ResourceManager, "assistant_light_16")
                ;
            AssistantLight16 = AssetLoader.LoadFont(QuaverResources.ResourceManager, "assistant_light_16");
            AssistantRegular16 = AssetLoader.LoadFont(QuaverResources.ResourceManager, "assistant_regular_16");
            RationalInteger16 = AssetLoader.LoadFont(QuaverResources.ResourceManager, "rational_integer_16");
            GoodTimes16 = AssetLoader.LoadFont(QuaverResources.ResourceManager, "good_times_16");
            AllerRegular16 = AssetLoader.LoadFont(QuaverResources.ResourceManager, "aller_regular_16");
            AllerLight16 = AssetLoader.LoadFont(QuaverResources.ResourceManager, "aller_light_16");
            AllerBold16 = AssetLoader.LoadFont(QuaverResources.ResourceManager, "aller_bold_16");
            Exo2Regular24 = AssetLoader.LoadFont(QuaverResources.ResourceManager, "exo2_regular_24");
        }
    }
}
