using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Resources;
using Quaver.Graphics;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Screens.Results.UI.ScoreResults
{
    public class ScoreResultsTable : HeaderedContainer
    {
        /// <summary>
        ///     Reference to the results screen itself
        /// </summary>
        private ResultsScreen Screen { get; }

        /// <summary>
        ///
        /// </summary>
        private List<ScoreResultsInfoItem> ResultValueItems { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="name"></param>
        public ScoreResultsTable(ResultsScreen screen, string name, List<ScoreResultsInfoItem> items)
                                    : base(new Vector2((WindowManager.Width - 120) / 2f, 120),
                                            name, BitmapFonts.Exo2Regular, 18, Alignment.MidCenter, 50, Colors.DarkGray)
        {
            Screen = screen;
            ResultValueItems = items;

            ResultValueItems.ForEach(x =>
            {
                if (ResultValueItems.FindAll(y => y.Title == x.Title).ToList().Count > 1)
                    throw new ArgumentException("ResultValueItems must not contain duplicate items with the same `Title.`");
            });

            Content = CreateContent();
            Content.Y = 50;

            // Create the table header coloring sprite.
            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = Content,
                Size = new ScalableVector2(Width, Content.Height / 2f),
                Tint = Color.White,
                Alpha = 0.08f
            };

            // Draw a table divider line across the width
            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = Content,
                Size = new ScalableVector2(Width, 1),
                Alignment = Alignment.MidLeft,
                Alpha = 0.50f
            };

            InitializeItems();
        }

        /// <summary>
        ///    Initialize all of the result value items.
        /// </summary>
        private void InitializeItems()
        {
            for (var i = 0; i < ResultValueItems.Count; i++)
            {
                var item = ResultValueItems[i];

                // Calculate x position
                var sizePer = Width / ResultValueItems.Count;
                var posX = sizePer * i + sizePer / 2f;

                item.Initialize(Content, posX);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ResultValueItems.ForEach(x => x.Update(gameTime.ElapsedGameTime.TotalMilliseconds));
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected sealed override Sprite CreateContent()
        {
            var content = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(ContentSize.X, ContentSize.Y),
                Tint = Color.Black,
                Alpha = 0.45f
            };

            return content;
        }
    }
}
