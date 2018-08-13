using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Assets;
using Quaver.Audio;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Quaver.Graphics.Notifications;
using Quaver.Skinning;
using Wobble.Audio;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.Select.UI.Selector
{
    public class MapsetSelectorItem : Button
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
        private MapsetSelector Selector { get; }

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
        ///     If the set has a 4k map.
        /// </summary>
        private bool Has4KMaps { get; set; }

        /// <summary>
        ///     If the set has a 7K Map (used for )
        /// </summary
        private bool Has7KMaps { get; set; }

        /// <summary>
        ///     Keeps track of if we've already played the hover sound for this object.
        /// </summary>
        private bool HoverSoundPlayed { get; set; }

        /// <summary>
        ///     The height of the button itself.
        /// </summary>
        public static int BUTTON_HEIGHT { get; } = 80;

        /// <summary>
        ///     If the set is currently selected.
        /// </summary>
        public bool Selected { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="index"></param>
        public MapsetSelectorItem(MapsetSelector selector, int index)
        {
            Selector = selector;
            MapsetIndex = index;

            Alpha = 1;
            Size = new ScalableVector2(Selector.Width , BUTTON_HEIGHT);
            X = 120;
            Image = UserInterface.SelectBorder;

            Title = new SpriteText(Fonts.Exo2Regular24, "")
            {
                Parent = this,
                TextScale = 0.60f,
                Y = 17
            };

            Artist = new SpriteText(Fonts.Exo2Regular24, "")
            {
                Parent = this,
                TextScale = 0.45f,
                TextColor = Color.White,
                Alpha = 1f
            };

            Creator = new SpriteText(Fonts.Exo2Regular24, "")
            {
                Parent = this,
                TextScale = 0.40f,
                TextColor = Color.White,
                Alpha = 1f
            };

            Thumbnail = new Sprite()
            {
                Parent = this,
                X = 10,
                Size = new ScalableVector2(105, Height * 0.85f),
                Alignment = Alignment.MidLeft,
                Image = UserInterface.MenuBackground,
                Alpha = 0
            };

            // Here we want to change the associated mapset to the one that was passed in
            // so we can actually fully initialize the properties of each sprite.
            // In the constructor, we ONLY create them, but ChangeAssociatedMapset() further
            // initializes the mapset. This is to reduce code duplication when we pool the button
            // and change the associated mapset for it.
            ChangeAssociatedMapset(MapsetIndex);
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
            var oldVal = Selector.SelectedSet.Value;
            Selector.SelectedSet.Value = MapsetIndex;
            DisplayAsSelected();

            // Display the other map's button as deselected.
            Selector.MapsetButtons[oldVal].DisplayAsDeselected();

            // Change selected difficulty
            Selector.DifficultySelector.SelectDifficulty(Mapset, Mapset.Maps.First());
            Selector.StartLoadingMapAssets(MapAssetsToLoad.Audio | MapAssetsToLoad.Background);
        }

        /// <summary>
        ///     Selects the map.
        /// </summary>
        public void DisplayAsSelected()
        {
            SkinManager.Skin.SoundHover.CreateChannel().Play();
            HoverSoundPlayed = true;

            Selected = true;
            Alpha = 1;

            // Push set outwards to make it appear as selected.
            Transformations.Clear();
            Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear, 120, 0, 100));

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
        ///     Deselects the map
        /// </summary>
        public void DisplayAsDeselected()
        {
            Selected = false;
            Alpha = 0.45f;

            // Push
            Transformations.Clear();
            Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear, X, 120, 100));

            // Pushes text backwards to its original position
            #region TEXT_ANIMATIONS

            Title.Transformations.Clear();
            Title.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear,
                                            Title.X, 15 + Title.MeasureString().X / 2f, 200));
            Title.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                                            Title.Alpha, 0.85f, 100));

            Artist.Transformations.Clear();
            Artist.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear,
                                            Artist.X, 15 + Artist.MeasureString().X / 2f, 210));
            Artist.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                                            Artist.Alpha, 0.85f, 100));

            Creator.Transformations.Clear();
            Creator.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear,
                                            Creator.X, 15 + Creator.MeasureString().X / 2f, 220));
            Creator.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                                            Creator.Alpha, 0.85f, 100));
            #endregion

            // Thumbnail alpha change
            Thumbnail.Transformations.Clear();
            Thumbnail.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                                            Thumbnail.Alpha, 0, 250));
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
            Artist.Text = Mapset.Artist;

            // Change artist/creator text properties.
            var artistSize = Artist.MeasureString() / 2f;
            Artist.Y = artistSize.Y + Title.Y + 15f;
            Artist.X = 15 + artistSize.X;

            Creator.Text = "By: " + Mapset.Creator;

            // Change artist/creator text properties.
            var creatorSize = Creator.MeasureString() / 2f;
            Creator.Y = creatorSize.Y + Artist.Y + 15f;
            Creator.X = 15 + creatorSize.X;

            // Check available game modes.
            CheckModesForSet();

            // Add click handler.
            RemoveClickHandlers();
            Clicked += (sender, args) =>
            {
                // Don't bother selecting it again if it's already the selected one.
                if (Selector.SelectedSet.Value == MapsetIndex)
                    return;

                Select();
            };

            if (MapsetIndex == Selector.SelectedSet.Value)
                DisplayAsSelected();
        }

        /// <summary>
        ///     When a background is loaded, it'll change the thumbnail of the map.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e)
        {
            // Change if the map is in the mapset's directory.
            // Easy way to check if the map is in the set.
            if (e.Map.Directory != Mapset.Directory)
                return;

            Thumbnail.Image = e.Texture;

            Thumbnail.Transformations.Clear();
            Thumbnail.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.EaseOutQuad, 0, 1, 500));
        }
    }
}