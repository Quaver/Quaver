using System;
using System.Drawing;
using Microsoft.Xna.Framework.Media;
using Quaver.API.Enums;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Notifications;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.Select.UI.Selector
{
    public class SongSelectorSet : Button
    {
        /// <summary>
        ///     The thumbnail of the map.
        /// </summary>
        public Sprite Thumbnail { get; }

        /// <summary>
        ///     The mapset this selector refers to.
        /// </summary>
        public Mapset Mapset { get; }

        /// <summary>
        ///     Reference to the parent SongSelector
        /// </summary>
        private SongSelector Selector { get; }

        /// <summary>
        ///     A flag that symbolizes the ranked status.
        /// </summary>
        private Sprite RankedStatusFlag { get; }

        /// <summary>
        ///     When the map is selected, this is the sprite that makes it appear as highlighted.
        /// </summary>
        private Sprite Highlight { get; }

        /// <summary>
        ///     The text that displays the artist of the map.
        /// </summary>
        private SpriteText Title { get; }

        /// <summary>
        ///     The text that displays the artist and creator.
        /// </summary>
        private SpriteText ArtistAndCreator { get; }

        /// <summary>
        ///     If the set has a 4k map.
        /// </summary>
        private bool Has4KMaps { get; set; }

        /// <summary>
        ///     If the set has a 7K Map (used for )
        /// </summary
        private bool Has7KMaps { get; set; }

        /// <summary>
        ///     Sprite that dictates if there are 4k maps in the set.
        /// </summary>
        private Sprite Keys4MapsAvailable { get; set; }

        /// <summary>
        ///     Sprite that dictates if there are 7k maps in the set.
        /// </summary>
        private Sprite Keys7MapsAvailable { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="set"></param>
        public SongSelectorSet(SongSelector selector, Mapset set)
        {
            Selector = selector;
            Mapset = set;

            Alpha = 0;
            Size = new ScalableVector2(Selector.Width - selector.Scrollbar.Width, 50);

            Thumbnail = new Sprite()
            {
                Parent = this,
                Image = UserInterface.MenuCompetitive,
                Size = new ScalableVector2(Height, Height),
                Alignment = Alignment.TopLeft,
                Alpha = 0.35f
            };

            RankedStatusFlag = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(5, Height),
                Tint = Colors.SecondaryAccent,
                X = Thumbnail.X + Thumbnail.Width
            };

            Highlight = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(0, Height),
                Tint = Color.White,
                Alpha = 0.15f,
                X = RankedStatusFlag.X + RankedStatusFlag.Width - 0.5f
            };

            Title = new SpriteText(Fonts.Exo2Regular24, Mapset.Title)
            {
                Parent = this,
                X = RankedStatusFlag.X + RankedStatusFlag.Width + 8,
                TextScale = 0.50f,
                Y = 15
            };

            Title.X += Title.MeasureString().X / 2f;

            ArtistAndCreator = new SpriteText(Fonts.Exo2Regular24, $"{Mapset.Artist} | by: {Mapset.Creator}")
            {
                Parent = this,
                X = RankedStatusFlag.X + RankedStatusFlag.Width + 8,
                TextScale = 0.35f,
                TextColor = Color.LightGray,
                Alpha = 0.90f
            };

            var artistAndCreatorSize = ArtistAndCreator.MeasureString() / 2f;
            ArtistAndCreator.Y = artistAndCreatorSize.Y + Title.Y + 12f;
            ArtistAndCreator.X += artistAndCreatorSize.X;

            CheckModesForSet();

            Keys7MapsAvailable = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                X = -20,
                Alpha = Has7KMaps ? 0.65f : 0.25f
            };

            Keys4MapsAvailable = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                X = Keys7MapsAvailable.X - Keys7MapsAvailable.Width - 15,
                Alpha = Has4KMaps ? 0.65f : 0.25f
            };

            Clicked += (sender, args) => Select();
        }

        /// <summary>
        ///     Checks which modes the maps in the set have.
        /// </summary>
        private void CheckModesForSet()
        {
            foreach (var map in Mapset.Maps)
            {
                switch (map.Mode)
                {
                    case GameMode.Keys4:
                        Has4KMaps = true;
                        break;
                    case GameMode.Keys7:
                        Has7KMaps = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (Has4KMaps && Has7KMaps)
                    return;
            }
        }

        /// <summary>
        ///     Makes this the currently selected mapset.
        /// </summary>
        public void Select()
        {
            // Add highlight transformation
            Highlight.Transformations.Clear();
            Highlight.Transformations.Add(new Transformation(TransformationProperty.Width, Easing.EaseInQuad, 0,
                                                Width - RankedStatusFlag.Width - Thumbnail.Width, 150));

            // Add map thumbnail transformation
           Thumbnail.Transformations.Clear();
           Thumbnail.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 0.35f, 1, 150));
        }

        public void Deselect()
        {
            RankedStatusFlag.Transformations.Clear();

            var tf = new Transformation(TransformationProperty.Width, Easing.EaseInQuad, Width, 3, 300);
            RankedStatusFlag.Transformations.Add(tf);
        }
    }
}