using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Gameplay;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Logging;

namespace Quaver.GameState.States
{
    internal class ScoreScreenState : IGameState
    {
        /// <summary>
        ///     The state
        /// </summary>
        public State CurrentState { get; set; } = State.ScoreScreen;

        /// <summary>
        /// 
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     The MD5 Hash of the beatmap.
        /// </summary>
        private string BeatmapMd5 { get; }

        /// <summary>
        ///     The score data from the previous gameplay session.
        /// </summary>
        private ScoreManager ScoreData { get; }

        /// <summary>
        ///     The artist of the played beatmap.
        /// </summary>
        private string Artist { get; }

        /// <summary>
        ///     The title of the played beatmap
        /// </summary>
        private string Title { get; }

        /// <summary>
        ///     The difficulty name of the played beatmap
        /// </summary>
        private string DifficultyName { get; }

        /// <summary>
        ///     The button to get back to song select
        /// </summary>
        private TextButton BackButton { get; set; }

        /// <summary>
        ///     Constructor - In order to get to this state, it's essential that you pass in 
        ///     the beatmap md5 and the score data.
        /// </summary>
        /// <param name="beatmapMd5"></param>
        /// <param name="scoreData"></param>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <param name="difficultyName"></param>
        public ScoreScreenState(string beatmapMd5, ScoreManager scoreData, string artist, string title, string difficultyName)
        {
            BeatmapMd5 = beatmapMd5;
            ScoreData = scoreData;
            Artist = artist;
            Title = title;
            DifficultyName = difficultyName;

            // Log relevant data
            Logger.Log($"User completed beatmap: {Artist} - {Title} - [{DifficultyName}]", Color.Cyan);
            Logger.Log("Beatmap MD5: " + BeatmapMd5, Color.Cyan);
            Logger.Log("Score: " + ScoreData.Score, Color.Cyan);
            Logger.Log($"Accuracy: {ScoreData.Accuracy}", Color.Cyan);
            Logger.Log($"Max Combo: {ScoreData.Combo}", Color.Cyan);
        }

        public void Initialize()
        {
            BackButton = new TextButton(new Vector2(300,200),"SONG SELECT" )
            {
                Alignment = Alignment.TopRight
            };
            BackButton.Clicked += OnBackButtonClick;

            UpdateReady = true;
        }

        private void OnBackButtonClick(object sender, EventArgs e)
        {
            GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        public void UnloadContent()
        {
            BackButton.Destroy();
        }

        public void Update(double dt)
        {
            BackButton.Update(dt);
        }

        public void Draw()
        {
            BackButton.Draw();
        }
    }
}
