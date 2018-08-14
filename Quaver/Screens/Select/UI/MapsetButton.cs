using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Database.Maps;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.Select.UI
{
    public class MapsetButton : Button
    {
        /// <summary>
        ///     Reference to the mapset this button is for.
        /// </summary>
        public Mapset Mapset { get; }

        /// <summary>
        ///     The index of the mapset in our list of available mapsets.
        /// </summary>
        public int MapsetIndex { get; }

        /// <summary>
        ///     Reference to the parent mapset container
        /// </summary>
        public MapsetContainer Container { get; }

        /// <summary>
        ///     The text that displays the artist of the map.
        /// </summary>
        public SpriteText Title { get; }

        /// <summary>
        ///     The text that displays the artist
        /// </summary>
        private SpriteText Artist { get; }

        /// <summary>
        ///     The text that displays the creator of the map.
        /// </summary>
        private SpriteText Creator { get; }

        /// <summary>
        ///     The thumbnail for the
        /// </summary>
        public Sprite Thumbnail { get; }

        /// <summary>
        ///    The height of each mapset button
        /// </summary>
        public static int BUTTON_HEIGHT => 80;

        /// <summary>
        ///     The amount of y spacing between each button.
        /// </summary>
        public static int BUTTON_Y_SPACING => 10;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="set"></param>
        /// <param name="mapsetIndex"></param>
        public MapsetButton(MapsetContainer container, Mapset set, int mapsetIndex)
        {
            Mapset = set;
            Container = container;
            MapsetIndex = mapsetIndex;

            DestroyIfParentIsNull = false;
            Size = new ScalableVector2(Container.Width, BUTTON_HEIGHT);
            Image = UserInterface.SelectBorder;

            Title = new SpriteText(Fonts.Exo2Regular24, "")
            {
                Parent = this,
                TextScale = 0.60f,
                Y = 17,
                Text = Mapset.Title
            };

            Title.X = 15 + Title.MeasureString().X / 2f;

            Artist = new SpriteText(Fonts.Exo2Regular24, "")
            {
                Parent = this,
                TextScale = 0.45f,
                TextColor = Color.White,
                Alpha = 1f,
                Text = Mapset.Artist
            };

            // Change artist/creator text properties.
            var artistSize = Artist.MeasureString() / 2f;
            Artist.Y = artistSize.Y + Title.Y + 15f;
            Artist.X = 15 + artistSize.X;

            Creator = new SpriteText(Fonts.Exo2Regular24, "")
            {
                Parent = this,
                TextScale = 0.40f,
                TextColor = Color.White,
                Alpha = 1f,
                Text = "By: " + Mapset.Creator
            };

            // Change artist/creator text properties.
            var creatorSize = Creator.MeasureString() / 2f;
            Creator.Y = creatorSize.Y + Artist.Y + 15f;
            Creator.X = 15 + creatorSize.X;

            Thumbnail = new Sprite()
            {
                Parent = this,
                X = 10,
                Size = new ScalableVector2(105, Height * 0.85f),
                Alignment = Alignment.MidLeft,
                Image = UserInterface.MenuBackground,
                Alpha = 0
            };

            Clicked += (sender, args) =>
            {
                MapManager.Selected.Value = Mapset.Maps.First();
                Console.WriteLine("Clicked");
            };
        }
    }
}