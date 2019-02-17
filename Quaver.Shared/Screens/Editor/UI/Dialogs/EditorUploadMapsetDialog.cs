/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Server.Client;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using SQLite;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs
{
    public class EditorUploadMapsetDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private Sprite ContainingBox { get; set; }

        /// <summary>
        /// </summary>
        private Line TopLine { get; set; }

        /// <summary>
        ///     The line that divides AreYouSure and the buttons.
        /// </summary>
        public Line DividerLine { get; set; }

        /// <summary>
        /// </summary>
        private Sprite LoadingWheel { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText Header { get; set; }

        /// <summary>
        /// </summary>
        public TextButton CloseButton { get; set; }

        /// <summary>
        /// </summary>
        private UploadMapsetResponse Response { get; set; }

        /// <summary>
        /// </summary>
        private TextButton VisitMapsetPageButton { get; set; }

        /// <summary>
        /// </summary>
        private Dictionary<MapsetSubmissionStatusCode, string> StatusCodeMessages { get; } = new Dictionary<MapsetSubmissionStatusCode, string>()
        {
            {MapsetSubmissionStatusCode.ErrorInternalServer, "Error: An internal server error has occurred. Please try again!"},
            {MapsetSubmissionStatusCode.ErrorUnauthorized, "Error:  Please re-log and try again!"},
            {MapsetSubmissionStatusCode.ErrorBanned, "Error: You cannot upload a mapset while banned!"},
            {MapsetSubmissionStatusCode.ErrorNoMapsetSent, "Error: No mapset was sent with the request!"},
            {MapsetSubmissionStatusCode.ErrorRequestTooLarge, "Error: Your mapset is too large! The max file size is 15mb"},
            {MapsetSubmissionStatusCode.ErrorInvalidMapsetFileExtension, "Error: File extension error. Please report this to a developer!"},
            {MapsetSubmissionStatusCode.ErrorMapsetNotZipFile, "Error: Your mapset was not a zip file. Please report this to a developer!"},
            {MapsetSubmissionStatusCode.ErrorInvalidFileType, "Error: Your mapset contains files of an invalid type"},
            {MapsetSubmissionStatusCode.ErrorNoContainingQuaFiles, "Error: Your mapset does not contain any .qua files!"},
            {MapsetSubmissionStatusCode.ErrorInvalidMapFile, "Error: One or more of your map files are invalid! (No objects, timing points?)"},
            {MapsetSubmissionStatusCode.ErrorInvalidMapMetadata, "Error: One or more of your map files do not have completed metadata"},
            {MapsetSubmissionStatusCode.ErrorUserNotCreator, "Error: You are not the creator of this mapset!"},
            {MapsetSubmissionStatusCode.ErrorConflictingMapIds, "Error: One or more of your maps have conflicting map ids!"},
            {MapsetSubmissionStatusCode.ErrorConflictingMapsetIds, "Error: One or more of your maps have conflicting mapset ids!"},
            {MapsetSubmissionStatusCode.ErrorConflictingDifficultyNames, "Error: One or more of your maps have the same difficulty name!"},
            {MapsetSubmissionStatusCode.ErrorNoExistingMapsetFound, "Error: You're trying to update a mapset, but this mapset isn't uploaded online!"},
            {MapsetSubmissionStatusCode.ErrorAlreadyRanked, "Error: You cannot update a mapset that is already ranked!"},
            {MapsetSubmissionStatusCode.ErrorContainsNonUploadedNotNewMapId, "Error: One or more of youir maps contains a non-uploaded map id!"},
            {MapsetSubmissionStatusCode.SuccessUploaded, "Success! Your mapset has been uploaded!"},
            {MapsetSubmissionStatusCode.SuccessUpdated, "Success! Your mapset has been updated!"},
            {MapsetSubmissionStatusCode.ErrorExceededLimit, "Error: You have exceeded the amount of maps you can upload at this time!"}
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorUploadMapsetDialog() : base(0.75f)
        {
            var game = GameBase.Game as QuaverGame;
            var screen = game?.CurrentScreen as EditorScreen;
            screen.UploadInProgress = true;

            CreateContent();
            UploadMapset();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateContainingBox();
            CreateTopLine();
            CreateHeader();
            CreateDividerLine();
            CreateLoadingWheel();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            HandleLoadingWheelAnimations();

            var tint = Color.Crimson;

            if (Response != null && (Response.Code == MapsetSubmissionStatusCode.SuccessUpdated || Response.Code == MapsetSubmissionStatusCode.SuccessUploaded))
                tint = Color.LimeGreen;

            CloseButton?.Border.FadeToColor(CloseButton.IsHovered ? Color.White : tint, dt, 60);
            CloseButton?.Text.FadeToColor(CloseButton.IsHovered ? Color.White : tint, dt, 60);
            VisitMapsetPageButton?.Border.FadeToColor(VisitMapsetPageButton.IsHovered ? Color.White : Colors.MainAccent, dt, 60);
            VisitMapsetPageButton?.Text.FadeToColor(VisitMapsetPageButton.IsHovered ? Color.White : Colors.MainAccent, dt, 60);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
        }

        /// <summary>
        /// </summary>
        private void CreateContainingBox() => ContainingBox = new Sprite
        {
            Parent = Container,
            Size = new ScalableVector2(WindowManager.Width, 165),
            Alignment = Alignment.MidCenter,
            Tint = Color.Black,
            Alpha = 0.85f,
        };

        /// <summary>
        /// </summary>
        private void CreateTopLine() => TopLine = new Line(new Vector2(WindowManager.Width, ContainingBox.AbsolutePosition.Y),
            Colors.MainAccent, 2)
        {
            Parent = ContainingBox,
            Alpha = 0.75f,
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        ///     Creates the heading text
        /// </summary>
        private void CreateHeader() => Header = new SpriteText(Fonts.Exo2Medium, "Uploading Mapset. Please wait...", 16)
        {
            Parent = ContainingBox,
            Y = 25,
            Alignment = Alignment.TopCenter
        };

        /// <summary>
        ///     Creates the divider line text.
        /// </summary>
        private void CreateDividerLine()
        {
            DividerLine = new Line(Vector2.Zero, Colors.MainAccent, 1)
            {
                Parent = Header,
                Alignment = Alignment.TopCenter,
                UsePreviousSpriteBatchOptions = true,
                Y = Header.Height + 20,
                Alpha = 0.75f
            };

            const int lineWidth = 100;
            DividerLine.EndPosition = new Vector2(DividerLine.AbsolutePosition.X + lineWidth, DividerLine.AbsolutePosition.Y);
            DividerLine.X -= lineWidth;
        }


        /// <summary>
        /// </summary>
        private void CreateLoadingWheel() => LoadingWheel = new Sprite
        {
            Parent = ContainingBox,
            Alignment = Alignment.TopCenter,
            Size = new ScalableVector2(60, 60),
            Image = UserInterface.LoadingWheel,
            Tint = Color.White,
            Y = DividerLine.Y + 40
        };

        /// <summary>
        ///     Animates the loading wheel.
        /// </summary>
        private void HandleLoadingWheelAnimations()
        {
            if (LoadingWheel.Animations.Count != 0)
                return;

            var rotation = MathHelper.ToDegrees(LoadingWheel.Rotation);
            LoadingWheel.ClearAnimations();
            LoadingWheel.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.Linear, rotation, rotation + 360, 1000));
        }

        /// <summary>
        ///     Uploads the mapset to the server
        /// </summary>
        private void UploadMapset() => ThreadScheduler.Run(() =>
        {
            try
            {
                Logger.Important($"Starting to upload mapset...", LogType.Network);

                var path = MapManager.Selected.Value.Mapset.ExportToZip(false);
                Response = OnlineManager.Client.UploadMapset(path);
                Logger.Important($"Uploaded mapset with response: {Response}", LogType.Network);

                File.Delete(path);

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (Response.Code)
                {
                    case MapsetSubmissionStatusCode.SuccessUpdated:
                    case MapsetSubmissionStatusCode.SuccessUploaded:
                        // Get all files in the directory and delete them, so we can get the updated ones
                        foreach (var f in Directory.GetFiles($"{ConfigManager.SongDirectory.Value}/{MapManager.Selected.Value.Directory}", "*.qua"))
                            File.Delete(f);

                        using (var conn = new SQLiteConnection(MapDatabaseCache.DatabasePath))
                            MapManager.Selected.Value.Mapset.Maps.ForEach(x => conn.Delete(x));

                        foreach (var map in Response.Maps)
                        {
                            if (map == null)
                                continue;

                            var filePath = $"{ConfigManager.SongDirectory.Value}/{MapManager.Selected.Value.Directory}/{map.Id}.qua";

                            Logger.Important($"Commencing download for map: {map.Id}", LogType.Runtime);

                            try
                            {
                                OnlineManager.Client.DownloadMap(filePath, map.Id);
                                Logger.Important($"Successfully downloaded map: {map.Id}", LogType.Network);
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                            MapDatabaseCache.MapsToUpdate.Add(Map.FromQua(Qua.Parse(filePath), filePath));

                            Thread.Sleep(1000);
                        }

                        DividerLine.Tint = Color.LimeGreen;
                        TopLine.Tint = Color.LimeGreen;

                        MapDatabaseCache.ForceUpdateMaps();
                        break;
                    default:
                        DividerLine.Tint = Color.Crimson;
                        TopLine.Tint = Color.Crimson;
                        break;
                }

                Header.Text = StatusCodeMessages[Response.Code];
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Network);
                DividerLine.Tint = Color.Crimson;
                TopLine.Tint = Color.Crimson;

                Header.Text = "An unknown error has occurred while uploading. Please check your log files!";
            }
            finally
            {
                LoadingWheel.Visible = false;
                CreateCloseButton();

                if (Response != null && (Response.Code == MapsetSubmissionStatusCode.SuccessUpdated ||
                                          Response.Code == MapsetSubmissionStatusCode.SuccessUploaded))
                {
                    CloseButton.Border.Tint = Color.LimeGreen;
                    CloseButton.Text.Tint = Color.LimeGreen;

                    CreateVisitMapsetPageButton();
                    VisitMapsetPageButton.X = -VisitMapsetPageButton.Width / 2f - 10;
                    CloseButton.X = CloseButton.Width / 2f + 10;
                }
            }
        });

        /// <summary>
        /// </summary>
        private void CreateCloseButton()
        {
            CloseButton = new TextButton(UserInterface.BlankBox,
                Fonts.Exo2Medium, "Close", 14, (o, e) =>
                {
                    DialogManager.Dismiss(this);

                    var game = GameBase.Game as QuaverGame;
                    var screen = game?.CurrentScreen as EditorScreen;
                    screen.UploadInProgress = false;

                    if (Response.Code != MapsetSubmissionStatusCode.SuccessUpdated && Response.Code != MapsetSubmissionStatusCode.SuccessUploaded)
                        return;

                    game?.CurrentScreen.Exit(() => new EditorScreen(MapManager.Selected.Value.LoadQua(false)));
                })
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
                Y = LoadingWheel.Y + 10,
                UsePreviousSpriteBatchOptions = true,
                Size = new ScalableVector2(200, 40),
                Tint = Color.Transparent,
                Text =
                {
                    Tint = Color.Crimson,
                    Font = Fonts.Exo2SemiBold
                }
            };

            CloseButton.AddBorder(Color.Crimson, 2);
        }

        /// <summary>
        /// </summary>
        private void CreateVisitMapsetPageButton()
        {
            VisitMapsetPageButton = new TextButton(UserInterface.BlankBox,
                Fonts.Exo2Medium, "Visit Mapset Page", 14, (o, e) =>
                {
                    BrowserHelper.OpenURL($"{OnlineClient.WEBSITE_URL}/mapsets/{Response.MapsetId}");
                    DialogManager.Dismiss(this);

                    var game = GameBase.Game as QuaverGame;
                    var screen = game?.CurrentScreen as EditorScreen;
                    screen.UploadInProgress = false;

                    if (Response.Code != MapsetSubmissionStatusCode.SuccessUpdated && Response.Code != MapsetSubmissionStatusCode.SuccessUploaded)
                        return;

                    game?.CurrentScreen.Exit(() => new EditorScreen(MapManager.Selected.Value.LoadQua(false)));
                })
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
                Y = LoadingWheel.Y + 10,
                UsePreviousSpriteBatchOptions = true,
                Size = new ScalableVector2(200, 40),
                Tint = Color.Transparent,
                Text =
                {
                    Tint = Colors.MainAccent,
                    Font = Fonts.Exo2SemiBold
                }
            };

            VisitMapsetPageButton.AddBorder(Colors.MainAccent, 2);
        }
    }
}