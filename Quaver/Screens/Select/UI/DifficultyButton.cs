using System;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.Select.UI
{
    public class DifficultyButton : Button
    {
        /// <summary>
        ///     The parent difficulty selector container.
        /// </summary>
        public DifficultyButtonContainer ButtonContainer { get; }

        /// <summary>
        ///     The map the difficulty is for.
        /// </summary>
        public Map Map { get; }

        /// <summary>
        ///     The height of the difficulty selection item.
        /// </summary>
        public static int HEIGHT = 33;

        /// <summary>
        ///     The game mode the map is for
        /// </summary>
        public Sprite GameMode { get; }

        /// <summary>
        ///     The grade achieved on the map.
        /// </summary>
        public Sprite GradeAchieved { get; }

        /// <summary>
        ///     The name of the difficulty.
        /// </summary>
        public SpriteText DifficultyName { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="buttonContainer"></param>
        /// <param name="map"></param>
        public DifficultyButton(DifficultyButtonContainer buttonContainer, Map map)
        {
            Map = map;
            ButtonContainer = buttonContainer;

            Parent = buttonContainer;
            Size = new ScalableVector2(ButtonContainer.Width, HEIGHT);
            Tint = Color.Black;

            GameMode = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(HEIGHT * 0.75f, HEIGHT * 0.75f),
                Image = SkinManager.Skin.Grades[Grade.S],
                X = 8,
                UsePreviousSpriteBatchOptions = true
            };

            GradeAchieved = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(HEIGHT * 0.75f, HEIGHT * 0.75f),
                Image = Map.HighestRank == Grade.None ? SkinManager.Skin.Grades[Grade.A] : SkinManager.Skin.Grades[Map.HighestRank],
                X = GameMode.X + GameMode.Width + 8,
                UsePreviousSpriteBatchOptions = true
            };

            DifficultyName = new SpriteText(Fonts.AllerRegular16, Map.DifficultyName)
            {
                Parent = this,
                TextScale = 0.65f,
                TextColor = Color.White,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };

            DifficultyName.X += GradeAchieved.X + GradeAchieved.Width + DifficultyName.MeasureString().X / 2f + 8f;
            Clicked += (sender, args) =>
            {
                Console.WriteLine("clicked");
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            FadeToColor(MapManager.Selected.Value == Map ? Colors.MainAccent : Color.Black, gameTime.ElapsedGameTime.TotalMilliseconds, 90);
            base.Update(gameTime);
        }
    }
}