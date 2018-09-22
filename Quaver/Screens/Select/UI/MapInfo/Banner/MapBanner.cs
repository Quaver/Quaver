using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Quaver.Helpers;
using Quaver.Modifiers;
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
        ///     Text that displays the current song's difficulty name.
        /// </summary>
        public SpriteText TextDifficultyName { get; private set; }

        /// <summary>
        ///     Text that displays the current song difficulty title
        /// </summary>
        public SpriteText TextDifficultyRating { get; private set; }

        /// <summary>
        ///     Text that displays the creator of the map.
        /// </summary>
        public SpriteText TextCreator { get; private set; }

        /// <summary>
        ///     Text that displays the activated mods.
        /// </summary>
        public SpriteText TextMods { get; private set; }

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
            Size = new ScalableVector2(778, 175);
            Y = 0;
            Image = UserInterface.BlankBox;
            Tint = Colors.MainAccent;
            Alpha = 0f;

            CreateMask();
            CreateBrightnessSprite();

            CreateArtistText();
            CreateTitleText();
            CreateDifficultyNameText();
            CreateDifficultyRatingText();
            CreateCreatorText();
            CreateModsText();
            RealignSongInformationText();

            CreateRankedStatusFlag();
            CreateMetadataContainers();

            BackgroundManager.Loaded += OnBackgroundLoaded;
            ModManager.ModsChanged += OnModsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BackgroundManager.Loaded -= OnBackgroundLoaded;
            ModManager.ModsChanged -= OnModsChanged;
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
                Size = new ScalableVector2(772, 171),
                Image = UserInterface.BlankBox,
                Alignment = Alignment.MidCenter,
                X = 0,
                Y = -1
            };

            Background = new Sprite
            {
                Size = new ScalableVector2(WindowManager.Width / 1.6f, WindowManager.Height / 1.6f),
                Alignment = Alignment.TopCenter,
                Y = -50
            };

            Mask.AddContainedSprite(Background);
        }

        /// <summary>
        ///     Creates the sprite for brightness on top of the banner.
        /// </summary>
        private void CreateBrightnessSprite() => Brightness = new Sprite
        {
            Parent = this,
            Size =  new ScalableVector2(Mask.Width + 2, Mask.Height + 1),
            Alignment = Mask.Alignment,
            Position = Mask.Position,
            Tint = Color.Black,
        };

        /// <summary>
        ///     Creates the Artist Text.
        /// </summary>
        private void CreateArtistText() => TextArtist = new SpriteText(Fonts.Exo2Regular24, MapManager.Selected.Value.Artist,  0.65f)
        {
            Parent = Brightness,
            Alignment = Alignment.TopLeft,
            Alpha = 1f
        };

        /// <summary>
        ///     Creates the song title text.
        /// </summary>
        private void CreateTitleText() => TextTitle = new SpriteText(Fonts.Exo2Regular24, MapManager.Selected.Value.Title,  0.60f)
        {
            Parent = Brightness,
            Alignment = Alignment.TopLeft,
        };

        /// <summary>
        ///     Creates the Difficulty text
        /// </summary>
        private void CreateDifficultyNameText() => TextDifficultyName = new SpriteText(Fonts.Exo2BoldItalic24,
            MapManager.Selected.Value.DifficultyName,  0.50f)
        {
            Parent = Brightness,
            Alignment = Alignment.TopLeft,
        };

        private void CreateDifficultyRatingText() => TextDifficultyRating = new SpriteText(Fonts.Exo2BoldItalic24,
            MapManager.Selected.Value.DifficultyName, 0.50f)
        {
            Parent = Brightness,
            Alignment = Alignment.TopRight,
            TextAlignment = Alignment.TopRight,
            X = -20,
            Y = 20
            // todo: set text color to match diff gradient
        };

        /// <summary>
        ///     Creates the Creator text.
        /// </summary>
        private void CreateCreatorText() => TextCreator = new SpriteText(Fonts.Exo2Regular24,
            $"- By: {MapManager.Selected.Value.Creator}",  0.45f)
        {
            Parent = Brightness,
            Alignment = Alignment.TopLeft,
        };

        /// <summary>
        ///     Creates the mods text.
        /// </summary>
        private void CreateModsText()
        {
            TextMods = new SpriteText(Fonts.Exo2Italic24, "Mods: " + ModHelper.GetModsString(ModManager.Mods), 0.45f)
            {
                Parent = Brightness,
                Alignment = Alignment.TopRight,
            };

            AlignModsText();
        }

        /// <summary>
        ///     Realigns all of the text in the container.
        /// </summary>
        private void RealignSongInformationText()
        {
            const int leftSideSpacing = 15;

            var textArtistSize = TextArtist.MeasureString() / 2f;
            TextArtist.Position = new ScalableVector2(textArtistSize.X + leftSideSpacing, textArtistSize.Y + 25);

            var textTitleSize = TextTitle.MeasureString() / 2f;
            TextTitle.Position = new ScalableVector2(textTitleSize.X + leftSideSpacing, TextArtist.Y + textArtistSize.Y + 15);

            var textDifficultySize = TextDifficultyName.MeasureString() / 2f;
            TextDifficultyName.Position = new ScalableVector2(textDifficultySize.X + leftSideSpacing, TextTitle.Y + textTitleSize.Y + 15);

            var textCreatorSize = TextCreator.MeasureString() / 2f;
            TextCreator.Position = new ScalableVector2(textCreatorSize.X + leftSideSpacing, TextDifficultyName.Y + textDifficultySize.Y + 15);
        }

        /// <summary>
        ///     Whenever game modifiers changed, update the text of it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e)
        {
            TextMods.Text = "Mods: " + ModHelper.GetModsString(e.Mods);
            AlignModsText();
        }

        /// <summary>
        ///     Aligns the text for mods. This is separated because it needs to be updated as the mods change
        ///     Slightly reduces overhead of realigning all of them.
        /// </summary>
        private void AlignModsText()
        {
            var modsTextSize = TextMods.MeasureString() / 2f;
            TextMods.Position = new ScalableVector2(-modsTextSize.X - 15, 15);
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
            Difficulty = new MetadataContainer("DIFFICULTY", $"{MapManager.Selected.Value.QssData.OverallDifficulty:0.##}")
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
            TextDifficultyName.Text = MapManager.Selected.Value.DifficultyName;
            TextDifficultyRating.Text = string.Format("{0:N2}", MapManager.Selected.Value.DifficultyRating);
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