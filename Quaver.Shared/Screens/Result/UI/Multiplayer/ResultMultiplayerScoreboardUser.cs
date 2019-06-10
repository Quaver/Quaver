using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Gameplay.UI.Scoreboard;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Result.UI.Multiplayer
{
    public class ResultMultiplayerScoreboardUser : PoolableSprite<ScoreboardUser>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override int HEIGHT { get; } = 44;

        /// <summary>
        /// </summary>
        private ResultMultiplayerScoreboardTableHeader Header { get; }

        /// <summary>
        /// </summary>
        private ResultMultiplayerScoreboardUserButton Button { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="header"></param>
        public ResultMultiplayerScoreboardUser(PoolableScrollContainer<ScoreboardUser> container, ScoreboardUser item, int index, ResultMultiplayerScoreboardTableHeader header)
            : base(container, item, index)
        {
            Header = header;
            Size = new ScalableVector2(container.Width, HEIGHT);
            Alpha = 0.85f;

            Button = new ResultMultiplayerScoreboardUserButton(Item, container)
            {
                Parent = this,
                Size = Size,
                UsePreviousSpriteBatchOptions = true
            };

            // ReSharper disable once ObjectCreationAsStatement
            var rank = new SpriteTextBitmap(FontsBitmap.GothamRegular, $"{item.Rank}.")
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 20,
                FontSize = 16,
                Tint = Item.Type == ScoreboardUserType.Self ? Colors.SecondaryAccent : Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            var avatar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(32, 32),
                X = 56,
                Image = item.Avatar.Image,
                UsePreviousSpriteBatchOptions = true
            };

            avatar.AddBorder(Color.White, 2);

            var username = new SpriteTextBitmap(FontsBitmap.GothamRegular, item.UsernameRaw)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = avatar.X + avatar.Width + 16,
                FontSize = 16,
                Tint = Item.Type == ScoreboardUserType.Self ? Colors.SecondaryAccent : Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            if (item.Processor == null)
                return;

            CreateData(new Dictionary<string, string>
            {
                {"Rating", item.CalculateRating().ToString("00.00")},
                {"Grade", ""},
                {"Accuracy", StringHelper.AccuracyToString(item.Processor.Accuracy)},
                {"Max Combo", item.Combo.Text},
                {"Marv", item.Processor.CurrentJudgements[Judgement.Marv].ToString()},
                {"Perf", item.Processor.CurrentJudgements[Judgement.Perf].ToString()},
                {"Great", item.Processor.CurrentJudgements[Judgement.Great].ToString()},
                {"Good", item.Processor.CurrentJudgements[Judgement.Good].ToString()},
                {"Okay", item.Processor.CurrentJudgements[Judgement.Okay].ToString()},
                {"Miss", item.Processor.CurrentJudgements[Judgement.Miss].ToString()},
                {"Mods", item.Processor.Mods <= 0 ? "None" : ModHelper.GetModsString(ModHelper.GetModsFromRate(ModHelper.GetRateFromMods(item.Processor.Mods)))}
            });

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Width, 1),
                Alpha = 0.3f
            };
        }

        public override void UpdateContent(ScoreboardUser item, int index)
        {
        }

        public override void Update(GameTime gameTime)
        {
            Visible = RectangleF.Intersects(ScreenRectangle, Container.ScreenRectangle);

            if (!Visible)
                return;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            base.Draw(gameTime);
        }

        public override void Destroy()
        {
            Button.Destroy();
            ButtonManager.Remove(Button);
            base.Destroy();
        }

        /// <summary>
        ///     Adds data to the row
        ///     <header name, value>
        /// </summary>
        /// <param name="data"></param>
        private void CreateData(Dictionary<string, string> data)
        {
            foreach (var pair in data)
            {
                var header = Header.Headers[pair.Key];

                Sprite val;

                if (pair.Key == "Grade")
                {
                    val = new Sprite()
                    {
                        Parent = this,
                        Alignment = Alignment.MidRight,
                        Size = new ScalableVector2(30, 30),
                        Image = Item.Processor.Failed ? SkinManager.Skin.Grades[Grade.F] : SkinManager.Skin.Grades[GradeHelper.GetGradeFromAccuracy(Item.Processor.Accuracy)],
                        UsePreviousSpriteBatchOptions = true
                    };
                }
                else
                {
                    val = new SpriteTextBitmap(FontsBitmap.GothamRegular, pair.Value)
                    {
                        Parent = this,
                        Alignment = Alignment.MidRight,
                        FontSize = 16,
                        UsePreviousSpriteBatchOptions = true
                    };
                }

                if (Item.Type == ScoreboardUserType.Self && val is SpriteTextBitmap t)
                    t.Tint = Colors.SecondaryAccent;

                val.X = header.X - header.Width / 2f;
                val.X += val.Width / 2f;
            }
        }
    }
}