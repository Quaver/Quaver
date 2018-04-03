using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using osu_database_reader;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Graphics.UserInterface;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Replays;
using Quaver.Graphics.Text;
using Quaver.Audio;
using Quaver.Net;
using Quaver.Net.Structures;
using Quaver.Online;
using Quaver.States.Enums;
using Quaver.States.Gameplay.Mania;
using Quaver.States.Gameplay.Mania.Components.Scoring;
using Quaver.States.Select;

namespace Quaver.States.Results
{
    internal class ResultsState : IGameState
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
        ///     The MD5 Hash of the map.
        /// </summary>
        private string MapMd5 { get; }

        /// <summary>
        ///     The score data from the previous gameplay session.
        /// </summary>
        private ManiaScoreManager ManiaScoreData { get; }

        /// <summary>
        ///     The artist of the played map.
        /// </summary>
        private string Artist { get; }

        /// <summary>
        ///     The title of the played map
        /// </summary>
        private string Title { get; }

        /// <summary>
        ///     The difficulty name of the played map
        /// </summary>
        private string DifficultyName { get; }

        /// <summary>
        ///     The button to get back to song select
        /// </summary>
        private QuaverTextButton BackButton { get; set; }

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
        private QuaverBakeableSprite PlayStatsSprite { get; set; }

        private QuaverContainer QuaverContainer { get; set; }

