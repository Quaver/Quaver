using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Multiplayer.Table.Scrolling
{
    public class ResultsMultiplayerPlayer : PoolableSprite<ScoreProcessor>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override int HEIGHT { get; } = 69;

        /// <summary>
        /// </summary>
        private MultiplayerGame Game { get; }

        /// <summary>
        /// </summary>
        private Dictionary<string, SpriteTextPlus> Headers { get; }

        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private Sprite BottomLine { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Rank { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Avatar { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Username { get; set; }

        /// <summary>
        /// </summary>
        private ScoreProcessor Processor => Item.StandardizedProcessor ?? Item;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="game"></param>
        /// <param name="headers"></param>
        /// <param name="map"></param>
        public ResultsMultiplayerPlayer(PoolableScrollContainer<ScoreProcessor> container, ScoreProcessor item, int index,
            MultiplayerGame game, Dictionary<string, SpriteTextPlus> headers, Map map) : base(container, item, index)
        {
            Map = map;
            Game = game;
            Headers = headers;
            Size = new ScalableVector2(container.Width, HEIGHT);
            Alpha = 0;

            CreateButton();
            CreateBottomLine();
            CreateRank();
            CreateAvatar();
            CreateUsername();

            SteamManager.SteamUserAvatarLoaded += OnSteamUserAvatarLoaded;

            CreateData();
        }

        public override void Update(GameTime gameTime)
        {
            if (ConfigManager.Username != null && ConfigManager.Username.Value == Item.PlayerName)
            {
                Button.Tint = Colors.SecondaryAccent;
                Button.Alpha = 1;
            }
            else if (Button.IsHovered)
            {
                Button.Tint = Color.White;
                Button.Alpha = 1;
            }
            else
            {
                Button.Tint = Color.White;
                Button.Alpha = 0;
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(ScoreProcessor item, int index)
        {
            Item = item;
            Index = index;

            ScheduleUpdate(() =>
            {
                Rank.Text = $"{index + 1}.";

                UpdateAvatar(item);

                Username.Text = $"{item.PlayerName ?? ""}";
                Username.Tint = GetUsernameColor();
            });
        }

        /// <summary>
        ///     Loads a cached avatar or requests it from Steam when it is not available yet.
        /// </summary>
        private void UpdateAvatar(ScoreProcessor item)
        {
            if (item.SteamId == 0)
            {
                Avatar.Image = UserInterface.UnknownAvatar;
                Avatar.Alpha = 1;
                return;
            }

            var avatar = TryGetAvatar(item.SteamId);

            if (avatar != null)
            {
                Avatar.Image = avatar;
                Avatar.Alpha = 1;
                return;
            }

            Avatar.Alpha = 0;
            SteamManager.SendAvatarRetrievalRequest(item.SteamId);
        }

        /// <summary>
        ///     Retrieves an avatar using the same cache preference as the V2 Online Hub.
        /// </summary>
        private Texture2D TryGetAvatar(ulong steamId)
        {
            if (SteamManager.UserAvatarsLarge.TryGetValue(steamId, out var texture) && texture != UserInterface.UnknownAvatar)
                return texture;

            if (SteamManager.UserAvatars.TryGetValue(steamId, out texture))
                return texture;

            return null;
        }

        /// <summary>
        ///     Applies an asynchronously loaded avatar if this pooled row still represents that Steam user.
        /// </summary>
        private void OnSteamUserAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
        {
            AddScheduledUpdate(() =>
            {
                if (IsDisposed || Item == null || Item.SteamId != e.SteamId)
                    return;

                var avatar = TryGetAvatar(e.SteamId);
                if (avatar == null)
                    return;

                Avatar.Image = avatar;
                Avatar.Alpha = 1;
            });
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                X = -6,
                Image = UserInterface.OptionsSidebarButtonBackground,
                Size = Size,
                Alpha = 0
            };
        }

        /// <summary>
        /// </summary>
        private void CreateBottomLine() => BottomLine = new Sprite
        {
            Parent = this,
            UsePreviousSpriteBatchOptions = true,
            Alignment = Alignment.BotLeft,
            Size = new ScalableVector2(Width, 2),
            Tint = ColorHelper.HexToColor("#525252")
        };

        /// <summary>
        /// </summary>
        private void CreateRank() => Rank = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), "0.", 20)
        {
            Parent = this,
            UsePreviousSpriteBatchOptions = true,
            Alignment = Alignment.MidLeft,
            X = 22
        };

        /// <summary>
        /// </summary>
        private void CreateAvatar() => Avatar = new Sprite
        {
            Parent = this,
            UsePreviousSpriteBatchOptions = true,
            Alignment = Alignment.MidLeft,
            Image = UserInterface.UnknownAvatar,
            Size = new ScalableVector2(46, 46),
            X = Rank.X + Rank.Width + 30,
        };

        /// <summary>
        /// </summary>
        private void CreateUsername() => Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), "", 20)
        {
            Parent = this,
            UsePreviousSpriteBatchOptions = true,
            Alignment = Alignment.MidLeft,
            X = Avatar.X + Avatar.Width + 14
        };

        /// <summary>
        /// </summary>
        private void CreateData()
        {
            var rating = new RatingProcessorKeys(Map.DifficultyFromMods(Processor.Mods)).CalculateRating(Processor);

            var data = new List<ResultsTableColumnData>()
            {
                new ResultsTableColumnData(ResultsLocalization.Get("Rating"), $"{StringHelper.RatingToString(rating)}", ColorHelper.HexToColor("#F2C94C")),
                new ResultsTableColumnData(ResultsLocalization.Get("Grade"), "", Color.White),
                new ResultsTableColumnData(ResultsLocalization.Get("Accuracy"), $"{StringHelper.AccuracyToString(Processor.Accuracy)}",
                    GetUsernameColor()),
                new ResultsTableColumnData(ResultsLocalization.Get("Max Combo"), $"{Processor.MaxCombo:n0}x", GetUsernameColor()),
                new ResultsTableColumnData(ResultsLocalization.Get("Mods"), ModHelper.GetModsString(Processor.Mods), ColorHelper.HexToColor("#808080"))
            };

            foreach (Judgement j in Enum.GetValues(typeof(Judgement)))
            {
                if (j > Judgement.Miss)
                    break;

                data.Add(new ResultsTableColumnData(ResultsLocalization.Get(j.ToString()), $"{Processor.CurrentJudgements[j]:n0}",
                    ResultsJudgementGraphJudgementBar.GetColor(j)));
            }

            CreateData(data);
        }

        /// <summary>
        ///     Adds data to the row
        /// </summary>
        /// <param name="data"></param>
        private void CreateData(List<ResultsTableColumnData> data)
        {
            foreach (var item in data)
            {
                var header = Headers[item.ColumnText];

                Sprite val;

                if (item.ColumnText == ResultsLocalization.Get("Grade"))
                {
                    var grade = Processor.Failed ? Grade.F : GradeHelper.GetGradeFromAccuracy(Processor.Accuracy);
                    var tex = TextureManager.Load($@"Quaver.Resources/Textures/Skins/Shared/Grades/grade-small-{grade.ToString().ToLower()}.png");

                    const int gradeWidth = 30;

                    val = new Sprite()
                    {
                        Parent = this,
                        Alignment = Alignment.MidRight,
                        Size = new ScalableVector2(gradeWidth, tex.Height / tex.Width * gradeWidth),
                        UsePreviousSpriteBatchOptions = true,
                        Image = tex
                    };
                }
                else
                {
                    val = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), item.Value, 20)
                    {
                        Parent = this,
                        TextAlignment = TextAlignment.Center,
                        Alignment = Alignment.MidRight,
                        UsePreviousSpriteBatchOptions = true,
                        Tint = item.Tint,
                    };

                    if (item.ColumnText == ResultsLocalization.Get("Mods"))
                    {
                        var text = (SpriteTextPlus) val;
                        text.MaxWidth = 140;
                    }
                }

                val.X = header.X - header.Width / 2f;
                val.X += val.Width / 2f;
            }
        }

        private Color GetUsernameColor()
        {
            if (Game.Ruleset != MultiplayerGameRuleset.Team)
                return Color.White;

            var team = ResultsMultiplayerTable.GetTeamFromScoreProcessor(Game, Item);

            switch (team)
            {
                case MultiplayerTeam.Red:
                    return ColorHelper.HexToColor("#F9645D");
                case MultiplayerTeam.Blue:
                    return ColorHelper.HexToColor("#0587E5");
            }

            return Color.White;
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            SteamManager.SteamUserAvatarLoaded -= OnSteamUserAvatarLoaded;
            base.Destroy();
        }
    }
}
