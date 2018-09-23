using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics.Backgrounds;
using Quaver.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Screens;

namespace Quaver.Screens.Select.UI.MapsetSelection
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

            Clicked += Select;
            BackgroundManager.Loaded += OnBackgroundLoaded;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BackgroundManager.Loaded -= OnBackgroundLoaded;
            base.Destroy();
        }

        /// <summary>
        ///     Makes the map displayed as if it is selected.
        /// </summary>
        public void DisplayAsSelected()
        {
            SkinManager.Skin.SoundHover.CreateChannel().Play();
            Alpha = 1;

            // Push set outwards to make it appear as selected.
            Transformations.Clear();
            Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutQuint, 120, 0, 800));

            // Pushes text forward to make room for the background.
            #region TEXT_ANIMATIONS

            Title.Transformations.Clear();
            Title.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear,
                Title.X, 125 + Title.MeasureString().X / 2f, 200));
            Title.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                Title.Alpha, 1, 100));

            Artist.Transformations.Clear();
            Artist.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear,
                Artist.X, 125 + Artist.MeasureString().X / 2f, 210));
            Artist.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                Artist.Alpha, 1, 100));

            Creator.Transformations.Clear();
            Creator.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear,
                Creator.X, 125 + Creator.MeasureString().X / 2f, 220));
            Creator.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                Creator.Alpha, 1, 100));

            #endregion
        }

        /// <summary>
        ///     Displays the map as if it were deselected.
        /// </summary>
        public void DisplayAsDeselected()
        {
            Alpha = 0.45f;

            // Push
            Transformations.Clear();
            Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutQuint, X, 120, 800));

            // Pushes text backwards to its original position
            #region TEXT_ANIMATIONS

            Title.Transformations.Clear();
            Title.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear,
                Title.X, 15 + Title.MeasureString().X / 2f, 300));
            Title.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                Title.Alpha, 0.85f, 300));

            Artist.Transformations.Clear();
            Artist.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear,
                Artist.X, 15 + Artist.MeasureString().X / 2f, 300));
            Artist.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                Artist.Alpha, 0.85f, 300));

            Creator.Transformations.Clear();
            Creator.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear,
                Creator.X, 15 + Creator.MeasureString().X / 2f, 300));
            Creator.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                Creator.Alpha, 0.85f, 300));
            #endregion

            // Thumbnail alpha change
            Thumbnail.Transformations.Clear();
            Thumbnail.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                Thumbnail.Alpha, 0, 250));
        }

        /// <summary>
        ///     Selects the mapset itself.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Select(object sender, EventArgs e)
        {
            if (Container.SelectedMapsetIndex != MapsetIndex)
            {
                // Set the preferred map in this current set.
                Container.Screen.AvailableMapsets[Container.SelectedMapsetIndex].PreferredMap = MapManager.Selected.Value;

                // If there is a preferred map, switch to it, otherwise choose the first map in the set.
                // TODO: Handle recommended difficulties here and give preferred more priority over recommended.
                var newMap = Mapset.PreferredMap ?? Mapset.Maps.First();

                Container.SelectMap(MapsetIndex, newMap);
            }
        }

        /// <summary>
        ///     When the map's background is loaded, we'll want to change the thunbnail.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e)
        {
            if (e.Map.Mapset.Directory != Mapset.Directory)
                return;

            Thumbnail.Image = e.Texture;

            Thumbnail.Transformations.Clear();
            Thumbnail.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                Thumbnail.Alpha, 1, 250));
        }
    }
}