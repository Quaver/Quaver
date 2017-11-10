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
using Quaver.GameState.States;
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

        //TODO: update later   TEST.
        private List<Button> Buttons { get; set; } = new List<Button>();
        private Button PlayButton { get; set; }
        private Boundary Boundary { get; set; } = new Boundary();
        private int ButtonPos { get; set; } = 50;

        public void Initialize()
        {
            //Create buttons for every beatmap set TODO: Use beatmap set instead of beatmaps
            foreach (KeyValuePair<string, List<Beatmap>> mapset in GameBase.VisibleBeatmaps)
            {
                //Create Song Buttons
                foreach (var map in mapset.Value)
                {
                    var newButton = new TextButton(new Vector2(300, 20),
                        map.Artist + " - " + map.Title + " [" + map.DifficultyName + "]")
                    {
                        Image = GameBase.LoadedSkin.ColumnTimingBar,
                        Alignment = Alignment.TopLeft,
                        PositionY = ButtonPos,
                        Parent = Boundary
                    };
                    newButton.TextSprite.Alignment = Alignment.MidLeft;
                    newButton.TextSprite.PositionX = 20;
                    var currentMap = map;
                    newButton.Clicked += (sender, e) => ButtonClick(sender, e, newButton.TextSprite.Text, currentMap);
                    Buttons.Add(newButton);
                    ButtonPos += 20;
                }

                PlayButton = new TextButton(new Vector2(200, 50), "Play")
                {
                    Image = GameBase.LoadedSkin.ColumnTimingBar,
                    Alignment = Alignment.TopRight,
                    Parent = Boundary
                };
                PlayButton.Clicked += PlayMap;
            }

            //Add map selected text TODO: remove later
            LogManager.AddLogger("MapSelected",Color.Yellow);
            LogManager.UpdateLogger("MapSelected", "Map Selected: "+GameBase.SelectedBeatmap.Artist + " - " + GameBase.SelectedBeatmap.Title + " [" + GameBase.SelectedBeatmap.DifficultyName + "]");
            UpdateReady = true;
        }

        public void LoadContent() { }

        public void UnloadContent()
        {
            UpdateReady = false;
            PlayButton.Clicked -= PlayMap;

            //TODO: Remove button delegates ?
            foreach (TextButton button in Buttons)
            {
                //button.Clicked -= Delegate;
            }

            Boundary.Destroy();
        }

        public void Update(GameTime gameTime)
        {
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

            GameBase.ChangeBeatmap(map);
            GameBase.SelectedBeatmap.Song.Play(GameBase.SelectedBeatmap.AudioPreviewTime);


            // Stop the selected song since it's only played during the main menu.
            //GameBase.SelectedBeatmap.Song.Stop();

            //Change to SongSelectState
            //GameStateManager.Instance.AddState(new SongSelectState());
        }

        //TODO: Remove
        public void PlayMap(object sender, EventArgs e)
        {
            GameBase.SelectedBeatmap.Song.Stop();
            GameStateManager.Instance.ChangeState(new SongLoadingState());
        }
    }
}
