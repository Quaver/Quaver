﻿using Microsoft.Xna.Framework;
using Quaver.Database.Maps;
using Quaver.GameState;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.States.Select
{
    /// <summary>
    ///     Displays beatmap info in a window whenever a beatmap gets selected
    /// </summary>
    internal class MapInfoWindow : IGameStateComponent
    {
        private QuaverContainer Boundary { get; set; }

        private QuaverTextbox MapInfo { get; set; }

        public void Draw()
        {
            Boundary.Draw();
        }

        public void Initialize(IGameState state)
        {
            Boundary = new QuaverContainer();

            var window = new QuaverSprite()
            {
                Size = new UDim2D(600 * GameBase.WindowUIScale, 300 * GameBase.WindowUIScale),
                Position = new UDim2D(5, 40),
                Tint = Color.Black,
                Alpha = 0.8f,
                Parent = Boundary
            };

            MapInfo = new QuaverTextbox()
            {
                Size = new UDim2D(-10, -10, 1, 1),
                Position = new UDim2D(5, 5),
                Font = QuaverFonts.Medium16,
                TextColor = Color.White,
                TextBoxStyle = TextBoxStyle.WordwrapMultiLine,
                TextAlignment = Alignment.TopLeft,
                Text = "Map selected: asdasdasd asd",
                Parent = window
            };

            //UpdateInfo(GameBase.SelectedBeatmap);
        }

        public void UnloadContent()
        {
            Boundary.Destroy();
        }

        public void Update(double dt)
        {
            Boundary.Update(dt);
        }

        public void UpdateInfo(Map map)
        {
            MapInfo.Text = "Map selected: " + map.Artist + " - " + map.Title + "(" + map.DifficultyName + ")" + "\n"
                + map.Creator;
        }
    }
}
