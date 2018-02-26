using Microsoft.Xna.Framework;
using Quaver.Database.Beatmaps;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.SongSelect
{
    /// <summary>
    ///     Displays beatmap info in a window whenever a beatmap gets selected
    /// </summary>
    class MapInfoWindow : IHelper
    {
        private Boundary Boundary { get; set; }

        private TextBoxSprite MapInfo { get; set; }

        public void Draw()
        {
            Boundary.Draw();
        }

        public void Initialize(IGameState state)
        {
            Boundary = new Boundary();

            var window = new Sprite()
            {
                Size = new UDim2(600 * GameBase.WindowUIScale, 300 * GameBase.WindowUIScale),
                Position = new UDim2(5, 40),
                Tint = Color.Black,
                Alpha = 0.8f,
                Parent = Boundary
            };

            MapInfo = new TextBoxSprite()
            {
                Size = new UDim2(-10, -10, 1, 1),
                Position = new UDim2(5, 5),
                Font = Fonts.Medium16,
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

        public void UpdateInfo(Beatmap map)
        {
            MapInfo.Text = "Map selected: " + map.Artist + " - " + map.Title + "(" + map.DifficultyName + ")" + "\n"
                + map.Creator;
        }
    }
}
