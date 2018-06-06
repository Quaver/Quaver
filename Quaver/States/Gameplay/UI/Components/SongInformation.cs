
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.Modifiers;

namespace Quaver.States.Gameplay.UI.Components
{
    /// <inheritdoc />
    /// <summary>
    ///     Sprite that displays the song's information, displayed at the beginning of the play session.
    /// </summary>
    internal class SongInformation : Sprite
    {
        /// <summary>
        /// Reference to the actual gameplay screen.
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     The title of the song
        /// </summary>
        private SpriteText Title { get; }

        /// <summary>
        ///     The difficulty name of the song
        /// </summary>
        private SpriteText Difficulty { get; }

        /// <summary>
        ///     The creator of the map
        /// </summary>
        private SpriteText Creator { get; }

        /// <summary>
        ///     The map's difficulty rating
        /// </summary>
        private SpriteText Rating { get; }
        
        /// <summary>
        ///     The activated mods.
        /// </summary>
        private SpriteText Mods { get; }

        /// <summary>
        ///     The amount of spacing between each piece of text.
        /// </summary>
        private int TextYSpacing { get; } = 16;

        /// <summary>
        ///     The scale used for fade animations.
        /// </summary>
        private static float AnimationScale => 120f / GameBase.AudioEngine.PlaybackRate;

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="screen"></param>
        internal SongInformation(GameplayScreen screen)
        {
            Screen = screen;
            
            Size = new UDim2D(750, 150);
            Tint = QuaverColors.MainAccentInactive;
            Alpha = 0;

            Title = new SpriteText
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Text = $"{Screen.Map.Artist} - \"{Screen.Map.Title}\"",
                Font = QuaverFonts.AssistantRegular16,
                PosY = 15,
                Alpha = 0
            };

            Difficulty = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Text = $"[{Screen.Map.DifficultyName}]",
                Font = QuaverFonts.AssistantRegular16,
                PosY = Title.PosY + TextYSpacing + TextYSpacing * 0.90f,
                TextScale = 0.90f,
                Alpha = 0
            };

            Creator = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Text = $"Mapped By: \"{Screen.Map.Creator}\"",
                Font = QuaverFonts.AssistantRegular16,
                PosY = Difficulty.PosY + TextYSpacing + TextYSpacing * 0.80f,
                TextScale = 0.80f,
                Alpha = 0
            };

            Rating = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Text = $"Rating: {StringHelper.AccuracyToString(Screen.Map.CalculateFakeDifficulty(GameBase.AudioEngine.PlaybackRate)).Replace("%", "")}",
                Font = QuaverFonts.AssistantRegular16,
                PosY = Creator.PosY + TextYSpacing + TextYSpacing * 0.75f,
                TextScale = 0.75f,
                Alpha = 0,
                TextColor = ColorHelper.DifficultyToColor(Screen.Map.CalculateFakeDifficulty(GameBase.AudioEngine.PlaybackRate))
            };

            // Get a formatted string of the activated mods.
            var modsString = "Mods: " + (GameBase.CurrentGameModifiers.Count > 0 ? $"{ModHelper.GetActivatedModsString()}" :  "None");         
            Mods = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Text = modsString,
                Font = QuaverFonts.AssistantRegular16,
                PosY = Rating.PosY + TextYSpacing + TextYSpacing * 0.7f,
                TextScale = 0.7f,
                Alpha = 0
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            // Fade in on map start
            if (Screen.Timing.CurrentTime < -500)
            {
                Title.FadeIn(dt, AnimationScale);
                Difficulty.FadeIn(dt, AnimationScale);
                Creator.FadeIn(dt, AnimationScale);
                Rating.FadeIn(dt, AnimationScale);
                Mods.FadeIn(dt, AnimationScale);
            }
            else
            {
                Title.FadeOut(dt, AnimationScale);
                Difficulty.FadeOut(dt, AnimationScale);
                Creator.FadeOut(dt, AnimationScale);
                Rating.FadeOut(dt, AnimationScale);
                Mods.FadeOut(dt, AnimationScale);
            }
            
            base.Update(dt);
        }
    }
}