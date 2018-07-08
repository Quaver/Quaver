
using System;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics;
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
        ///     Text that says "watching" when watching a replay.
        /// </summary>
        private SpriteText Watching { get; }

        /// <summary>
        ///     The player's name of the replay that we're watching.
        /// </summary>
        private SpriteText PlayerName { get; }

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
            Tint = Colors.MainAccentInactive;
            Alpha = 0;

            // Replay
            const float replayTextScale = 0.95f;
            
            // Create watching text outside of replay mode because other text relies on it.
            Watching = new SpriteText
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Text = "Watching",
                Font = Fonts.AssistantRegular16,
                PosY = 0,
                TextScale = replayTextScale,
                Alpha = 0
            };
            
            // Handle positioning and create player name text if we're actually
            // watching a replay.
            if (Screen.InReplayMode)
            {
                PlayerName = new SpriteText
                {
                    Parent = this,
                    Alignment = Alignment.TopCenter,
                    Text = Screen.LoadedReplay.PlayerName,
                    Font = Fonts.AssistantRegular16,
                    PosY = Watching.PosY,
                    TextScale = replayTextScale,
                    TextColor = Colors.MainAccent,
                    Alpha = 0
                };

                var watchingLength = Watching.Font.MeasureString(Watching.Text).X;
                var playerNameLength = PlayerName.Font.MeasureString(PlayerName.Text).X;
                var totalLength = watchingLength + playerNameLength;
                var center = totalLength / 2f;

                Watching.PosX = watchingLength / 2.0f - center;
                PlayerName.PosX = Watching.PosX + watchingLength + playerNameLength / 2.0f - center / 2f + 2;
            }
        
            Title = new SpriteText
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Text = $"{Screen.Map.Artist} - {Screen.Map.Title}",
                Font = Fonts.AllerRegular16,
                PosY = Watching.PosY + TextYSpacing + TextYSpacing,
                Alpha = 0,
                TextScale = 0.85f
            };

            Difficulty = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Text = $"[{Screen.Map.DifficultyName}]",
                Font = Fonts.AllerRegular16,
                PosY = Title.PosY + TextYSpacing + TextYSpacing * 0.85f,
                TextScale = 0.80f,
                Alpha = 0
            };

            Creator = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Text = $"Mapped By: \"{Screen.Map.Creator}\"",
                Font = Fonts.AllerRegular16,
                PosY = Difficulty.PosY + TextYSpacing + TextYSpacing * 0.80f,
                TextScale = 0.75f,
                Alpha = 0
            };

            Rating = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Text = $"Rating: {StringHelper.AccuracyToString(Screen.Map.AverageNotesPerSecond(GameBase.AudioEngine.PlaybackRate)).Replace("%", "")}",
                Font = Fonts.AllerRegular16,
                PosY = Creator.PosY + TextYSpacing + TextYSpacing * 0.75f,
                TextScale = 0.70f,
                Alpha = 0,
                TextColor = ColorHelper.DifficultyToColor(Screen.Map.AverageNotesPerSecond(GameBase.AudioEngine.PlaybackRate))
            };

            // Get a formatted string of the activated mods.
            var modsString = "Mods: " + (GameBase.CurrentGameModifiers.Count > 0 ? $"{ModHelper.GetActivatedModsString()}" :  "None");         
            Mods = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Text = modsString,
                Font = Fonts.AllerRegular16,
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

                if (Screen.InReplayMode)
                {
                    Watching.FadeIn(dt, AnimationScale);
                    PlayerName.FadeIn(dt, AnimationScale);
                }
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