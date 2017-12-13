using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Quaver.Config;
using Quaver.Database;
using Quaver.Gameplay;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using Quaver.Replays;
using Quaver.Scores;
using Quaver.Graphics.Text;

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
        ///     The replay from the previous play state
        /// </summary>
        private Replay Replay { get; set; }

        /// <summary>
        ///     The replay frames that were captured during the previous play.
        /// </summary>
        private List<ReplayFrame> ReplayFrames { get; set; }

        /// <summary>
        ///     The path to save this particular replay at.
        /// </summary>
        private string ReplayPath { get; set; }

        /// <summary>
        ///     The instance of the applause sound effect
        /// </summary>
        private SoundEffectInstance ApplauseInstance { get; set; }

        /// <summary>
        ///     The Boundary Containing every Judge Text
        /// </summary>
        private Boundary JudgeInfoBoundary { get; set; }

        /// <summary>
        ///     Boundary containing ms deviance data ui
        /// </summary>
        private Sprite MsDevianceBoundary { get; set; }

        //todo: have images and crap, but have the text only display number and not title
        /// <summary>
        ///     The Text displaying Judge info
        /// </summary>
        private TextBoxSprite[] JudgeText { get; set; }

        /// <summary>
        ///     The text that displays score
        /// </summary>
        private TextBoxSprite ScoreText { get; set; }

        /// <summary>
        ///     The text that displays max combo
        /// </summary>
        private TextBoxSprite ComboText { get; set; }

        /// <summary>
        ///     The text that displays accuracy
        /// </summary>
        private TextBoxSprite AccuracyText { get; set; }

        /// <summary>
        ///     Constructor - In order to get to this state, it's essential that you pass in 
        ///     the beatmap md5 and the score data.
        /// </summary>
        /// <param name="beatmapMd5"></param>
        /// <param name="scoreData"></param>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <param name="difficultyName"></param>
        public ScoreScreenState(string beatmapMd5, ScoreManager scoreData, string artist, string title, string difficultyName, List<ReplayFrame> replayFrames)
        {
            // Initialize data
            BeatmapMd5 = beatmapMd5;
            ScoreData = scoreData;
            Artist = artist;
            Title = title;
            DifficultyName = difficultyName;
            ReplayFrames = replayFrames;
            Replay = CreateReplayFromScore();

            ReplayPath = $"{Configuration.Username} - {Artist} - {Title} [{DifficultyName}] ({DateTime.UtcNow})";

            // Insert the score into the database
            Task.Run(() =>
            {
                // Write replay to log file if debug is toggled
                Replay.WriteToLogFile();
                Replay.Write(ReplayPath, true);
            }).ContinueWith(async (t) => await LocalScoreCache.InsertScoreIntoDatabase(CreateLocalScore(Replay)));

            // Create an instance of the applause sound effect so that we can stop it later.
            ApplauseInstance = GameBase.LoadedSkin.Applause.CreateInstance();
        }

        /// <summary>
        ///     Initialize
        /// </summary>
        public void Initialize()
        {
            // Iniitalize UI Elements
            CreateUI();

            // Log the score
            LogScore();

            UpdateReady = true;

            // Play Applause
            ApplauseInstance.Play();
        }
        
        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            BackButton.Destroy();
            JudgeInfoBoundary.Destroy();
            MsDevianceBoundary.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            BackButton.Update(dt);
            JudgeInfoBoundary.Update(dt);
            MsDevianceBoundary.Update(dt);
        }

        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            BackButton.Draw();
            JudgeInfoBoundary.Draw();
            MsDevianceBoundary.Draw();
        }

        /// <summary>
        ///     Creates the UI
        /// </summary>
        private void CreateUI()
        {
            // Create Back Button
            BackButton = new TextButton(new Vector2(300,200),"BACK" )
            {
                Alignment = Alignment.TopRight
            };

            BackButton.Clicked += OnBackButtonClick;

            //Create Judge Info Boundary
            JudgeInfoBoundary = new Boundary()
            {
                SizeX = 300,
                SizeY = 200,
                Alignment = Alignment.MidCenter
            };

            //Create Judge Text
            JudgeText = new TextBoxSprite[6];
            for (var i=0; i<6; i++)
            {
                JudgeText[i] = new TextBoxSprite()
                {
                    Text = "[" + ScoreData.JudgeNames[i] + "]: Press|Release: " + ScoreData.JudgePressSpread[i] + " | " + ScoreData.JudgeReleaseSpread[i],
                    TextColor = CustomColors.JudgeColors[i],
                    Font = Fonts.Medium16,
                    PositionY = 200 * i/6,
                    ScaleX = 1,
                    Textwrap = false,
                    TextAlignment = Alignment.MidCenter,
                    Multiline = false,
                    Parent = JudgeInfoBoundary
                };
            }

            //Create Score Text
            ScoreText = new TextBoxSprite()
            {
                Text = ScoreData.ScoreTotal.ToString(),
                Font = Fonts.Medium24,
                TextAlignment = Alignment.MidLeft,
                TextColor = Color.White,
                Textwrap = false,
                Multiline = false,
                ScaleX = 1,
                SizeY = 70,
                PositionY = -70,
                Parent = JudgeInfoBoundary
            };

            //Create Accuracy Text
            AccuracyText = new TextBoxSprite()
            {
                Text = $"{ScoreData.Accuracy * 100:0.00}%",
                Font = Fonts.Medium24,
                TextAlignment = Alignment.MidRight,
                TextColor = Color.White,
                Textwrap = false,
                Multiline = false,
                ScaleX = 1,
                SizeY = 70,
                PositionY = -70,
                Parent = JudgeInfoBoundary
            };

            // create ms deviance box
            MsDevianceBoundary = new Sprite()
            {
                SizeX = 400,
                SizeY = 150,
                Alignment = Alignment.BotCenter,
                Tint = Color.Black,
                PositionY = -100,
                Alpha = 0.5f
            };

            //

            //create note data graph todo: add text and stuff
            for (var i=0; i<5; i++)
            {
                Sprite ob;

                //bottom
                ob = new Sprite()
                {
                    PositionY = MsDevianceBoundary.SizeY * (ScoreData.HitWindowPress[i] / ScoreData.HitWindowPress[4]) / 2,
                    ScaleX = 1,
                    SizeY = 1,
                    Tint = CustomColors.JudgeColors[i],
                    Alpha = 0.1f,
                    Alignment = Alignment.MidLeft,
                    Parent = MsDevianceBoundary
                };

                //top
                ob = new Sprite()
                {
                    PositionY = -MsDevianceBoundary.SizeY * (ScoreData.HitWindowPress[i] / ScoreData.HitWindowPress[4]) / 2,
                    ScaleX = 1,
                    SizeY = 1,
                    Tint = CustomColors.JudgeColors[i],
                    Alpha = 0.1f,
                    Alignment = Alignment.MidLeft,
                    Parent = MsDevianceBoundary
                };
            }

            //record time intervals on graph every 15 seconds
            int timeIndex = 1;
            while (timeIndex * 15000 < ScoreData.SongLength)
            {
                var ob = new Sprite()
                {
                    PositionX = MsDevianceBoundary.SizeX * (float)((timeIndex*15000)/ ScoreData.SongLength),
                    SizeX = 1,
                    ScaleY = 1,
                    Alpha = 0.2f,
                    Parent = MsDevianceBoundary
                };

                timeIndex++;
            }

            //temp todo: create proper ms deviance display. make this not lag some how
            //record misses
            foreach (var ms in ScoreData.MsDeviance)
            {
                if (ms.Type == 5)
                {
                    var ob = new Sprite()
                    {
                        PositionX = ((float)ms.Position * MsDevianceBoundary.SizeX) - 1,
                        SizeX = 1,
                        ScaleY = 1,
                        Tint = CustomColors.JudgeMiss,
                        Alpha = 0.4f,
                        Parent = MsDevianceBoundary
                    };
                }
            }
            //record other offset data
            foreach (var ms in ScoreData.MsDeviance)
            {
                if (ms.Type != 5)
                {
                    var ob = new Sprite()
                    {
                        PositionX = ((float)ms.Position * MsDevianceBoundary.SizeX) - 1,
                        PositionY = ((float)ms.Offset * (MsDevianceBoundary.SizeY / 2)) - 1,
                        SizeX = 2,
                        SizeY = 2,
                        Tint = CustomColors.JudgeColors[ms.Type],
                        Alignment = Alignment.MidLeft,
                        Parent = MsDevianceBoundary
                    };
                }
            }

            //create labels
            TextBoxSprite label;
            
            //top
            label = new TextBoxSprite()
            {
                Text = "Late (+" + Math.Floor(ScoreData.HitWindowPress[4]) + "ms)",
                Font = Fonts.Medium12,
                PositionX = 2,
                PositionY = 2,
                SizeX = 200,
                SizeY = 50,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.TopLeft,
                Multiline = false,
                Textwrap = false,
                Parent = MsDevianceBoundary
            };

            //bottom
            label = new TextBoxSprite()
            {
                Text = "Early (-" + Math.Floor(ScoreData.HitWindowPress[4]) + "ms)",
                Font = Fonts.Medium12,
                PositionX = 2,
                PositionY = -2,
                SizeX = 200,
                SizeY = 50,
                Alignment = Alignment.BotLeft,
                TextAlignment = Alignment.BotLeft,
                Multiline = false,
                Textwrap = false,
                Parent = MsDevianceBoundary
            };
        }

        /// <summary>
        ///     Back Button Click Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackButtonClick(object sender, EventArgs e)
        {
            ApplauseInstance.Stop(true);
            GameBase.LoadedSkin.Back.Play((float) Configuration.VolumeGlobal / 100 * Configuration.VolumeEffect / 100, 0, 0);
            GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        /// <summary>
        ///     Creates a local score object from the score given
        /// </summary>
        /// <returns></returns>
        private LocalScore CreateLocalScore(Replay rp)
        {
            // Store the score in the database
            return new LocalScore
            {
                BeatmapMd5 = BeatmapMd5,
                Name = Configuration.Username,
                DateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                Score = ScoreData.ScoreTotal,
                Accuracy = Math.Round(ScoreData.Accuracy * 100, 2),
                MaxCombo = ScoreData.Combo,
                MarvPressCount = ScoreData.JudgePressSpread[0],
                MarvReleaseCount = ScoreData.JudgeReleaseSpread[0],
                PerfPressCount = ScoreData.JudgePressSpread[1],
                PerfReleaseCount = ScoreData.JudgeReleaseSpread[1],
                GreatPressCount = ScoreData.JudgePressSpread[2],
                GreatReleaseCount = ScoreData.JudgeReleaseSpread[2],
                GoodPressCount = ScoreData.JudgePressSpread[3],
                GoodReleaseCount = ScoreData.JudgeReleaseSpread[3],
                OkayPressCount = ScoreData.JudgePressSpread[4],
                OkayReleaseCount = ScoreData.JudgeReleaseSpread[4],
                Misses = ScoreData.JudgePressSpread[5] + ScoreData.JudgeReleaseSpread[5],
                Rating = 0.0f,
                Mods = GameBase.CurrentGameModifiers.Sum(x => (int)x.ModIdentifier),
                ScrollSpeed = Configuration.ScrollSpeed,
                ReplayData = ReplayPath += ".qua"
            };
        }

        /// <summary>
        ///     Creates a replay object from all the score data
        /// </summary>
        private Replay CreateReplayFromScore()
        {
            var rp = new Replay
            {
                QuaverVersion = GameBase.BuildVersion,
                BeatmapMd5 = BeatmapMd5,
                ReplayMd5 = "Not Implemented",
                Name = Configuration.Username,
                Date = DateTime.UtcNow,
                ScrollSpeed = Configuration.ScrollSpeed,
                Score = ScoreData.ScoreTotal,
                Accuracy = (float)Math.Round(ScoreData.Accuracy * 100, 2),
                MaxCombo = ScoreData.Combo,
                MarvPressCount = ScoreData.JudgePressSpread[0],
                MarvReleaseCount = ScoreData.JudgeReleaseSpread[0],
                PerfPressCount = ScoreData.JudgePressSpread[1],
                PerfReleaseCount = ScoreData.JudgeReleaseSpread[1],
                GreatPressCount = ScoreData.JudgePressSpread[2],
                GreatReleaseCount = ScoreData.JudgeReleaseSpread[2],
                GoodPressCount = ScoreData.JudgePressSpread[3],
                GoodReleaseCount = ScoreData.JudgeReleaseSpread[3],
                OkayPressCount = ScoreData.JudgePressSpread[4],
                OkayReleaseCount = ScoreData.JudgeReleaseSpread[4],
                Misses = ScoreData.JudgePressSpread[5] + ScoreData.JudgeReleaseSpread[5],
                ReplayFrames = ReplayFrames
            };

            // Add the mods to the replay
            GameBase.CurrentGameModifiers.ForEach(x => rp.Mods = rp.Mods | x.ModIdentifier);

            return rp;
        }

        /// <summary>
        ///     Logs the score to the runtime log and console.
        /// </summary>
        private void LogScore()
        {
            Logger.Log($"Quaver Version: {Replay.QuaverVersion}", Color.Pink);
            Logger.Log($"Beatmap MD5: {Replay.BeatmapMd5}", Color.Pink);
            Logger.Log($"Replay MD5: {Replay.ReplayMd5}", Color.Pink);
            Logger.Log($"Player: {Configuration.Username}", Color.Pink);
            Logger.Log($"Date: {Replay.Date.ToString(CultureInfo.InvariantCulture)}", Color.Pink);
            Logger.Log($"Mods: {GameBase.CurrentGameModifiers.Sum(x => (int)x.ModIdentifier)}", Color.Pink);
            Logger.Log($"Scroll Speed: {Configuration.ScrollSpeed}", Color.Pink);
            Logger.Log($"Score: {Replay.Score}", Color.Pink);
            Logger.Log($"Accuracy: {Replay.Accuracy}%", Color.Pink);
            Logger.Log($"Max Combo: {Replay.MaxCombo}", Color.Pink);
            Logger.Log($"Marv Count: {Replay.MarvPressCount + Replay.MarvReleaseCount}", Color.Pink);
            Logger.Log($"Perf Count: {Replay.PerfPressCount + Replay.PerfReleaseCount}", Color.Pink);
            Logger.Log($"Great Count: {Replay.GreatPressCount + Replay.GreatReleaseCount}", Color.Pink);
            Logger.Log($"Good Count: {Replay.GoodPressCount + Replay.GoodReleaseCount}", Color.Pink);
            Logger.Log($"Okay Count: {Replay.OkayPressCount + Replay.OkayReleaseCount}", Color.Pink);
            Logger.Log($"Miss Count: {Replay.Misses}", Color.Pink);
            Logger.Log($"Replay Frame Count: {Replay.ReplayFrames.Count}", Color.Pink);
        }
    }
}
