using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Common.Objects.Multiplayer;
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
            Rank.Text = $"{index + 1}.";

            if (SteamManager.UserAvatars != null && SteamManager.UserAvatars.ContainsKey(item.SteamId))
                Avatar.Image = SteamManager.UserAvatars[item.SteamId];
            else
                Avatar.Image = UserInterface.UnknownAvatar;

            Username.Text = $"{item.PlayerName ?? ""}";
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
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
        private void CreateRank() => Rank = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "0.", 22)
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
        private void CreateUsername() => Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
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
                new ResultsTableColumnData("Rating", $"{StringHelper.RatingToString(rating)}", ColorHelper.HexToColor("#F2C94C")),
                new ResultsTableColumnData("Grade", "", Color.White),
                new ResultsTableColumnData("Accuracy", $"{StringHelper.AccuracyToString(Processor.Accuracy)}",
                    Color.White),
                new ResultsTableColumnData("Max Combo", $"{Processor.MaxCombo:n0}x", Color.White),
                new ResultsTableColumnData("Mods", ModHelper.GetModsString(Processor.Mods), ColorHelper.HexToColor("#808080"))
            };

            foreach (Judgement j in Enum.GetValues(typeof(Judgement)))
            {
                if (j > Judgement.Miss)
                    break;

                data.Add(new ResultsTableColumnData(j.ToString(), $"{Processor.CurrentJudgements[j]:n0}",
                    ResultsJudgementGraphBar.GetColor(j)));
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

                if (item.ColumnText == "Grade")
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
                    val = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), item.Value, 22)
                    {
                        Parent = this,
                        TextAlignment = TextAlignment.Center,
                        Alignment = Alignment.MidRight,
                        UsePreviousSpriteBatchOptions = true,
                        Tint = item.Tint,
                    };

                    if (item.ColumnText == "Mods")
                    {
                        var text = (SpriteTextPlus) val;
                        text.MaxWidth = 140;
                    }
                }

                val.X = header.X - header.Width / 2f;
                val.X += val.Width / 2f;
            }
        }
    }
}