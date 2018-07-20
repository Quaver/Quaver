using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Results.UI.ScoreResults
{
    internal class ScoreResultsTable : HeaderedContainer
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
                                    : base(new Vector2((GameBase.WindowRectangle.Width - 120) / 2f, 120),
                                            name, Fonts.AllerRegular16, 0.90f, Alignment.MidCenter, 50, Colors.DarkGray)
        {
            Screen = screen;
            ResultValueItems = items;

            ResultValueItems.ForEach(x =>
            {
                if (ResultValueItems.FindAll(y => y.Title == x.Title).ToList().Count > 1)
                    throw new ArgumentException("ResultValueItems must not contain duplicate items with the same `Title.`");
            });

            Content = CreateContent();
            Content.PosY = 50;

            // Create the table header coloring sprite.
            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = Content,
                Size = new UDim2D(SizeX, Content.SizeY / 2f),
                Tint = Color.White,
                Alpha = 0.08f
            };

            // Draw a table divider line across the width
            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = Content,
                Size = new UDim2D(SizeX, 1),
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
                var sizePer = SizeX / ResultValueItems.Count;
                var posX =  sizePer * i + sizePer / 2f;

                item.Initialize(Content, posX);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            ResultValueItems.ForEach(x => x.Update(dt));

            base.Update(dt);
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
                Size = new UDim2D(ContentSize.X, ContentSize.Y),
                Tint = Color.Black,
                Alpha = 0.45f
            };

            return content;
        }
    }
}