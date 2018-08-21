using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics.Backgrounds;
using Quaver.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Window;

namespace Quaver.Screens.Select.UI.MapInfo.Banner
{
    public class MapBanner : Sprite
    {
        /// <summary>
        ///     Reference to the select screen.
        /// </summary>
        public SelectScreen Screen { get; }

        /// <summary>
        ///     Reference to the select screen's view.
        /// </summary>
        public SelectScreenView View { get; }

        /// <summary>
        ///     The mask for the map banner.
        /// </summary>
        public SpriteMaskContainer Mask { get; private set; }

        /// <summary>
        ///     The map background.
        /// </summary>
        private Sprite Background { get; set; }

        /// <summary>
        ///     Sprite that provides some brightness/darkness.
        /// </summary>
        public Sprite Brightness { get; private set; }

        /// <summary>
        ///     The text that displays the current artist.
        /// </summary>
        public SpriteText TextArtist { get; private set; }

        /// <summary>
        ///     The text that displays the current song's title.
        /// </summary>
        public SpriteText TextTitle { get; private set; }

        /// <summary>
        ///     Text that displays the current song's difficulty.
        /// </summary>
        public SpriteText TextDifficulty { get; private set; }

        /// <summary>
        ///     Text that displays the creator of the map.
        /// </summary>
        public SpriteText TextCreator { get; private set; }

        /// <summary>
        ///     Flag that displays the ranked status of the map.
        /// </summary>
        public RankedStatusFlag RankedStatusFlag { get; private set; }

        /// <summary>
        ///     The background shape that serves as a background behind the metadata.
        /// </summary>
        public Sprite MetadataContainerBackground { get; private set; }

        /// <summary>
        ///     Metadata container that says the bpm.
        /// </summary>
        public MetadataContainer Bpm { get; private set; }

        /// <summary>
        ///     Metadata container that has the song length.
        /// </summary>
        public MetadataContainer Length { get; private set; }

        /// <summary>
        ///     Metadata container that has the difficulty rating.
        /// </summary>
        public MetadataContainer Difficulty { get; private set; }