        /// <summary>
        ///     Constructor - In order to get to this state, it's essential that you pass in 
        ///     the map md5 and the score data.
        /// </summary>
        /// <param name="mapMd5"></param>
        /// <param name="maniaScoreData"></param>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <param name="difficultyName"></param>
        public ResultsState(string mapMd5, ManiaScoreManager maniaScoreData, string artist, string title, string difficultyName, List<ReplayFrame> replayFrames)
        {
            // Initialize data
            MapMd5 = mapMd5;
            ManiaScoreData = maniaScoreData;
            Artist = artist;
            Title = title;
            DifficultyName = difficultyName;
            ReplayFrames = replayFrames;
            Replay = CreateReplayFromScore();

            ReplayPath = $"{ConfigManager.Username} - {Artist} - {Title} [{DifficultyName}] ({DateTime.UtcNow})";

            // TODO: Add an audio fade out effect here instead of abruptly stopping it. If failed, it should abruptly stop in the play state. Not here.
            try
            {
                GameBase.AudioEngine.Stop();
            }
            catch (AudioEngineException e)
            {
                // No need to handle this. This will only be thrown if the audio stream isn't actually loaded.
                // so it's fine.
            }
            
            // TODO: The failed sound should play in the play state before switching to this one, however this is ok for now.
            ApplauseInstance = (ManiaScoreData.Failed) ? GameBase.LoadedSkin.SoundComboBreak.CreateInstance() : GameBase.LoadedSkin.SoundApplause.CreateInstance();

            // Insert the score into the database
            Task.Run(async () =>
            {
                // Submit score
                SubmitScore();

                // Write replay to log file if debug is toggled
                Replay.WriteToLogFile();
                Replay.Write(ReplayPath, true);

                try
                {
                    var newScore = CreateLocalScore(Replay);

                    // Insert the score in the DB.
                    await LocalScoreCache.InsertScoreIntoDatabase(newScore);

                    var previousScores = await LocalScoreCache.FetchMapScores(mapMd5);

                    if (previousScores.Count > 0)
                        previousScores = previousScores.OrderByDescending(x => x.Score).ToList();

                    // If this score is higher than the last or there are no scores, we want to update this 
                    // map's cache.
                    if (previousScores.Count > 0 && maniaScoreData.ScoreTotal >= previousScores[0].Score)
                        GameBase.SelectedMap.HighestRank = newScore.Grade;

                    GameBase.SelectedMap.LastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    await MapCache.UpdateMap(GameBase.SelectedMap);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, LogType.Runtime);
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
            // Iniitalize QuaverUserInterface Elements
            CreateUI();

            // Log the score
            LogScore();

            //Unload ManiaScoreManager Data
            ManiaScoreData.UnloadData();

            // Play Applause
            ApplauseInstance.Volume = GameBase.SoundEffectVolume;
            ApplauseInstance.Play();

            // Update overlay
            GameBase.GameOverlay.OverlayActive = true;

            UpdateReady = true;
        }
        
        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            BackButton.Clicked -= OnBackButtonClick;
            BackButton.Destroy();
            QuaverContainer.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            BackButton.Update(dt);
            QuaverContainer.Update(dt);
        }

        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            BackgroundManager.Draw();
            BackButton.Draw();
            QuaverContainer.Draw();
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Creates the QuaverUserInterface
        /// </summary>
        private void CreateUI()
        {
            // Create Base QuaverContainer
            QuaverContainer = new QuaverContainer();

            // Create Back QuaverButton
            BackButton = new QuaverTextButton(new Vector2(150,40),"BACK" )
            {
                PosY = 70,
                Alignment = Alignment.TopRight
            };

            BackButton.Clicked += OnBackButtonClick;

            //create note data graph todo: add text and stuff
            PlayStatsSprite = new QuaverBakeableSprite()
            {
                Parent = QuaverContainer,
                ScaleX = 1,
                ScaleY = 1
            };

            CreateJudgeWindowUI();
            CreateMsDevianceUI();
            CreateAccuracyDataUI();
            CreateHealthDataUI();
        }

        /// <summary>
        ///     Back QuaverButton Click Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackButtonClick(object sender, EventArgs e)
        {
            ApplauseInstance.Stop(true);
            GameBase.LoadedSkin.SoundBack.Play(GameBase.SoundEffectVolume, 0, 0);
            GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        /// <summary>
        ///     Creates a local score object from the score given
        /// </summary>
        /// <returns></returns>
        private LocalScore CreateLocalScore(Replay rp)
        {
            var grade = (ManiaScoreData.Failed) ? Grades.F : GradeHelper.GetGradeFromAccuracy((float) Math.Round(ManiaScoreData.Accuracy * 100, 2));

            // Store the score in the database
            return new LocalScore
            {
                MapMd5 = MapMd5,
                Name = ConfigManager.Username,
                DateTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                Score = ManiaScoreData.ScoreTotal,
                Accuracy = Math.Round(ManiaScoreData.Accuracy * 100, 2),
                Grade = grade,
                MaxCombo = ManiaScoreData.MaxCombo,
                MarvPressCount = ManiaScoreData.JudgePressSpread[0],
                MarvReleaseCount = ManiaScoreData.JudgeReleaseSpread[0],
                PerfPressCount = ManiaScoreData.JudgePressSpread[1],
                PerfReleaseCount = ManiaScoreData.JudgeReleaseSpread[1],
                GreatPressCount = ManiaScoreData.JudgePressSpread[2],
                GreatReleaseCount = ManiaScoreData.JudgeReleaseSpread[2],
                GoodPressCount = ManiaScoreData.JudgePressSpread[3],
                GoodReleaseCount = ManiaScoreData.JudgeReleaseSpread[3],
                OkayPressCount = ManiaScoreData.JudgePressSpread[4],
                OkayReleaseCount = ManiaScoreData.JudgeReleaseSpread[4],
                Misses = ManiaScoreData.JudgePressSpread[5] + ManiaScoreData.JudgeReleaseSpread[5],
                Rating = 0.0f,
                Mods = GameBase.CurrentGameModifiers.Sum(x => (int)x.ModIdentifier),
                ScrollSpeed = ManiaScoreData.ScrollSpeed,
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
                MapMd5 = MapMd5,
                ReplayMd5 = "Not Implemented",
                Name = ConfigManager.Username,
                Date = DateTime.UtcNow,
                ScrollSpeed = ManiaScoreData.ScrollSpeed,
                Score = ManiaScoreData.ScoreTotal,
                Accuracy = (float)Math.Round(ManiaScoreData.Accuracy * 100, 2),
                MaxCombo = ManiaScoreData.MaxCombo,
                MarvPressCount = ManiaScoreData.JudgePressSpread[0],
                MarvReleaseCount = ManiaScoreData.JudgeReleaseSpread[0],
                PerfPressCount = ManiaScoreData.JudgePressSpread[1],
                PerfReleaseCount = ManiaScoreData.JudgeReleaseSpread[1],
                GreatPressCount = ManiaScoreData.JudgePressSpread[2],
                GreatReleaseCount = ManiaScoreData.JudgeReleaseSpread[2],
                GoodPressCount = ManiaScoreData.JudgePressSpread[3],
                GoodReleaseCount = ManiaScoreData.JudgeReleaseSpread[3],
                OkayPressCount = ManiaScoreData.JudgePressSpread[4],
                OkayReleaseCount = ManiaScoreData.JudgeReleaseSpread[4],
                Misses = ManiaScoreData.JudgePressSpread[5] + ManiaScoreData.JudgeReleaseSpread[5],
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
            Logger.LogImportant($"Quaver Version: {Replay.QuaverVersion}", LogType.Runtime);
            Logger.LogImportant($"Map MD5: {Replay.MapMd5}", LogType.Runtime);
            Logger.LogImportant($"Replay MD5: {Replay.ReplayMd5}", LogType.Runtime);
            Logger.LogImportant($"Player: {ConfigManager.Username}", LogType.Runtime);
            Logger.LogImportant($"Date: {Replay.Date.ToString(CultureInfo.InvariantCulture)}", LogType.Runtime);
            Logger.LogImportant($"Mods: {GameBase.CurrentGameModifiers.Sum(x => (int)x.ModIdentifier)}", LogType.Runtime);
            Logger.LogImportant($"Scroll ManiaModSpeed: {ManiaScoreData.ScrollSpeed}", LogType.Runtime);
            Logger.LogImportant($"Score: {Replay.Score}", LogType.Runtime);
            Logger.LogImportant($"Accuracy: {Replay.Accuracy}%", LogType.Runtime);
            Logger.LogImportant($"Max Combo: {Replay.MaxCombo}", LogType.Runtime);
            Logger.LogImportant($"Marv Count: {Replay.MarvPressCount + Replay.MarvReleaseCount}", LogType.Runtime);
            Logger.LogImportant($"Perf Count: {Replay.PerfPressCount + Replay.PerfReleaseCount}", LogType.Runtime);
            Logger.LogImportant($"Great Count: {Replay.GreatPressCount + Replay.GreatReleaseCount}", LogType.Runtime);
            Logger.LogImportant($"Good Count: {Replay.GoodPressCount + Replay.GoodReleaseCount}", LogType.Runtime);
            Logger.LogImportant($"Okay Count: {Replay.OkayPressCount + Replay.OkayReleaseCount}", LogType.Runtime);
            Logger.LogImportant($"Miss Count: {Replay.Misses}", LogType.Runtime);
            Logger.LogImportant($"Replay Frame Count: {Replay.ReplayFrames.Count}", LogType.Runtime);
        }

        /// <summary>
        ///     Sets the Discord Rich Presence for the score screen state
        /// </summary>
        private void SetDiscordRichPresence()
        {
            // Set Discord Rich Presence w/ score data
            var mapData = $"{GameBase.SelectedMap.Qua.Artist} - {GameBase.SelectedMap.Qua.Title} [{GameBase.SelectedMap.Qua.DifficultyName}]";
            var accuracy = (float)Math.Round(ManiaScoreData.Accuracy * 100, 2);
            var grade = (ManiaScoreData.Failed) ? Grades.F : GradeHelper.GetGradeFromAccuracy(accuracy);
            var status = (ManiaScoreData.Failed) ? "Failed - " : "Finished -";

            DiscordController.ChangeDiscordPresence(mapData, $"{status} {accuracy}% {grade.ToString()} {ManiaScoreData.MaxCombo}x");
        }

        /// <summary>
        ///     Create QuaverUserInterface set relating to note deviance
        /// </summary>
        private void CreateMsDevianceUI()
        {
            // create ms deviance box
            var boundary = new QuaverSprite()
            {
                Size = new UDim2D(400, 150),
                Position = new UDim2D(10, -90),
                Alignment = Alignment.BotLeft,
                Tint = Color.Black,
                Alpha = 0.5f,
                Parent = PlayStatsSprite
            };

            // create labels for hit windows
            for (var i = 0; i < 5; i++)
            {
                QuaverSprite ob;

                //bottom
                ob = new QuaverSprite()
                {
                    Position = new UDim2D(0, boundary.Size.Y.Offset * (ManiaScoreData.HitWindowPress[i] / ManiaScoreData.HitWindowPress[4]) / 2),
                    Size = new UDim2D(0, 1, 1, 0),
                    Tint = GameBase.LoadedSkin.JudgeColors[i],
                    Alpha = 0.2f,
                    Alignment = Alignment.MidLeft,
                    Parent = boundary
                };

                //top
                ob = new QuaverSprite()
                {
                    Position = new UDim2D(0, -boundary.Size.Y.Offset * (ManiaScoreData.HitWindowPress[i] / ManiaScoreData.HitWindowPress[4]) / 2),
                    Size = new UDim2D(0, 1, 1, 0),
                    Tint = GameBase.LoadedSkin.JudgeColors[i],
                    Alpha = 0.2f,
                    Alignment = Alignment.MidLeft,
                    Parent = boundary
                };
            }

            // Create time markers
            CreateTimeMarkers(boundary);

            //temp todo: create proper ms deviance display. make this not lag some how
            //record misses
            foreach (var ms in ManiaScoreData.NoteDevianceData)
            {
                if (ms.Value > ManiaScoreData.HitWindowPress[4])
                {
                    var ob = new QuaverSprite()
                    {
                        Position = new UDim2D(((float)(ms.Position / ManiaScoreData.PlayTimeTotal) * boundary.Size.X.Offset) - 1f, 0),
                        Size = new UDim2D(2, 0, 0, 1),
                        Tint = GameBase.LoadedSkin.GetJudgeColor(Judge.Miss),
                        Alpha = 0.25f,
                        Parent = boundary
                    };
                }
            }

            //record other offset data
            foreach (var ms in ManiaScoreData.NoteDevianceData)
            {
                if (ms.Value < ManiaScoreData.HitWindowPress[4])
                {
                    int tint = 0;
                    for (tint = 0; tint < 4; tint++)
                    {
                        if (Math.Abs(ms.Value) < ManiaScoreData.HitWindowPress[tint]) break;
                    }

                    var ob = new QuaverSprite()
                    {
                        Position = new UDim2D((float)(ms.Position / ManiaScoreData.PlayTimeTotal * boundary.Size.X.Offset) - 1.5f, (float)(ms.Value * (boundary.Size.Y.Offset / 2) / ManiaScoreData.HitWindowPress[4]) - 1.5f),
                        Size = new UDim2D(3, 3),
                        Tint = GameBase.LoadedSkin.JudgeColors[tint],
                        Alignment = Alignment.MidLeft,
                        Parent = boundary
                    };
                }
            }

            CreateAxisLabels(boundary, "Late (+" + Math.Floor(ManiaScoreData.HitWindowPress[4]) + "ms)", "Early (-" + Math.Floor(ManiaScoreData.HitWindowPress[4]) + "ms)");
        }

        /// <summary>
        ///     Create QuaverUserInterface set relating to Health
        /// </summary>
        private void CreateHealthDataUI()
        {
            //Create QuaverContainer for Health Data Display
            var boundary = new QuaverSprite()
            {
                Size = new UDim2D(400, 150),
                Position = new UDim2D(-10, -160 - 90),
                Alignment = Alignment.BotRight,
                Tint = Color.Black,
                Alpha = 0.5f,
                Parent = PlayStatsSprite
            };

            // Create time markers
            CreateTimeMarkers(boundary);

            var graphElements = InterpolateGraph(ManiaScoreData.HealthData, boundary.SizeX, boundary.SizeY, 0);
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
        ///     Create QuaverUserInterface set relating to accuracy
        /// </summary>
        private void CreateAccuracyDataUI()
        {
            double lowestAcc = 1;
            foreach (var acc in ManiaScoreData.AccuracyData)
            {
                if (acc.Value < lowestAcc)
                {
                    lowestAcc = acc.Value;
                }
            }
            lowestAcc = Math.Max(0, (Math.Floor(lowestAcc * 1000) / 10) - 2);
            var lowAccRatio = (float)(1 / (100 - lowestAcc));

            //Create QuaverContainer for Accuracy Display
            var boundary = new QuaverSprite()
            {
                Size = new UDim2D(400, 150),
                Position = new UDim2D(-10, -90),
                Alignment = Alignment.BotRight,
                Tint = Color.Black,
                Alpha = 0.5f,
                Parent = PlayStatsSprite
            };

            // Create time markers
            CreateTimeMarkers(boundary);

            //Create labels for grade windows
            List<QuaverSprite> guides = new List<QuaverSprite>();
            for (var i = 1; i < 7; i++)
            {
                if (ManiaScoreData.GradePercentage[i] > lowestAcc)
                {
                    QuaverSprite ob = new QuaverSprite()
                    {
                        Position = new UDim2D(0, boundary.Size.Y.Offset * (float)(1 - ((ManiaScoreData.GradePercentage[i] - lowestAcc) * lowAccRatio))),
                        Size = new UDim2D(0, 1, 1, 0),
                        Tint = QuaverColors.GradeColors[i+1],
                        Alpha = 0.2f,
                        Parent = boundary
                    };
                    guides.Add(ob);
                }
            }

            //Display accuracy chart
            var graphElements = InterpolateGraph(ManiaScoreData.AccuracyData, boundary.SizeX, boundary.SizeY, lowestAcc/100);
            float scale;
            Console.WriteLine(guides.Count);
            foreach (var ob in graphElements)
            {
                Color tint = guides[0].Tint;
                if (ob.PosY <= 0.01)
                {
                    tint = QuaverColors.GradeColors[7];
                }
                else if (ob.PosY > guides[0].PosY)
                {
                    tint = QuaverColors.GradeColors[7 - guides.Count];
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
        ///     Create QuaverUserInterface displaying judge count and score/acc
        /// </summary>
        private void CreateJudgeWindowUI()
        {
            //Create Judge Info QuaverContainer
            QuaverTextbox ob;
            var boundary = new QuaverContainer()
            {
                Size = new UDim2D(350, 240),
                PosX = 10,
                PosY = 50,
                Alignment = Alignment.TopCenter,
                Parent = PlayStatsSprite
            };

            //Create Judge Text
            for (var i = 0; i < 6; i++)
            {
                ob = new QuaverTextbox()
                {
                    Text = "[" + ManiaGameplayReferences.JudgeNames[i] + "]: " + ManiaScoreData.JudgePressSpread[i] + " | " + ManiaScoreData.JudgeReleaseSpread[i] + " Total: " + (ManiaScoreData.JudgePressSpread[i] + ManiaScoreData.JudgeReleaseSpread[i]),
                    TextColor = GameBase.LoadedSkin.JudgeColors[i],
                    Font = Fonts.Medium16,
                    Position = new UDim2D(0, 200 * i / 6 + 100),
                    Size = new UDim2D(0, 0, 1, 0),
                    TextAlignment = Alignment.MidRight,
                    Parent = boundary
                };
            }

            //Create Score Text
            ob = new QuaverTextbox()
            {
                Text = ManiaScoreData.ScoreTotal.ToString(),
                Font = Fonts.Medium24,
                TextAlignment = Alignment.MidLeft,
                TextColor = Color.White,
                Position = new UDim2D(0, 30),
                Size = new UDim2D(0, 70, 1, 0),
                Parent = boundary
            };

            //Create Accuracy Text
            ob = new QuaverTextbox()
            {
                Text = $"{ManiaScoreData.Accuracy * 100:0.00}%",
                Font = Fonts.Medium24,
                TextAlignment = Alignment.MidRight,
                TextColor = Color.White,
                Position = new UDim2D(0, 30),
                Size = new UDim2D(0, 70, 1, 0),
                Parent = boundary
            };

            // Create a hollow pi chart
            CreateJudgeSpreadPiChart(boundary, 75, new Vector2(0, 200));
        }

        /// <summary>
        ///     Returns a list of graph elements used to display a curve for a set of gameplay data.
        /// </summary>
        /// <param name="graph">The list of gameplay data</param>
        /// <param name="boundarySizeX">Size of boundary parent (x offset)</param>
        /// <param name="boundarySizeY">Size of boundary parent (y offset)</param>
        /// <param name="dataLowestY">Lowest point of the data set</param>
        /// <returns></returns>
        private List<QuaverSprite> InterpolateGraph(List<ManiaGameplayData> graph, float boundarySizeX, float boundarySizeY, double dataLowestY)
        {
            // Create graph elements and any references to object
            List<QuaverSprite> graphElements = new List<QuaverSprite>();
            double lowestRatio = (1 / (1 - dataLowestY));
            int currentIndex = 0;

            // Current position and offset values
            double currentXPos = 0;
            double currentOffset = graph[currentIndex].Value;
            double currentYpos = currentOffset;

            // Target values
            double targetPosition = graph[currentIndex + 1].Position / ManiaScoreData.PlayTimeTotal * boundarySizeX;
            double targetOffset = (graph[currentIndex + 1].Value - dataLowestY) * lowestRatio;
            double target;

            // Splining
            double splineOffsetSize = (targetOffset - currentOffset);
            double splinePositionSize = Math.Abs(targetPosition - currentXPos);
            double splineTarget;
            double splineSign;

            // This algorithm first calculates the slope between two points and creates the graph elements accordingly
            // The algorithm simultaneously applies a B-Spline algorithm to smoothen the curve of the graph
            while (currentXPos < boundarySizeX)
            {
                // Calculate currentY value of graph
                target = (1 - (targetPosition - currentXPos) / splinePositionSize) * splineOffsetSize + currentOffset;
                splineTarget = ((1 - target) * boundarySizeY - currentYpos) / 6;
                splineSign = Math.Sign(splineTarget);
                splineTarget = Math.Min(Math.Abs(splineTarget), 2.9);
                currentYpos += splineSign * splineTarget;

                // Calculate current X value of graph
                currentXPos += 1 / (1 + splineTarget);//Math.Abs((targetPosition - currentXPos) / splinePositionSize));

                // If the current X value is greater than the X value of the next point, change the current index to the next point
                if (currentXPos > targetPosition)
                {
                    if (currentIndex < graph.Count - 2)
                    {
                        currentIndex++;
                        targetPosition = (float)graph[currentIndex + 1].Position / ManiaScoreData.PlayTimeTotal * boundarySizeX;

                        while (currentIndex < graph.Count - 2 && currentXPos > targetPosition)
                        {
                            currentIndex++;
                            targetPosition = (float)graph[currentIndex + 1].Position / ManiaScoreData.PlayTimeTotal * boundarySizeX;
                        }

                        targetOffset = (graph[currentIndex + 1].Value - dataLowestY) * lowestRatio;
                        currentOffset = (graph[currentIndex].Value - dataLowestY) * lowestRatio;
                        splineOffsetSize = targetOffset - currentOffset;
                        splinePositionSize = Math.Abs(targetPosition - currentXPos);
                    }
                    else
                        break;
                }

                // Create graph element
                var ob = new QuaverSprite()
                {
                    Position = new UDim2D((float)currentXPos, (float)currentYpos),
                    Size = new UDim2D(3, 3)
                };
                graphElements.Add(ob);
            }
            return graphElements;
        }

        /// <summary>
        ///     Create 2 Y-axis labels for a graph.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="topLabel"></param>
        /// <param name="botLabel"></param>
        private void CreateAxisLabels(Drawable parent, string topLabel, string botLabel)
        {
            //top
            var label = new QuaverTextbox()
            {
                Text = topLabel,
                Font = Fonts.Medium12,
                Position = new UDim2D(2, 2),
                Size = new UDim2D(200, 50),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.TopLeft,
                Parent = parent
            };

            //bottom
            label = new QuaverTextbox()
            {
                Text = botLabel,
                Font = Fonts.Medium12,
                Position = new UDim2D(2, -2),
                Size = new UDim2D(200, 50),
                Alignment = Alignment.BotLeft,
                TextAlignment = Alignment.BotLeft,
                Parent = parent
            };
        }

        /// <summary>
        ///     Create time markers for a graph.
        /// </summary>
        /// <param name="parent"></param>
        private void CreateTimeMarkers(Drawable parent)
        {
            //Record time intervals on graph every 15 seconds
            int timeIndex = 1;
            while (timeIndex * 15000 < ManiaScoreData.PlayTimeTotal)
            {
                var ob = new QuaverSprite()
                {
                    Position = new UDim2D(parent.Size.X.Offset * (float)((timeIndex * 15000) / ManiaScoreData.PlayTimeTotal), 0),
                    Size = new UDim2D(1, 0, 0, 1),
                    Alpha = timeIndex % 4 == 0 ? 0.5f : 0.1f,
                    Parent = parent
                };

                timeIndex++;
            }
        }

        /// <summary>
        ///     Creates a hollow pi graph with data set from the player's judge sprad
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private void CreateJudgeSpreadPiChart(QuaverContainer parent, double radius, Vector2 offset)
        {
            // Variables used for calculation
            int[] totalSpreadCount = new int[6];
            int totalJudgeCount = 0;
            double[] judgeSpreadRatio = new double[6];
            double circularRatio = 0.02 * Math.PI;

            // Drawing/looping variables
            int tint = 0;
            double position = 0;
            //double drawPause = 0.5;

            // How much to rotate by in percentage
            double interval = 20 / (Math.PI * radius);

            // Calculate count/ratio variables
            for (var i = 0; i < 6; i++)
            {
                totalSpreadCount[i] = ManiaScoreData.JudgePressSpread[i] + ManiaScoreData.JudgeReleaseSpread[i];
                totalJudgeCount += totalSpreadCount[i];
                judgeSpreadRatio[i] = totalJudgeCount;
            }

            // Calculate judge count ratio
            for (var i = 0; i < 6; i++)
                judgeSpreadRatio[i] = judgeSpreadRatio[i] / totalJudgeCount;

            // This algorithm loops through by drawing each section by rotation (determined by postion)
            // It will draw a hollow pi graph with "dots" aka 2x2 squares
            while (position < 100)
            {
                // If a section is done drawing, it will switch to the next color
                // It will also stop drawing for 1% of the graph to allow a void inbetween each section
                if (tint < 5 && position / 100 >= judgeSpreadRatio[tint])
                {
                    tint = tint + 1;
                    //drawPause = 0.5;
                }

                // If it's not "paused," it will keep on drawing
                //if (drawPause <= 0)
                    for (int i =0; i< 14; i++)
                    {
                        var ob = new QuaverSprite()
                        {
                            Position = new UDim2D
                            (
                                (float)(Math.Cos(position * circularRatio - Math.PI / 2) * (radius - (2.5 * i))) + offset.X,
                                (float)(Math.Sin(position * circularRatio - Math.PI / 2) * (radius - (2.5 * i))) + offset.Y
                            ),
                            Tint = GameBase.LoadedSkin.JudgeColors[tint],
                            Size = new UDim2D(3, 3),
                            Parent = parent
                        };
                    }

                // Update position and draw pausing
                position += interval;
                //if (drawPause > 0) drawPause -= interval;
            }
        }

        /// <summary>
        ///     Submits a score to the server.
        /// </summary>
        private void SubmitScore()
        {
            if (!Flamingo.Connected)
                return;

            // Get Replay
            var replay = LZMACoder.Compress(Encoding.ASCII.GetBytes(ReplayHelper.ReplayFramesToString(Replay.ReplayFrames)));

            // Get the current scroll speed the user used.
            int scrollSpeed;
            switch (GameBase.SelectedMap.Mode)
            {
                case GameModes.Keys4:
                    scrollSpeed = ConfigManager.ScrollSpeed4k;
                    break;
                case GameModes.Keys7:
                    scrollSpeed = ConfigManager.ScrollSpeed7k;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Invalid game mode for map selected!");
            }

            // Get current game mods
            ModIdentifier mods = 0;
            GameBase.CurrentGameModifiers.ForEach(x => mods |= x.ModIdentifier);

            // Create OnlineScore object.
            FlamingoRequests.SubmitScore(new OnlineScore(MapMd5, ManiaScoreData.Failed, replay, ManiaScoreData.PlayTimeTotal, ManiaScoreData.ScoreTotal, ManiaScoreData.Accuracy,
                ManiaScoreData.MaxCombo, scrollSpeed, ManiaScoreData.JudgePressSpread, ManiaScoreData.JudgeReleaseSpread, GameBase.AudioEngine.PlaybackRate,
                GameBase.GameTime.ElapsedMilliseconds, GameBase.BuildVersion, mods, ConfigManager.Username, SteamworksHelper.PTicket, 
                GameBase.SelectedMap.Mode));
        }
    }
}
