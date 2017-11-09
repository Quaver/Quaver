using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Discord;
using Quaver.Gameplay;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Peppy;

namespace Quaver.GameState.States
{
    internal class MainMenuState : IGameState
    {
        public State CurrentState { get; set; } = State.MainMenu;

        //TEST
        public Button testButton;

        public Button importPeppyButton;

        public void Initialize()
        {
            // Load and play the randomly selected beatmap's song.
            if (GameBase.SelectedBeatmap != null)
            {
                // In the event that the song is already loaded up, we don't want to load it again
                // through this state.
                if (GameBase.SelectedBeatmap.Song != null)
                    GameBase.SelectedBeatmap.Song.Resume();
                else
                {
                    // Here we assume that the song hasn't been loaded since its length is 0,
                    // so we'll attempt to load it up and play it.
                    GameBase.SelectedBeatmap.LoadAudio();
                    GameBase.SelectedBeatmap.Song.Play();
                }

                // Set Rich Presence
                GameBase.DiscordController.presence.details = $"Listening to: {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title}";
                DiscordRPC.UpdatePresence(ref GameBase.DiscordController.presence);
            }
            else
            {
                // Set Rich Presence
                GameBase.DiscordController.presence.details = $"Idle";
                DiscordRPC.UpdatePresence(ref GameBase.DiscordController.presence);
            }

            testButton = new TextButton()
            {
                Size = new Vector2(200, 40),
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.MidCenter,
                Position = Vector2.Zero,
                Text = "Next State"
            };
            testButton.UpdateRect();
            testButton.Clicked += ButtonClick;

            importPeppyButton = new TextButton()
            {
                Size = new Vector2(200, 40),
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.TopCenter,
                Position = Vector2.Zero,
                Text = "Import .osz"
            };
            importPeppyButton.UpdateRect();
            importPeppyButton.Clicked += Osz.OnImportButtonClick;           
        }

        public void ButtonClick(object sender, EventArgs e)
        {
            LogManager.QuickLog("Clicked",Color.White,1f);

            // Stop the selected song since it's only played during the main menu.
            GameBase.SelectedBeatmap.Song.Stop();

            GameStateManager.Instance.AddState(new SongLoadingState());
        }


        public void LoadContent()
        {
            
        }

        public void UnloadContent()
        {
            testButton.Clicked -= ButtonClick;
            testButton.Destroy();
        }

        public void Update(GameTime gameTime)
        {
            testButton.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
            importPeppyButton.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
        }

        public void Draw()
        {
            testButton.Draw();
            importPeppyButton.Draw();
        }
    }
}
