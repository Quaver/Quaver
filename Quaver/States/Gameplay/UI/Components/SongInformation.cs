
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
    
    internal class SongInformation : Sprite
    {
        private GameplayScreen Screen { get; }

        private SpriteText Title { get; }

        private SpriteText Difficulty { get; }

        private SpriteText Creator { get; }

        private SpriteText QSS { get; }
        
        private SpriteText Mods { get; }

        private int TextYSpacing { get; } = 16;

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

            QSS = new SpriteText()
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

            var modsString = "Mods: " + (GameBase.CurrentGameModifiers.Count > 0 ? $"{ModHelper.GetActivatedModsString()}" :  "None");
            
            Mods = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Text = modsString,
                Font = QuaverFonts.AssistantRegular16,
                PosY = QSS.PosY + TextYSpacing + TextYSpacing * 0.7f,
                TextScale = 0.7f,
                Alpha = 0
            };
        }

        internal override void Update(double dt)
        {
            // Fade in 
            if (Screen.Timing.CurrentTime < -500)
            {
                Fade(dt, 0.75f, 120 / GameBase.AudioEngine.PlaybackRate);
                Title.FadeIn(dt, 120 / GameBase.AudioEngine.PlaybackRate);
                Difficulty.FadeIn(dt, 120/ GameBase.AudioEngine.PlaybackRate);
                Creator.FadeIn(dt, 120 / GameBase.AudioEngine.PlaybackRate);
                QSS.FadeIn(dt, 120 / GameBase.AudioEngine.PlaybackRate);
                Mods.FadeIn(dt, 120 / GameBase.AudioEngine.PlaybackRate);
            }
            else
            {
                FadeOut(dt, 120 / GameBase.AudioEngine.PlaybackRate);
                Title.FadeOut(dt, 120 / GameBase.AudioEngine.PlaybackRate);
                Difficulty.FadeOut(dt, 120 / GameBase.AudioEngine.PlaybackRate);
                Creator.FadeOut(dt, 120 / GameBase.AudioEngine.PlaybackRate);
                QSS.FadeOut(dt, 120 / GameBase.AudioEngine.PlaybackRate);
                Mods.FadeOut(dt, 120 / GameBase.AudioEngine.PlaybackRate);
            }
            
            base.Update(dt);
        }
    }
}