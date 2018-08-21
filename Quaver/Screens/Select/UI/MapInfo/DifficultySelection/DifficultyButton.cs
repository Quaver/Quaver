using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.Select.UI.MapInfo.DifficultySelection
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
        public static int HEIGHT = 34;

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
            Tint = Colors.MainAccent;
            Image = UserInterface.DiffButtonInactive;

            GradeAchieved = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(HEIGHT * 0.75f, HEIGHT * 0.75f),
                Image = Map.HighestRank == Grade.None ? SkinManager.Skin.Grades[Grade.A] : SkinManager.Skin.Grades[Map.HighestRank],
                X = 8,
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
                buttonContainer.Selector.SelectDifficulty(map.Mapset, map);
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Image = MapManager.Selected.Value == Map ? UserInterface.DiffButton : UserInterface.DiffButtonInactive;
            base.Update(gameTime);
        }
    }
}