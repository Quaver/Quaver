using System;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Quaver.API.Enums;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Notifications;
using Quaver.Skinning;
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
        public Mapset Mapset { get; private set; }

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
        public SpriteText Title { get; }

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

        /// <summary>
        ///     Keeps track of if we've already played the hover sound for this object.
        /// </summary>
        private bool HoverSoundPlayed { get; set; }

        /// <summary>
        ///     The height of the button itself.
        /// </summary>
        public static int BUTTON_HEIGHT { get; } = 50;

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
            Size = new ScalableVector2(Selector.Width - selector.Scrollbar.Width, BUTTON_HEIGHT);

            Thumbnail = new Sprite()
            {
                Parent = this,
                Image = UserInterface.MenuCompetitive,
                Size = new ScalableVector2(Height, Height),
                Alignment = Alignment.TopLeft,
            };

            RankedStatusFlag = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(5, Height),
                X = Thumbnail.X + Thumbnail.Width
            };

            Highlight = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(0, Height),
                Tint = Color.White,
            };

            Title = new SpriteText(Fonts.Exo2Regular24, "")
            {
                Parent = this,
                TextScale = 0.50f,
                Y = 15
            };

            ArtistAndCreator = new SpriteText(Fonts.Exo2Regular24, "")
            {
                Parent = this,
                TextScale = 0.35f,
                TextColor = Color.LightGray,
                Alpha = 0.90f
            };

            Keys7MapsAvailable = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                X = -20,
            };

            Keys4MapsAvailable = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                X = Keys7MapsAvailable.X - Keys7MapsAvailable.Width - 15,
            };

            // Here we want to change the associated mapset to the one that was passed in
            // so we can actually fully initialize the properties of each sprite.
            // In the constructor, we ONLY create them, but ChangeAssociatedMapset() further
            // initializes the mapset. This is to reduce code duplication when we pool the button
            // and change the associated mapset for it.
            ChangeAssociatedMapset(Mapset);
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (IsHovered)
            {
                if (!HoverSoundPlayed)
                {
                    SkinManager.Skin.SoundHover.CreateChannel().Play();
                    HoverSoundPlayed = true;
                }
            }
            else
            {
                HoverSoundPlayed = false;
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     Checks which modes the maps in the set have.
        /// </summary>
        private void CheckModesForSet()
        {
            // Reset them each time we check because we call this method over
            // to re-check.
            Has4KMaps = false;
            Has7KMaps = false;

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
            Highlight.Transformations.Add(new Transformation(Easing.Linear, Highlight.Tint, Colors.MainAccent, 300));

            // Add map thumbnail transformation
            Thumbnail.Transformations.Clear();
            Thumbnail.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 0.35f, 1, 150));

            // Change map for now.
            MapManager.Selected.Value = Mapset.Maps.First();
        }

        public void Deselect()
        {
            RankedStatusFlag.Transformations.Clear();

            var tf = new Transformation(TransformationProperty.Width, Easing.EaseInQuad, Width, 3, 300);
            RankedStatusFlag.Transformations.Add(tf);
        }

        /// <summary>
        ///     Changes the associated mapset with this button.
        /// </summary>
        /// <param name="set"></param>
        public void ChangeAssociatedMapset(Mapset set)
        {
            Mapset = set;

            // Set thumbnail properties.
            Thumbnail.Transformations.Clear();
            Thumbnail.Image = UserInterface.MenuCompetitive;
            Thumbnail.Alpha = 0.35f;

            // Set RankedStatusFlag properties
            RankedStatusFlag.Tint = Colors.SecondaryAccent;

            // Change highlight properties.
            Highlight.Transformations.Clear();
            Highlight.Alpha = 0.15f;
            Highlight.Width = Width - RankedStatusFlag.Width - Thumbnail.Width;
            Highlight.X = RankedStatusFlag.X + RankedStatusFlag.Width;

            // Change title text.
            Title.Text = Mapset.Title;
            Title.X = RankedStatusFlag.X + RankedStatusFlag.Width + 8 + Title.MeasureString().X / 2f;

            // Change artist text.
            ArtistAndCreator.Text = $"{Mapset.Artist} // by: {Mapset.Creator}";

            // Change artist/creator text properties.
            var artistAndCreatorSize = ArtistAndCreator.MeasureString() / 2f;
            ArtistAndCreator.Y = artistAndCreatorSize.Y + Title.Y + 12f;
            ArtistAndCreator.X = RankedStatusFlag.X + RankedStatusFlag.Width + 8 + artistAndCreatorSize.X;

            // Check available game modes.
            CheckModesForSet();

            // Change available map properties.
            Keys7MapsAvailable.Alpha = Has7KMaps ? 0.65f : 0.25f;
            Keys4MapsAvailable.Alpha = Has4KMaps ? 0.65f : 0.25f;

            // Add click handler.
            RemoveClickHandlers();
            Clicked += (sender, args) => Select();
        }
    }
}