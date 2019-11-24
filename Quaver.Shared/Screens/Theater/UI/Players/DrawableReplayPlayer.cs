using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Replays;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Theater.UI.Players
{
    public class DrawableReplayPlayer : Sprite
    {
        /// <summary>
        /// </summary>
        private Replay Replay { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus PlayerName { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Mods { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus PerformanceRating { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Accuracy { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Judgements { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="replay"></param>
        public DrawableReplayPlayer(Replay replay)
        {
            Replay = replay;
            Size = new ScalableVector2(400, 180);

            Tint = Color.Black;
            Alpha = 0.45f;

            CreatePlayerName();
            CreateMods();
            CreatePerformanceRating();
            CreateAccuracy();
            CreateJudgements();

            ScheduleUpdate(UpdateContent);
        }

        /// <summary>
        /// </summary>
        private void UpdateContent()
        {
            const int space = 12;
            PlayerName.Text = $"Player: {Replay.PlayerName}";

            Mods.Text = $"Mods: {ModHelper.GetModsString(Replay.Mods)}";
            Mods.Y = PlayerName.Y + PlayerName.Height + space;

            PerformanceRating.Text = $"Performance Rating: " +
                                     $"{StringHelper.RatingToString(new RatingProcessorKeys(MapManager.Selected.Value.DifficultyFromMods(Replay.Mods)).CalculateRating(Replay.Accuracy))}";
            PerformanceRating.Y = Mods.Y + Mods.Height + space;

            Accuracy.Text = $"Accuracy: {StringHelper.AccuracyToString(Replay.Accuracy)}";
            Accuracy.Y = PerformanceRating.Y + PerformanceRating.Height + space;

            Judgements.Text = $"Judgements: {Replay.CountMarv} / {Replay.CountPerf} / {Replay.CountGreat} / {Replay.CountGood} / " +
                              $"{Replay.CountOkay} / {Replay.CountMiss} / {Replay.MaxCombo}x";

            Judgements.Y = Accuracy.Y + Accuracy.Height + space;
        }

        /// <summary>
        /// </summary>
        private void CreatePlayerName()
        {
            PlayerName = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                X = 10,
                Y = 12
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMods()
        {
            Mods = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                X = PlayerName.X
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePerformanceRating()
        {
            PerformanceRating = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                X = PlayerName.X
            };
        }

        /// <summary>
        /// </summary>
        private void CreateAccuracy()
        {
            Accuracy = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                X = PlayerName.X
            };
        }

        /// <summary>
        /// </summary>
        private void CreateJudgements()
        {
            Judgements = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                X = PlayerName.X
            };
        }
    }
}