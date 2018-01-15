using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Quaver.API.Enums;
using Quaver.Config;
using Quaver.Database;
using Quaver.Discord;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using Quaver.Replays;
using Quaver.Scores;
using Quaver.Graphics.Text;
using Quaver.GameState.Gameplay;
using Quaver.Utility;
using Quaver.Audio;

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
        ///     Static image of the players' play stats
        /// </summary>
        private BakeableSprite PlayStatsSprite { get; set; }

        private Boundary Boundary { get; set; }

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

            // TODO: Add an audio fade out effect here instead of abruptly stopping it. If failed, it should abruptly stop in the play state. Not here.
            SongManager.Stop();

            // TODO: The failed sound should play in the play state before switching to this one, however this is ok for now.
            ApplauseInstance = (ScoreData.Failed) ? GameBase.LoadedSkin.SoundComboBreak.CreateInstance() : GameBase.LoadedSkin.SoundApplause.CreateInstance();

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

            //Unload ScoreManager Data
            ScoreData.UnloadData();

            // Play Applause
            ApplauseInstance.Play();

            UpdateReady = true;
        }
        
        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            BackButton.Clicked -= OnBackButtonClick;
            BackButton.Destroy();
            Boundary.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            BackButton.Update(dt);
            Boundary.Update(dt);
        }

        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            BackgroundManager.Draw();
            BackButton.Draw();
            Boundary.Draw();
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Creates the UI
        /// </summary>
        private void CreateUI()
        {
            // Create Base Boundary
            Boundary = new Boundary();

            // Create Back Button
            BackButton = new TextButton(new Vector2(150,40),"BACK" )
            {
                Alignment = Alignment.TopRight
            };

            BackButton.Clicked += OnBackButtonClick;

            //create note data graph todo: add text and stuff
            PlayStatsSprite = new BakeableSprite()
            {
                Parent = Boundary,
                ScaleX = 1,
                ScaleY = 1
            };

            CreateJudgeWindowUI();
            CreateMsDevianceUI();
            CreateAccuracyDataUI();
            CreateHealthDataUI();
        }

        /// <summary>
        ///     Back Button Click Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackButtonClick(object sender, EventArgs e)
        {
            ApplauseInstance.Stop(true);
            GameBase.LoadedSkin.SoundBack.Play((float) Configuration.VolumeGlobal / 100 * Configuration.VolumeEffect / 100, 0, 0);
            GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        /// <summary>
        ///     Creates a local score object from the score given
        /// </summary>
        /// <returns></returns>
        private LocalScore CreateLocalScore(Replay rp)
        {
            var grade = (ScoreData.Failed) ? Grades.F : Util.GetGradeFromAccuracy((float) Math.Round(ScoreData.Accuracy * 100, 2));

            // Store the score in the database
            return new LocalScore
            {
                BeatmapMd5 = BeatmapMd5,
                Name = Configuration.Username,
                DateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                Score = ScoreData.ScoreTotal,
                Accuracy = Math.Round(ScoreData.Accuracy * 100, 2),
                Grade = grade,
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
            var grade = (ScoreData.Failed) ? Grades.F : Util.GetGradeFromAccuracy(accuracy);
            var status = (ScoreData.Failed) ? "Failed" : "Finished";

            DiscordController.ChangeDiscordPresence(mapData, $"{status} - {accuracy}% - {grade.ToString()}");
        }

        /// <summary>
        ///     Create UI set relating to note deviance
        /// </summary>
        private void CreateMsDevianceUI()
        {
            // create ms deviance box
            var boundary = new Sprite()
            {
                Size = new UDim2(400, 150),
                Position = new UDim2(10, -10),
                Alignment = Alignment.BotLeft,
                Tint = Color.Black,
                Alpha = 0.5f,
                Parent = PlayStatsSprite
            };

            // create labels for hit windows
            for (var i = 0; i < 5; i++)
            {
                Sprite ob;

                //bottom
                ob = new Sprite()
                {
                    Position = new UDim2(0, boundary.Size.Y.Offset * (ScoreData.HitWindowPress[i] / ScoreData.HitWindowPress[4]) / 2),
                    Size = new UDim2(0, 1, 1, 0),
                    Tint = GameColors.JudgeColors[i],
                    Alpha = 0.2f,
                    Alignment = Alignment.MidLeft,
                    Parent = boundary
                };

                //top
                ob = new Sprite()
                {
                    Position = new UDim2(0, -boundary.Size.Y.Offset * (ScoreData.HitWindowPress[i] / ScoreData.HitWindowPress[4]) / 2),
                    Size = new UDim2(0, 1, 1, 0),
                    Tint = GameColors.JudgeColors[i],
                    Alpha = 0.2f,
                    Alignment = Alignment.MidLeft,
                    Parent = boundary
                };
            }

            // Create time markers
            CreateTimeMarkers(boundary);

            //temp todo: create proper ms deviance display. make this not lag some how
            //record misses
            foreach (var ms in ScoreData.NoteDevianceData)
            {
                if (ms.Value > ScoreData.HitWindowPress[4])
                {
                    var ob = new Sprite()
                    {
                        Position = new UDim2(((float)(ms.Position / ScoreData.PlayTimeTotal) * boundary.Size.X.Offset) - 1f, 0),
                        Size = new UDim2(2, 0, 0, 1),
                        Tint = GameColors.JudgeMiss,
                        Alpha = 0.25f,
                        Parent = boundary
                    };
                }
            }

            //record other offset data
            foreach (var ms in ScoreData.NoteDevianceData)
            {
                if (ms.Value < ScoreData.HitWindowPress[4])
                {
                    int tint = 0;
                    for (tint = 0; tint < 4; tint++)
                    {
                        if (Math.Abs(ms.Value) < ScoreData.HitWindowPress[tint]) break;
                    }

                    var ob = new Sprite()
                    {
                        Position = new UDim2((float)(ms.Position / ScoreData.PlayTimeTotal * boundary.Size.X.Offset) - 1.5f, (float)(ms.Value * (boundary.Size.Y.Offset / 2) / ScoreData.HitWindowPress[4]) - 1.5f),
                        Size = new UDim2(3, 3),
                        Tint = GameColors.JudgeColors[tint],
                        Alignment = Alignment.MidLeft,
                        Parent = boundary
                    };
                }
            }

            CreateAxisLabels(boundary, "Late (+" + Math.Floor(ScoreData.HitWindowPress[4]) + "ms)", "Early (-" + Math.Floor(ScoreData.HitWindowPress[4]) + "ms)");
        }

        /// <summary>
        ///     Create UI set relating to Health
        /// </summary>
        private void CreateHealthDataUI()
        {
            //Create Boundary for Health Data Display
            var boundary = new Sprite()
            {
                Size = new UDim2(400, 150),
                Position = new UDim2(-10, -170),
                Alignment = Alignment.BotRight,
                Tint = Color.Black,
                Alpha = 0.5f,
                Parent = PlayStatsSprite
            };

            // Create time markers
            CreateTimeMarkers(boundary);

            var graphElements = InterpolateGraph(ScoreData.HealthData, boundary.SizeX, boundary.SizeY, 0);
            float scale;
            foreach (var ob in graphElements)
            {
                scale = ob.PosY / boundary.SizeY;
                ob.Tint = new Color(scale, 1 - scale, 0);
                ob.PosX -= 1.5f;
                ob.PosY -= 1.5f;
                ob.Parent = boundary;
            };

            CreateAxisLabels(boundary, "Health 100%", "Health 0%");
        }
        
        /// <summary>
        ///     Create UI set relating to accuracy
        /// </summary>
        private void CreateAccuracyDataUI()
        {
            double lowestAcc = 1;
            foreach (var acc in ScoreData.AccuracyData)
            {
                if (acc.Value < lowestAcc)
                {
                    lowestAcc = acc.Value;
                }
            }
            lowestAcc = Math.Max(0, (Math.Floor(lowestAcc * 1000) / 10) - 2);
            var lowAccRatio = (float)(1 / (100 - lowestAcc));

            //Create Boundary for Accuracy Display
            var boundary = new Sprite()
            {
                Size = new UDim2(400, 150),
                Position = new UDim2(-10, -10),
                Alignment = Alignment.BotRight,
                Tint = Color.Black,
                Alpha = 0.5f,
                Parent = PlayStatsSprite
            };

            // Create time markers
            CreateTimeMarkers(boundary);

            //Create labels for grade windows
            List<Sprite> guides = new List<Sprite>();
            for (var i = 1; i < 7; i++)
            {
                if (ScoreData.GradePercentage[i] > lowestAcc)
                {
                    Sprite ob = new Sprite()
                    {
                        Position = new UDim2(0, boundary.Size.Y.Offset * (float)(1 - ((ScoreData.GradePercentage[i] - lowestAcc) * lowAccRatio))),
                        Size = new UDim2(0, 1, 1, 0),
                        Tint = GameColors.GradeColors[i+1],
                        Alpha = 0.2f,
                        Parent = boundary
                    };
                    guides.Add(ob);
                }
            }

            //Display accuracy chart
            var graphElements = InterpolateGraph(ScoreData.AccuracyData, boundary.SizeX, boundary.SizeY, lowestAcc/100);
            float scale;
            Console.WriteLine(guides.Count);
            foreach (var ob in graphElements)
            {
                Color tint = guides[0].Tint;
                if (ob.PosY <= 0.01)
                {
                    tint = GameColors.GradeColors[7];
                }
                else if (ob.PosY > guides[0].PosY)
                {
                    tint = GameColors.GradeColors[7 - guides.Count];
                }
                else
                {
                    for (int i = 0; i < guides.Count; i++)
                    {
                        if (ob.PosY > guides[i].PosY)
                        {
                            tint = guides[Math.Max(i - 1, 0)].Tint;
                            break;
                        }
                    }
                }

                scale = ob.PosY / boundary.SizeY;
                ob.Tint = tint;
                ob.PosX -= 1.5f;
                ob.PosY -= 1.5f;
                ob.Parent = boundary;
            };


            CreateAxisLabels(boundary, "Acc 100%", $@"Acc {lowestAcc}%");
        }

        /// <summary>
        ///     Create UI displaying judge count and score/acc
        /// </summary>
        private void CreateJudgeWindowUI()
        {
            //Create Judge Info Boundary
            TextBoxSprite ob;
            var boundary = new Boundary()
            {
                Size = new UDim2(350, 240),
                PosX = 10,
                Alignment = Alignment.TopLeft,
                Parent = PlayStatsSprite
            };

            //Create Judge Text
            for (var i = 0; i < 6; i++)
            {
                ob = new TextBoxSprite()
                {
                    Text = "[" + GameplayReferences.JudgeNames[i] + "]: " + ScoreData.JudgePressSpread[i] + " | " + ScoreData.JudgeReleaseSpread[i] + " Total: " + (ScoreData.JudgePressSpread[i] + ScoreData.JudgeReleaseSpread[i]),
                    TextColor = GameColors.JudgeColors[i],
                    Font = Fonts.Medium16,
                    Position = new UDim2(0, 200 * i / 6 + 100),
                    Size = new UDim2(0, 0, 1, 0),
                    TextAlignment = Alignment.MidRight,
                    Parent = boundary
                };
            }

            //Create Score Text
            ob = new TextBoxSprite()
            {
                Text = ScoreData.ScoreTotal.ToString(),
                Font = Fonts.Medium24,
                TextAlignment = Alignment.MidLeft,
                TextColor = Color.White,
                Position = new UDim2(0, 30),
                Size = new UDim2(0, 70, 1, 0),
                Parent = boundary
            };

            //Create Accuracy Text
            ob = new TextBoxSprite()
            {
                Text = $"{ScoreData.Accuracy * 100:0.00}%",
                Font = Fonts.Medium24,
                TextAlignment = Alignment.MidRight,
                TextColor = Color.White,
                Position = new UDim2(0, 30),
                Size = new UDim2(0, 70, 1, 0),
                Parent = boundary
            };
        }

        private List<Sprite> InterpolateGraph(List<GameplayData> graph, float boundarySizeX, float boundarySizeY, double dataLowestY)
        {
            List<Sprite> graphElements = new List<Sprite>();
            double lowestRatio = (1 / (1 - dataLowestY));
            int currentIndex = 0;

            double currentPosition = 0;
            double currentOffset = graph[currentIndex].Value;
            //float sizeX = boundary.Size.X.Offset;
            //float sizeY = boundary.Size.Y.Offset;

            double targetPosition = graph[currentIndex + 1].Position / ScoreData.PlayTimeTotal * boundarySizeX;
            double targetOffset = (graph[currentIndex + 1].Value - dataLowestY) * lowestRatio;
            double splineOffsetSize = (targetOffset - currentOffset);
            double splinePositionSize = Math.Abs(targetPosition - currentPosition);
            double curYpos = currentOffset;
            double target;

            while (currentPosition < boundarySizeX)
            {
                currentPosition += 0.25f;
                if (currentPosition > targetPosition)
                {
                    if (currentIndex < graph.Count - 2)
                    {
                        currentIndex++;
                        targetPosition = (float)graph[currentIndex + 1].Position / ScoreData.PlayTimeTotal * boundarySizeX;
                        targetOffset = (graph[currentIndex + 1].Value - dataLowestY) * lowestRatio;
                        currentOffset = (graph[currentIndex].Value - dataLowestY) * lowestRatio;
                        splineOffsetSize = targetOffset - currentOffset;
                        splinePositionSize = Math.Abs(targetPosition - currentPosition);
                    }
                    else
                        break;
                }

                target = (1 - (targetPosition - currentPosition) / splinePositionSize) * splineOffsetSize + currentOffset;
                curYpos += ((1 - target) * boundarySizeY - curYpos) / 12f;

                var ob = new Sprite()
                {
                    Position = new UDim2((float)currentPosition, (float)curYpos),
                    Size = new UDim2(3, 3),
                };
                graphElements.Add(ob);
            }
            return graphElements;
        }

        private void CreateAxisLabels(Drawable parent, string topLabel, string botLabel)
        {
            //top
            var label = new TextBoxSprite()
            {
                Text = topLabel,
                Font = Fonts.Medium12,
                Position = new UDim2(2, 2),
                Size = new UDim2(200, 50),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.TopLeft,
                Parent = parent
            };

            //bottom
            label = new TextBoxSprite()
            {
                Text = botLabel,
                Font = Fonts.Medium12,
                Position = new UDim2(2, -2),
                Size = new UDim2(200, 50),
                Alignment = Alignment.BotLeft,
                TextAlignment = Alignment.BotLeft,
                Parent = parent
            };
        }

        private void CreateTimeMarkers(Drawable parent)
        {
            //Record time intervals on graph every 15 seconds
            int timeIndex = 1;
            while (timeIndex * 15000 < ScoreData.PlayTimeTotal)
            {
                var ob = new Sprite()
                {
                    Position = new UDim2(parent.Size.X.Offset * (float)((timeIndex * 15000) / ScoreData.PlayTimeTotal), 0),
                    Size = new UDim2(1, 0, 0, 1),
                    Alpha = timeIndex % 4 == 0 ? 0.5f : 0.1f,
                    Parent = parent
                };

                timeIndex++;
            }
        }
    }
}
