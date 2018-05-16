using Microsoft.Xna.Framework.Graphics;

namespace Quaver.Main
{
    internal static class QuaverFonts
    {
        public static SpriteFont Medium12 { get; set; }
        public static SpriteFont Medium16 { get; set; }
        public static SpriteFont Medium24 { get; set; }
        public static SpriteFont Medium48 { get; set; }
        public static SpriteFont Bold12 { get; set; } 
        public static SpriteFont AssistantLight16 { get; set; }
        public static SpriteFont AssistantRegular16 { get; set; }

        /// <summary>
        ///     Loads all the fonts for the game.
        /// </summary>
        internal static void Load()
        {
            Medium12 = GameBase.Content.Load<SpriteFont>("Medium12");
            Medium16 = GameBase.Content.Load<SpriteFont>("Medium16");
            Medium24 = GameBase.Content.Load<SpriteFont>("Medium24");
            Medium48 = GameBase.Content.Load<SpriteFont>("Medium48");
            Bold12 = GameBase.Content.Load<SpriteFont>("Bold12");
            AssistantLight16 = GameBase.Content.Load<SpriteFont>("assistant_light_16");
            AssistantRegular16 = GameBase.Content.Load<SpriteFont>("assistant_regular_16");
        }
    }
}
