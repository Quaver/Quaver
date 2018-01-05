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
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using Quaver.Replays;
using Quaver.Scores;
using Quaver.Graphics.Text;
using Quaver.GameState.Gameplay;
using Quaver.Utility;

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
            Task.Run(async () =>
            {
                // Write replay to log file if debug is toggled
                Replay.WriteToLogFile();
                Replay.Write(ReplayPath, true);

                try
                {
                    var newScore = CreateLocalScore(Replay);

                    // Insert the score in the DB.
                    await LocalScoreCache.InsertScoreIntoDatabase(newScore);

                    var previousScores = await LocalScoreCache.SelectBeatmapScores(beatmapMd5);

                    if (previousScores.Count > 0)
                        previousScores = previousScores.OrderByDescending(x => x.Score).ToList();

                    // If this score is higher than the last or there are no scores, we want to update this 
                    // beatmap's cache.
                    if (previousScores.Count > 0 && scoreData.ScoreTotal >= previousScores[0].Score)
                        GameBase.SelectedBeatmap.HighestRank = newScore.Grade;

                    GameBase.SelectedBeatmap.LastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    await BeatmapCache.UpdateBeatmap(GameBase.SelectedBeatmap);
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message, LogColors.GameError);
                }
            });

            // Create an instance of the applause sound effect so that we can stop it later.
            ApplauseInstance = GameBase.LoadedSkin.Applause.CreateInstance();

            // Set Rich Presence for this state
            SetDiscordRichPresence();
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
            BackButton = new TextButton(new Vector2(150,40),"BACK" )
            {
                Alignment = Alignment.TopRight
            };

            BackButton.Clicked += OnBackButtonClick;

            //Create Judge Info Boundary
            JudgeInfoBoundary = new Boundary()
            {
                Size = new UDim2(300, 200),
                Alignment = Alignment.MidCenter
            };

            //Create Judge Text
            JudgeText = new TextBoxSprite[6];
            for (var i=0; i<6; i++)
            {
                JudgeText[i] = new TextBoxSprite()
                {
                    Text = "[" + GameplayReferences.JudgeNames[i] + "]: " + ScoreData.JudgePressSpread[i] + " | " + ScoreData.JudgeReleaseSpread[i] + " Total: "+(ScoreData.JudgePressSpread[i] + ScoreData.JudgeReleaseSpread[i]),
                    TextColor = GameColors.JudgeColors[i],
                    Font = Fonts.Medium16,
                    Position = new UDim2(0,200 * i/6),
                    Size = new UDim2(0, 0, 1, 0),
                    TextAlignment = Alignment.MidRight,
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
                Position = new UDim2(0, -70),
                Size = new UDim2(0, 70, 1, 0),
                Parent = JudgeInfoBoundary
            };

            //Create Accuracy Text
            AccuracyText = new TextBoxSprite()
            {
                Text = $"{ScoreData.Accuracy * 100:0.00}%",
                Font = Fonts.Medium24,
                TextAlignment = Alignment.MidRight,
                TextColor = Color.White,
                Position = new UDim2(0, -70),
                Size = new UDim2(0, 70, 1, 0),
                Parent = JudgeInfoBoundary
            };

            // create ms deviance box
            MsDevianceBoundary = new Sprite()
            {
                Size = new UDim2(400, 150),
                Position = new UDim2(0, -100),
                Alignment = Alignment.BotCenter,
                Tint = Color.Black,
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
                    Position = new UDim2(0, MsDevianceBoundary.Size.Y.Offset * (ScoreData.HitWindowPress[i] / ScoreData.HitWindowPress[4]) / 2),
                    Size = new UDim2(0, 1, 1, 0),
                    Tint = GameColors.JudgeColors[i],
                    Alpha = 0.1f,
                    Alignment = Alignment.MidLeft,
                    Parent = MsDevianceBoundary
                };

                //top
                ob = new Sprite()
                {
                    Position = new UDim2(0, -MsDevianceBoundary.Size.Y.Offset * (ScoreData.HitWindowPress[i] / ScoreData.HitWindowPress[4]) / 2),
                    Size = new UDim2(0, 1, 1, 0),
                    Tint = GameColors.JudgeColors[i],
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
                    Position = new UDim2(MsDevianceBoundary.Size.X.Offset * (float)((timeIndex * 15000) / ScoreData.SongLength), 0),
                    Size = new UDim2(1, 0, 0, 1),
                    Alpha = timeIndex % 4 == 0 ? 0.5f : 0.15f,
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
                        Position = new UDim2(((float)ms.Position * MsDevianceBoundary.Size.X.Offset) - 1f, 0),
                        Size = new UDim2(2, 0, 0, 1),
                        Tint = GameColors.JudgeMiss,
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
                        Position = new UDim2(((float)ms.Position * MsDevianceBoundary.Size.X.Offset) - 1.5f, ((float)ms.Offset * (MsDevianceBoundary.Size.Y.Offset / 2f)) - 1.5f),
                        Size = new UDim2(3, 3),
                        Tint = GameColors.JudgeColors[ms.Type],
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
                Position = new UDim2(2, 2),
                Size = new UDim2(200, 50),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.TopLeft,
                Parent = MsDevianceBoundary
            };

            //bottom
            label = new TextBoxSprite()
            {
                Text = "Early (-" + Math.Floor(ScoreData.HitWindowPress[4]) + "ms)",
                Font = Fonts.Medium12,
                Position = new UDim2(2, -2),
                Size = new UDim2(200, 50),
                Alignment = Alignment.BotLeft,
                TextAlignment = Alignment.BotLeft,
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
                Grade = Util.GetGradeFromAccuracy((float)Math.Round(ScoreData.Accuracy * 100, 2)),
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
                ReplayData = ReplayPath += ".qr"
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
            Logger.Log($"Quaver Version: {Replay.QuaverVersion}", LogColors.GameInfo);
            Logger.Log($"Beatmap MD5: {Replay.BeatmapMd5}", LogColors.GameInfo);
            Logger.Log($"Replay MD5: {Replay.ReplayMd5}", LogColors.GameInfo);
            Logger.Log($"Player: {Configuration.Username}", LogColors.GameInfo);
            Logger.Log($"Date: {Replay.Date.ToString(CultureInfo.InvariantCulture)}", LogColors.GameInfo);
            Logger.Log($"Mods: {GameBase.CurrentGameModifiers.Sum(x => (int)x.ModIdentifier)}", LogColors.GameInfo);
            Logger.Log($"Scroll Speed: {Configuration.ScrollSpeed}", LogColors.GameInfo);
            Logger.Log($"Score: {Replay.Score}", LogColors.GameInfo);
            Logger.Log($"Accuracy: {Replay.Accuracy}%", LogColors.GameInfo);
            Logger.Log($"Max Combo: {Replay.MaxCombo}", LogColors.GameInfo);
            Logger.Log($"Marv Count: {Replay.MarvPressCount + Replay.MarvReleaseCount}", LogColors.GameInfo);
            Logger.Log($"Perf Count: {Replay.PerfPressCount + Replay.PerfReleaseCount}", LogColors.GameInfo);
            Logger.Log($"Great Count: {Replay.GreatPressCount + Replay.GreatReleaseCount}", LogColors.GameInfo);
            Logger.Log($"Good Count: {Replay.GoodPressCount + Replay.GoodReleaseCount}", LogColors.GameInfo);
            Logger.Log($"Okay Count: {Replay.OkayPressCount + Replay.OkayReleaseCount}", LogColors.GameInfo);
            Logger.Log($"Miss Count: {Replay.Misses}", LogColors.GameInfo);
            Logger.Log($"Replay Frame Count: {Replay.ReplayFrames.Count}", LogColors.GameInfo);
        }

        /// <summary>
        ///     Sets the Discord Rich Presence for the score screen state
        /// </summary>
        private void SetDiscordRichPresence()
        {
            // Set Discord Rich Presence w/ score data
            var mapData = $"{GameBase.SelectedBeatmap.Qua.Artist} - {GameBase.SelectedBeatmap.Qua.Title} [{GameBase.SelectedBeatmap.Qua.DifficultyName}]";
            var accuracy = (float)Math.Round(ScoreData.Accuracy * 100, 2);

            GameBase.ChangeDiscordPresence(mapData, $"Finished - {accuracy}% - {Util.GetGradeFromAccuracy(accuracy).ToString()}");
        }
    }
}
