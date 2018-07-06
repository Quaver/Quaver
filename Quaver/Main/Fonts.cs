using Microsoft.Xna.Framework.Graphics;

namespace Quaver.Main
{
    internal static class Fonts
    {
        public static SpriteFont Medium12 { get; set; }
        public static SpriteFont Medium16 { get; set; }
        public static SpriteFont Medium24 { get; set; }
        public static SpriteFont Medium48 { get; set; }
        public static SpriteFont Bold12 { get; set; } 
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
            Medium12 = GameBase.Content.Load<SpriteFont>("Medium12");
            Medium16 = GameBase.Content.Load<SpriteFont>("Medium16");
            Medium24 = GameBase.Content.Load<SpriteFont>("Medium24");
            Medium48 = GameBase.Content.Load<SpriteFont>("Medium48");
            Bold12 = GameBase.Content.Load<SpriteFont>("Bold12");
            AssistantLight16 = GameBase.Content.Load<SpriteFont>("assistant_light_16");
            AssistantRegular16 = GameBase.Content.Load<SpriteFont>("assistant_regular_16");
            RationalInteger16 = GameBase.Content.Load<SpriteFont>("rational_integer_16");
            GoodTimes16 = GameBase.Content.Load<SpriteFont>("good_times_16");
            AllerRegular16 = GameBase.Content.Load<SpriteFont>("aller_regular_16");
            AllerLight16 = GameBase.Content.Load<SpriteFont>("aller_light_16");
            AllerBold16 = GameBase.Content.Load<SpriteFont>("aller_bold_16");
            Exo2Regular24 = GameBase.Content.Load<SpriteFont>("exo2_regular_24");
        }
    }
}
