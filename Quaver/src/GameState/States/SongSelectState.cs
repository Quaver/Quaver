using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Logging;
using Quaver.Main;

namespace Quaver.GameState
{
    internal class SongSelectState : IGameState
    {
        public State CurrentState { get; set; } = State.MainMenu;
        public bool UpdateReady { get; set; }

        //TEST
        private List<Button> Buttons = new List<Button>();
        private Boundary Boundary = new Boundary();

        private int _buttonPos = 150;

        public void Initialize()
        {
            //Create buttons for every beatmap set TODO: Use beatmap set instead of beatmaps
            foreach (KeyValuePair<string, List<Beatmap>> mapset in GameBase.VisibleBeatmaps)
            {
                foreach (var map in mapset.Value)
                {
                    var newButton = new TextButton(new Vector2(300, 20),
                        map.Artist + " - " + map.Title + " [" + map.DifficultyName + "]")
                    {
                        Image = GameBase.LoadedSkin.ColumnTimingBar,
                        Alignment = Alignment.TopLeft,
                        PositionY = _buttonPos,
                        Parent = Boundary
                    };
                    newButton.TextSprite.Alignment = Alignment.MidLeft;
                    newButton.TextSprite.PositionX = 20;
                    var currentMap = map;
                    newButton.Clicked += (sender, e) => ButtonClick(sender, e, newButton.TextSprite.Text, currentMap);
                    Buttons.Add(newButton);
                    _buttonPos += 20;
                }
            }

            //Add map selected text TODO: remove later
            LogManager.AddLogger("MapSelected",Color.Yellow);
            LogManager.UpdateLogger("MapSelected", "Map Selected: "+GameBase.SelectedBeatmap.Artist + " - " + GameBase.SelectedBeatmap.Title + " [" + GameBase.SelectedBeatmap.DifficultyName + "]");
            UpdateReady = true;
        }

        public void LoadContent() { }

        public void UnloadContent()
        {

            GameBase.SelectedBeatmap.Song.Stop();
        }

        public void Update(GameTime gameTime)
        {

            // Select and play random maps.
            //MenuAudioPlayer.PlayRandomBeatmaps();

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            Boundary.Update(dt);
        }

        public void Draw()
        {
            Boundary.Draw();
        }

        //TODO: Remove
        public void ButtonClick(object sender, EventArgs e, string text, Beatmap map)
        {
            LogManager.UpdateLogger("MapSelected","Map Selected: "+text);

            GameBase.SelectedBeatmap.Song.Stop();
            GameBase.SelectedBeatmap = map;
            GameBase.SelectedBeatmap.LoadAudio();
            GameBase.SelectedBeatmap.Song.Play();


            // Stop the selected song since it's only played during the main menu.
            //GameBase.SelectedBeatmap.Song.Stop();

            //Change to SongSelectState
            //GameStateManager.Instance.AddState(new SongSelectState());
        }
    }
}
