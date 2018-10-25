using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics.Backgrounds;
using Quaver.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Animations;
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

            Title = new SpriteText(BitmapFonts.Exo2Regular, "", 14)
            {
                Parent = this,
                Y = 17,
                Text = Mapset.Title
            };

            Title.X = 15 + Title.Width;

            Artist = new SpriteText(BitmapFonts.Exo2Regular, "", 14)
            {
                Parent = this,
                Tint = Color.White,
                Alpha = 1f,
                Text = Mapset.Artist,
                Y = Title.Y + 15f,
                X = 15
            };

            Creator = new SpriteText(BitmapFonts.Exo2Regular, "", 12)
            {
                Parent = this,
                Tint = Color.White,
                Alpha = 1f,
                Text = "By: " + Mapset.Creator,
                Y = Artist.Y + 15f,
                X = 15
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
            Animations.Clear();
            Animations.Add(new Animation(AnimationProperty.X, Easing.OutQuint, 120, 0, 800));

            // Pushes text forward to make room for the background.
            #region TEXT_ANIMATIONS

            Title.Animations.Clear();
            Title.Animations.Add(new Animation(AnimationProperty.X, Easing.Linear,
                Title.X, 125 + Title.Width, 200));
            Title.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear,
                Title.Alpha, 1, 100));

            Artist.Animations.Clear();
            Artist.Animations.Add(new Animation(AnimationProperty.X, Easing.Linear,
                Artist.X, 125 + Artist.Width, 210));
            Artist.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear,
                Artist.Alpha, 1, 100));

            Creator.Animations.Clear();
            Creator.Animations.Add(new Animation(AnimationProperty.X, Easing.Linear,
                Creator.X, 125 + Creator.Width, 220));
            Creator.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear,
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
            Animations.Clear();
            Animations.Add(new Animation(AnimationProperty.X, Easing.OutQuint, X, 120, 800));

            // Pushes text backwards to its original position
            #region TEXT_ANIMATIONS

            Title.Animations.Clear();
            Title.Animations.Add(new Animation(AnimationProperty.X, Easing.Linear,
                Title.X, 15 + Title.Width, 300));
            Title.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear,
                Title.Alpha, 0.85f, 300));

            Artist.Animations.Clear();
            Artist.Animations.Add(new Animation(AnimationProperty.X, Easing.Linear,
                Artist.X, 15 + Artist.Width, 300));
            Artist.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear,
                Artist.Alpha, 0.85f, 300));

            Creator.Animations.Clear();
            Creator.Animations.Add(new Animation(AnimationProperty.X, Easing.Linear,
                Creator.X, 15 + Creator.Width, 300));
            Creator.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear,
                Creator.Alpha, 0.85f, 300));
            #endregion

            // Thumbnail alpha change
            Thumbnail.Animations.Clear();
            Thumbnail.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear,
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

            Thumbnail.Animations.Clear();
            Thumbnail.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear,
                Thumbnail.Alpha, 1, 250));
        }
    }
}