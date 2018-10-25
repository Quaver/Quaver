using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Assets;
using Quaver.Audio;
using Quaver.Graphics;
using Quaver.Helpers;
using Quaver.Modifiers;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Gameplay.UI
{
    public class SongInformation : Sprite
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
        private static float AnimationScale => 120f / AudioEngine.Track.Rate;

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="screen"></param>
        internal SongInformation(GameplayScreen screen)
        {
            Screen = screen;

            Size = new ScalableVector2(750, 150);
            Tint = Colors.MainAccentInactive;
            Alpha = 0;

            // Create watching text outside of replay mode because other text relies on it.
            Watching = new SpriteText(BitmapFonts.Exo2Regular, "Watching", 16)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 0,
                Alpha = 0
            };

            // Handle positioning and create player name text if we're actually
            // watching a replay.
            if (Screen.InReplayMode)
            {
                PlayerName = new SpriteText(BitmapFonts.Exo2Regular, Screen.LoadedReplay.PlayerName, 18)
                {
                    Parent = this,
                    Alignment = Alignment.TopCenter,
                    Y = Watching.Y,
                    Tint = Colors.MainAccent,
                    Alpha = 0
                };

                var watchingLength = Watching.Width;
                var playerNameLength = PlayerName.Width;
                var totalLength = watchingLength + playerNameLength;
                var center = totalLength / 2f;

                Watching.X = watchingLength / 2.0f - center;
                PlayerName.X = Watching.X + watchingLength + playerNameLength / 2.0f - center / 2f + 2;
            }

            Title = new SpriteText(BitmapFonts.Exo2Regular, $"{Screen.Map.Artist} - {Screen.Map.Title}", 18)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Watching.Y + TextYSpacing + TextYSpacing,
                Alpha = 0,
            };

            Difficulty = new SpriteText(BitmapFonts.Exo2Regular, $"[{Screen.Map.DifficultyName}]", 18)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Title.Y + TextYSpacing + TextYSpacing * 0.85f,
                Alpha = 0
            };

            Creator = new SpriteText(BitmapFonts.Exo2Regular, $"Mapped By: \"{Screen.Map.Creator}\"", 18)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Difficulty.Y + TextYSpacing + TextYSpacing * 0.80f,
                Alpha = 0
            };

            Rating = new SpriteText(BitmapFonts.Exo2Regular, $"Rating: {StringHelper.AccuracyToString(Screen.Map.AverageNotesPerSecond(AudioEngine.Track.Rate)).Replace("%", "")}",
                16)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Creator.Y + TextYSpacing + TextYSpacing * 0.75f,
                Alpha = 0,
                Tint = ColorHelper.DifficultyToColor(Screen.Map.AverageNotesPerSecond(AudioEngine.Track.Rate))
            };

            // Get a formatted string of the activated mods.
            var modsString = "Mods: " + (ModManager.CurrentModifiersList.Count > 0 ? $"{ModHelper.GetModsString(Screen.Ruleset.ScoreProcessor.Mods)}" : "None");
            Mods = new SpriteText(BitmapFonts.Exo2Regular, modsString, 18)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Rating.Y + TextYSpacing + TextYSpacing * 0.7f,
                Alpha = 0
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Fade in on map start
            if (Screen.Timing.Time < -500)
            {
                var alpha = MathHelper.Lerp(Title.Alpha, 1, (float) Math.Min(dt / AnimationScale, 1));

                Title.Alpha = alpha;
                Difficulty.Alpha = alpha;
                Creator.Alpha = alpha;
                Rating.Alpha = alpha;
                Mods.Alpha = alpha;

                if (Screen.InReplayMode)
                {
                    Watching.Alpha = alpha;
                    PlayerName.Alpha = alpha;
                }
            }
            else
            {
                var alpha = MathHelper.Lerp(Title.Alpha, 0, (float)Math.Min(dt / AnimationScale, 1));

                Title.Alpha = alpha;
                Difficulty.Alpha = alpha;
                Creator.Alpha = alpha;
                Rating.Alpha = alpha;
                Mods.Alpha = alpha;
            }

            base.Update(gameTime);
        }
    }
}
