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
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.Select.UI.Selector
{
    public class SongSelectorSet : Button
    {
        /// <summary>
        ///     The mapset this selector refers to.
        /// </summary>
        public Mapset Mapset => Selector.Screen.AvailableMapsets[MapsetIndex];

        /// <summary>
        ///     The index of Screen.AvailableMaps this refers to.
        /// </summary>
        public int MapsetIndex { get; private set; }

        /// <summary>
        ///     Reference to the parent SongSelector
        /// </summary>
        private SongSelector Selector { get; }

        /// <summary>
        ///     The background of the map.
        /// </summary>
        private Sprite Background { get; }
        private Sprite BackgroundDimmer { get; }

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
        public static int BUTTON_HEIGHT { get; } = 75;

        /// <summary>
        ///     If the set is currently selected.
        /// </summary>
        public bool Selected { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="index"></param>
        public SongSelectorSet(SongSelector selector, int index)
        {
            Selector = selector;
            MapsetIndex = index;

            Alpha = 0;
            Size = new ScalableVector2(Selector.Width , BUTTON_HEIGHT);
            X = 100;

            Background = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(Selector.Width, BUTTON_HEIGHT),
                Image = UserInterface.MenuBackground
            };

            BackgroundDimmer = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(Selector.Width, BUTTON_HEIGHT),
                Tint = Color.Black,
                Alpha = 0.65f
            };

            Title = new SpriteText(Fonts.Exo2Regular24, "")
            {
                Parent = this,
                TextScale = 0.60f,
                Y = 13
            };

            ArtistAndCreator = new SpriteText(Fonts.Exo2Regular24, "")
            {
                Parent = this,
                TextScale = 0.40f,
                TextColor = Color.LightGray,
                Alpha = 1f
            };

            Keys4MapsAvailable = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(18, 18),
                X = 15,
                Y = -8,
            };

            Keys7MapsAvailable = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(18, 18),
                X = Keys4MapsAvailable.X + Keys4MapsAvailable.Width + 10,
                Y = Keys4MapsAvailable.Y
            };

            // Here we want to change the associated mapset to the one that was passed in
            // so we can actually fully initialize the properties of each sprite.
            // In the constructor, we ONLY create them, but ChangeAssociatedMapset() further
            // initializes the mapset. This is to reduce code duplication when we pool the button
            // and change the associated mapset for it.
            ChangeAssociatedMapset(MapsetIndex);
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
            Selected = true;
            Selector.SelectedSet = MapsetIndex;
            DisplayAsSelected();

            // Display the other map's button as deselected.
            Selector.MapsetButtonPool.ForEach(x =>
            {
                if (x.Mapset != Mapset)
                    x.DisplayAsDeselected();
            });

            // Change map for now.
            MapManager.Selected.Value = Mapset.Maps.First();
        }

        /// <summary>
        ///     Selects the map.
        /// </summary>
        public void DisplayAsSelected()
        {
            Selected = true;
            BackgroundDimmer.Alpha = 0.45f;

            Transformations.Clear();
            Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutBounce, 100, 0, 600));
        }

        /// <summary>
        ///     Deselects the map
        /// </summary>
        public void DisplayAsDeselected()
        {
            Selected = false;
            BackgroundDimmer.Alpha = 0.65f;

            Transformations.Clear();
            Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutBounce, X, 100, 600));
        }

        /// <summary>
        ///     Changes the associated mapset with this button.
        /// </summary>
        /// <param name="index"></param>
        public void ChangeAssociatedMapset(int index)
        {
            MapsetIndex = index;

            // Change title text.
            Title.Text = Mapset.Title;
            Title.X = 15 + Title.MeasureString().X / 2f;

            // Change artist text.
            ArtistAndCreator.Text = $"{Mapset.Artist} // by: {Mapset.Creator}";

            // Change artist/creator text properties.
            var artistAndCreatorSize = ArtistAndCreator.MeasureString() / 2f;
            ArtistAndCreator.Y = artistAndCreatorSize.Y + Title.Y + 15f;
            ArtistAndCreator.X = 15 + artistAndCreatorSize.X;

            // Check available game modes.
            CheckModesForSet();

            // Change available map properties.
            Keys7MapsAvailable.Alpha = Has7KMaps ? 1 : 0.10f;
            Keys4MapsAvailable.Alpha = Has4KMaps ? 1 : 0.10f;

            // Add click handler.
            RemoveClickHandlers();
            Clicked += (sender, args) => Select();

            if (MapsetIndex == Selector.SelectedSet)
                DisplayAsSelected();
        }
    }
}