        /// <summary>
        ///     Metadata container for the map's game mode.
        /// </summary>
        public MetadataContainer Mode { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MapBanner(SelectScreen screen, SelectScreenView view)
        {
            Screen = screen;
            View = view;

            Alignment = Alignment.TopCenter;

            // All of this serves as a border for now.
            Size = new ScalableVector2(610, 150);
            Y = 10;
            Image = UserInterface.BlankBox;
            Tint = Color.Black;
            Alpha = 1f;

            CreateMask();
            CreateBrightnessSprite();

            CreateArtistText();
            CreateTitleText();
            CreateDifficultyText();
            CreateCreatorText();
            RealignSongInformationText();

            CreateRankedStatusFlag();
            CreateMetadataContainers();

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
        ///     Creates the mask and banner.
        /// </summary>
        private void CreateMask()
        {
            Mask = new SpriteMaskContainer
            {
                Parent = this,
                Size = new ScalableVector2(606, 146),
                Image = UserInterface.BlankBox,
                Alignment = Alignment.MidCenter,
                X = 0,
                Y = -1
            };

            Background = new Sprite
            {
                Size = new ScalableVector2(WindowManager.Width / 2f, WindowManager.Height / 2f),
                Alignment = Alignment.TopCenter,
                Y = -100
            };

            Mask.AddContainedSprite(Background);
        }

        /// <summary>
        ///     Creates the sprite for brightness on top of the banner.
        /// </summary>
        private void CreateBrightnessSprite() => Brightness = new Sprite
        {
            Parent = this,
            Size =  new ScalableVector2(Mask.Width + 2, Mask.Height),
            Alignment = Mask.Alignment,
            Position = Mask.Position,
            Tint = Color.Black,
        };

        /// <summary>
        ///     Creates the Artist Text.
        /// </summary>
        private void CreateArtistText() => TextArtist = new SpriteText(Fonts.Exo2Regular24, MapManager.Selected.Value.Artist,  0.60f)
        {
            Parent = Brightness,
            Alignment = Alignment.TopLeft,
            Alpha = 1f
        };

        /// <summary>
        ///     Creates the song title text.
        /// </summary>
        private void CreateTitleText() => TextTitle = new SpriteText(Fonts.Exo2Regular24, MapManager.Selected.Value.Title,  0.55f)
        {
            Parent = Brightness,
            Alignment = Alignment.TopLeft,
        };

        /// <summary>
        ///     Creates the Difficulty text
        /// </summary>
        private void CreateDifficultyText() => TextDifficulty = new SpriteText(Fonts.Exo2BoldItalic24,
            MapManager.Selected.Value.DifficultyName,  0.45f)
        {
            Parent = Brightness,
            Alignment = Alignment.TopLeft,
        };

        /// <summary>
        ///     Creates the Creator text.
        /// </summary>
        private void CreateCreatorText() => TextCreator = new SpriteText(Fonts.Exo2Regular24,
            $"- By: {MapManager.Selected.Value.Creator}",  0.40f)
        {
            Parent = Brightness,
            Alignment = Alignment.TopLeft,
        };

        /// <summary>
        ///     Realigns all of the text in the container.
        /// </summary>
        private void RealignSongInformationText()
        {
            const int leftSideSpacing = 15;

            var textArtistSize = TextArtist.MeasureString() / 2f;
            TextArtist.Position = new ScalableVector2(textArtistSize.X + leftSideSpacing, textArtistSize.Y + 15);

            var textTitleSize = TextTitle.MeasureString() / 2f;
            TextTitle.Position = new ScalableVector2(textTitleSize.X + leftSideSpacing, TextArtist.Y + textArtistSize.Y + 15);

            var textDifficultySize = TextDifficulty.MeasureString() / 2f;
            TextDifficulty.Position = new ScalableVector2(textDifficultySize.X + leftSideSpacing, TextTitle.Y + textTitleSize.Y + 15);

            var textCreatorSize = TextCreator.MeasureString() / 2f;
            TextCreator.Position = new ScalableVector2(textCreatorSize.X + leftSideSpacing, TextDifficulty.Y + textDifficultySize.Y + 15);
        }

        /// <summary>
        ///     Creates the flag that displays the ranked status.
        /// </summary>
        private void CreateRankedStatusFlag() => RankedStatusFlag = new RankedStatusFlag()
        {
            Parent = Brightness,
            Alignment = Alignment.BotRight,
        };

        /// <summary>
        ///     Creates the metadata containers to display more information about the map.
        /// </summary>
        private void CreateMetadataContainers()
        {
            MetadataContainerBackground = new Sprite()
            {
                Parent = Brightness,
                Alignment = Alignment.BotLeft,
                Image = UserInterface.MetadataContainer,
                Size = new ScalableVector2(510, 25),
                Tint = ColorHelper.HexToColor("#2a6fdb"),
                Y = 2
            };

            Mode = new MetadataContainer("MODE", ModeHelper.ToShortHand(MapManager.Selected.Value.Mode))
            {
                Parent = Brightness,
                Alignment = Alignment.BotLeft,
                Y = -3,
                X = 15
            };

            // Create container for BPM
            var bpm = (int) MapManager.Selected.Value.Bpm;
            Bpm = new MetadataContainer("BPM", bpm.ToString(CultureInfo.InvariantCulture))
            {
                Parent = Brightness,
                Alignment = Alignment.BotLeft,
                Y = Mode.Y,
                X = Mode.X + Mode.Width + 15
            };

            // Create container for song length
            Length = new MetadataContainer("LENGTH", TimeSpan.FromMilliseconds(MapManager.Selected.Value.SongLength).ToString(@"mm\:ss"))
            {
                Parent = Brightness,
                Alignment = Alignment.BotLeft,
                Y = Mode.Y,
                X = Bpm.X + Bpm.Width + 15
            };

            // Create container for difficulty
            Difficulty = new MetadataContainer("DIFFICULTY", $"{MapManager.Selected.Value.DifficultyRating:0.##}")
            {
                Parent = Brightness,
                Alignment = Alignment.BotLeft,
                X = Length.X + Length.Width + 15,
                Y = Mode.Y
            };
        }

        /// <summary>
        ///     Updates the selected map
        /// </summary>
        public void UpdateSelectedMap(bool fadeBackground)
        {
            if (fadeBackground)
            {
                // Dim the background.
                Brightness.Transformations.Clear();
                var brightnessTf = new Transformation(TransformationProperty.Alpha, Easing.Linear, Brightness.Alpha, 1, 200);
                Brightness.Transformations.Add(brightnessTf);
            }

            // Update song information text
            TextArtist.Text = MapManager.Selected.Value.Artist;
            TextTitle.Text = MapManager.Selected.Value.Title;
            TextDifficulty.Text = MapManager.Selected.Value.DifficultyName;
            TextCreator.Text = $"- By: {MapManager.Selected.Value.Creator}";
            RealignSongInformationText();

            // Update other metadata
            Difficulty.UpdateValue($"{MapManager.Selected.Value.DifficultyRating:0.##}");
            Length.UpdateValue(TimeSpan.FromMilliseconds(MapManager.Selected.Value.SongLength).ToString(@"mm\:ss"));

            var bpm = (int) MapManager.Selected.Value.Bpm;
            Bpm.UpdateValue(bpm.ToString());

            Mode.UpdateValue(ModeHelper.ToShortHand(MapManager.Selected.Value.Mode));
            AlignMetadataContainers();

            // Update the ranked status banner.
            RankedStatusFlag.ChangeColorAndText();
        }

        /// <summary>
        ///     When a new background is loaded, it'll change to it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e)
        {
            if (e.Map != MapManager.Selected.Value)
                return;

            Background.Image = e.Texture;

            // Undim the background.
            Brightness.Transformations.Clear();
            var brightnessTf = new Transformation(TransformationProperty.Alpha, Easing.Linear, Brightness.Alpha, 0.40f, 300);
            Brightness.Transformations.Add(brightnessTf);
        }

        /// <summary>
        ///     Aligns the metadata containers so that they're displayed correctly.
        /// </summary>
        private void AlignMetadataContainers()
        {
            Bpm.X = Mode.X + Mode.Width + 15;
            Length.X = Bpm.X + Bpm.Width + 15;
            Difficulty.X = Length.X + Length.Width + 15;
        }
    }
}