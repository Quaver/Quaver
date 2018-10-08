using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Resources;
using System.Text.RegularExpressions;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Config;
using Quaver.Helpers;
using Quaver.Resources;
using Wobble;
using Wobble.Assets;
using Wobble.Audio.Samples;
using Wobble.Graphics;
using Wobble.Logging;

namespace Quaver.Skinning
{
    public class SkinStore
    {
         /// <summary>
        ///     The directory of the skin.
        /// </summary>
        internal static string Dir => $"{ConfigManager.SkinDirectory.Value}/{ConfigManager.Skin.Value}/";

        /// <summary>
        ///     The skin.ini file.
        /// </summary>
        internal IniData Config { get; private set;  }

        /// <summary>
        ///     Dictionary that contains both skins for 4K & 7K
        /// </summary>
        internal Dictionary<GameMode, SkinKeys> Keys { get; }

        /// <summary>
        ///     The name of the skin.
        /// </summary>
        internal string Name { get; private set; } = "Default Quaver Skin";

        /// <summary>
        ///     The author of the skin.
        /// </summary>
        internal string Author { get; private set; } = "Quaver Team";

        /// <summary>
        ///     The version of the skin.
        /// </summary>
        internal string Version { get; private set; } = "v0.1";

        /// <summary>
        ///     Regular expression for spritesheets
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal static string SpritesheetRegex(string element) => $@"^{element}@(\d+)x(\d+).png$";

        /// <summary>
        ///     The user's mouse cursor.
        /// </summary>
        internal Texture2D Cursor { get; private set; }

        /// <summary>
        ///     Grade Textures.
        /// </summary>
        internal Dictionary<Grade, Texture2D> Grades { get; } = new Dictionary<Grade, Texture2D>();

        /// <summary>
        ///     Judgement animation elements
        /// </summary>
        internal Dictionary<Judgement, List<Texture2D>> Judgements { get; } = new Dictionary<Judgement, List<Texture2D>>();

        /// <summary>
        ///     The numbers that display the user's current score.
        /// </summary>
        internal Texture2D[] ScoreDisplayNumbers { get; } = new Texture2D[10];

        /// <summary>
        ///     The decimal "." character in the score display.
        /// </summary>
        internal Texture2D ScoreDisplayDecimal { get; private set; }

        /// <summary>
        ///     The percent "%" character in the score display.
        /// </summary>
        internal Texture2D ScoreDisplayPercent { get; private set; }

        /// <summary>
        ///     The numbers that display the user's current combo
        /// </summary>
        internal Texture2D[] ComboDisplayNumbers = new Texture2D[10];

        /// <summary>
        ///     The numbers that display the current song time.
        /// </summary>
        internal Texture2D[] SongTimeDisplayNumbers = new Texture2D[10];

        /// <summary>
        ///     The ":" character displayed in the song time display.
        /// </summary>
        internal Texture2D SongTimeDisplayColon { get; private set; }

        /// <summary>
        ///     The minus "-" character displayed in the song time display.
        /// </summary>
        internal Texture2D SongTimeDisplayMinus { get; private set; }

        /// <summary>
        ///     The user's background displayed during the pause menu.
        /// </summary>
        internal Texture2D PauseBackground { get; private set; }

        /// <summary>
        ///     The continue button displayed in the pause menu
        /// </summary>
        internal Texture2D PauseContinue { get; private set; }

        /// <summary>
        ///     The retry button displayed in the pause menu
        /// </summary>
        internal Texture2D PauseRetry { get; private set; }

        /// <summary>
        ///     The back button displayed in the pause menu.
        /// </summary>
        internal Texture2D PauseBack { get; private set; }

        /// <summary>
        ///     The overlay that displayed the judgement counts.
        /// </summary>
        internal Texture2D JudgementOverlay { get; private set; }

        /// <summary>
        ///     The scoreboard displayed on the screen for the player.
        /// </summary>
        internal Texture2D Scoreboard { get; private set; }

        /// <summary>
        ///     The scoreboard displayed for other players.
        /// </summary>
        internal Texture2D ScoreboardOther { get; private set; }

        /// <summary>
        ///     The health bar displayed in the background. (Non-Moving one.)
        /// </summary>
        internal List<Texture2D> HealthBarBackground { get; private set; }

        /// <summary>
        ///     The health bar displayed in the foreground (Moving)
        /// </summary>
        internal List<Texture2D> HealthBarForeground { get; private set; }

        /// <summary>
        ///     Skip animation when user is on a break.
        /// </summary>
        internal List<Texture2D> Skip { get; private set; }

        /// <summary>
        ///     Sound effect elements.
        /// </summary>
        internal AudioSample SoundHit { get; private set; }
        internal AudioSample SoundHitClap { get; private set; }
        internal AudioSample SoundHitWhistle { get; private set; }
        internal AudioSample SoundHitFinish { get; private set; }
        internal AudioSample SoundComboBreak { get; private set; }
        internal AudioSample SoundApplause { get; private set; }
        internal AudioSample SoundScreenshot { get; private set; }
        internal AudioSample SoundClick { get; private set; }
        internal AudioSample SoundBack { get; private set; }
        internal AudioSample SoundHover { get; private set; }
        internal AudioSample SoundFailure { get; private set; }
        internal AudioSample SoundRetry { get; private set; }

        /// <summary>
        ///     Ctor - Loads up a skin from a given directory.
        /// </summary>
        internal SkinStore()
        {
            LoadConfig();

            // Load up Keys game mode skins.
            Keys = new Dictionary<GameMode, SkinKeys>
            {
                {GameMode.Keys4, new SkinKeys(this, GameMode.Keys4)},
                {GameMode.Keys7, new SkinKeys(this, GameMode.Keys7)}
            };

            LoadUniversalElements();

            // Change cursor image.
            GameBase.Game.GlobalUserInterface.Cursor.Image = Cursor;
        }

        /// <summary>
        ///     Loads up the config file and its default elements.
        /// </summary>
        private void LoadConfig()
        {
            const string name = "skin.ini";

            if (!File.Exists($"{Dir}/{name}"))
                return;

            Config = new FileIniDataParser().ReadFile($"{Dir}/{name}");

            // Parse very general things in config.
            Name = ConfigHelper.ReadString(Name, Config["General"]["Name"]);
            Author = ConfigHelper.ReadString(Author, Config["General"]["Author"]);
            Version = ConfigHelper.ReadString(Version, Config["General"]["Version"]);
        }

        /// <summary>
        ///     Loads universal skin elements used across every single game mode.
        /// </summary>
        private void LoadUniversalElements()
        {
            const string cursor = "main-cursor";
            Cursor = LoadSingleTexture($"{Dir}/Cursor/{cursor}", QuaverResources.main_cursor);

            LoadGradeElements();
            LoadJudgements();
            LoadNumberDisplays();
            LoadPause();
            LoadScoreboard();
            LoadHealthBar();
            LoadSkip();
            LoadSoundEffects();
        }

        /// <summary>
        ///     Loads a single texture element.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resource"></param>
        /// <param name="extension"></param>
        internal static Texture2D LoadSingleTexture(string path, byte[] resource, string extension = ".png")
        {
            path += extension;
            return File.Exists(path) ? AssetLoader.LoadTexture2DFromFile(path) : AssetLoader.LoadTexture2D(resource);
        }

        /// <summary>
        ///     Loads a single texture element.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resource"></param>
        /// <param name="extension"></param>
        internal static Texture2D LoadSingleTexture(ResourceManager rm, string path, string resource, string extension = ".png")
        {
            path += extension;
            return File.Exists(path) ? AssetLoader.LoadTexture2DFromFile(path) : AssetLoader.LoadTexture2D(rm, resource);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="element"></param>
        /// <param name="resource"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        internal static List<Texture2D> LoadSpritesheet(string folder, string element, string resource, int rows, int columns, string extension = ".png")
        {
            var dir = $"{Dir}/{folder}";

            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir);

                foreach (var f in files)
                {
                    var regex = new Regex(SpritesheetRegex($"{element}"));
                    var match = regex.Match(Path.GetFileName(f));

                    // See if the file matches the regex.
                    if (match.Success)
                    {
                        // Load it up if so.
                        var texture = AssetLoader.LoadTexture2DFromFile(f);
                        return AssetLoader.LoadSpritesheetFromTexture(texture, int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
                    }

                    // Otherwise check to see if that base element (without animations) actually exists.
                    // if so, load it singularly into a list.
                    if (Path.GetFileNameWithoutExtension(f) == element)
                        return new List<Texture2D> { AssetLoader.LoadTexture2DFromFile(f) };
                }
            }

            // If we end up getting down here, that means we need to load the spritesheet from our resources.
            // if 0x0 is specified for the default, then it'll simply load the element without rowsxcolumns
            if (rows == 0 && columns == 0)
                return new List<Texture2D> { LoadSingleTexture(QuaverResources.ResourceManager, $"{dir}/{element}", resource)};

            return AssetLoader.LoadSpritesheetFromTexture(AssetLoader.LoadTexture2D(QuaverResources.ResourceManager, $"{resource}_{rows}x{columns}"), rows, columns);
        }

        /// <summary>
        ///     Loads .wav sound effect files.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private static AudioSample LoadSoundEffect(string path, string element)
        {
            path += ".wav";

            // Load the actual file stream if it exists.
            try
            {
                if (File.Exists(path))
                    return new AudioSample(path);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }

            // Load the default if the path doesn't exist
            return new AudioSample((UnmanagedMemoryStream)ResourceHelper.GetProperty(element));
        }

        /// <summary>
        ///     Loads all grade texture elements
        /// </summary>
        private void LoadGradeElements()
        {
            // Load Grades
            foreach (Grade grade in Enum.GetValues(typeof(Grade)))
            {
                if (grade == Grade.None)
                    continue;

                var element = $"grade-small-{grade.ToString().ToLower()}";

                byte[] defaultGrade = null;

                switch (grade)
                {
                    case Grade.None:
                        break;
                    case Grade.A:
                        defaultGrade = QuaverResources.grade_small_a;
                        break;
                    case Grade.B:
                        defaultGrade = QuaverResources.grade_small_b;
                        break;
                    case Grade.C:
                        defaultGrade = QuaverResources.grade_small_c;
                        break;
                    case Grade.D:
                        defaultGrade = QuaverResources.grade_small_d;
                        break;
                    case Grade.F:
                        defaultGrade = QuaverResources.grade_small_f;
                        break;
                    case Grade.S:
                        defaultGrade = QuaverResources.grade_small_s;
                        break;
                    case Grade.SS:
                        defaultGrade = QuaverResources.grade_small_ss;
                        break;
                    case Grade.X:
                        defaultGrade = QuaverResources.grade_small_x;
                        break;
                    case Grade.XX:
                        defaultGrade = QuaverResources.grade_small_xx;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Grades[grade] = LoadSingleTexture($"{Dir}/Grades/{element}", defaultGrade);
            }
        }

        /// <summary>
        ///     Loads judgement texture elements.
        /// </summary>
        private void LoadJudgements()
        {
            const string folder = "Judgements";

            // Load Judgements
            foreach (Judgement j in Enum.GetValues(typeof(Judgement)))
            {
                if (j == Judgement.Ghost)
                    return;

                var element = $"judge-{j.ToString().ToLower()}";           
               Judgements[j] = new List<Texture2D>() { LoadSingleTexture(QuaverResources.ResourceManager, $"{Dir}/{folder}/{element}", element) };
            }
            
            // Load judgement overlay
            const string judgementOverlay = "judgement-overlay";
            JudgementOverlay = LoadSingleTexture(QuaverResources.ResourceManager, $"{Dir}/{folder}/{judgementOverlay}", judgementOverlay);
        }

        /// <summary>
        ///     Loads all number display skin elements.
        /// </summary>
        private void LoadNumberDisplays()
        {
            // Load Number Displays
            var numberDisplayFolder = $"{Dir}/Numbers/";
            for (var i = 0; i < 10; i++)
            {
                // Score
                var scoreElement = $"score-{i}";
                ScoreDisplayNumbers[i] = LoadSingleTexture(QuaverResources.ResourceManager, $"{numberDisplayFolder}/{scoreElement}", $"{scoreElement}");

                // Combo
                var comboElement = $"combo-{i}";
                ComboDisplayNumbers[i] = LoadSingleTexture(QuaverResources.ResourceManager, $"{numberDisplayFolder}/{comboElement}", comboElement);

                // Song Time
                var songTimeElement = $"song-time-{i}";
                SongTimeDisplayNumbers[i] = LoadSingleTexture(QuaverResources.ResourceManager, $"{numberDisplayFolder}/{songTimeElement}", songTimeElement);
            }

            const string scoreDecimal = "score-decimal";
            ScoreDisplayDecimal = LoadSingleTexture($"{numberDisplayFolder}/{scoreDecimal}", QuaverResources.score_decimal);

            const string scorePercent = "score-percent";
            ScoreDisplayPercent = LoadSingleTexture($"{numberDisplayFolder}/{scorePercent}", QuaverResources.score_percent);

            const string songTimeColon = "song-time-colon";
            SongTimeDisplayColon = LoadSingleTexture($"{numberDisplayFolder}/{songTimeColon}", QuaverResources.song_time_colon);

            const string songTimeMinus = "song-time-minus";
            SongTimeDisplayMinus = LoadSingleTexture($"{numberDisplayFolder}/{songTimeMinus}", QuaverResources.song_time_minus);
        }

        /// <summary>
        ///     Loads all pause menu elements.
        /// </summary>
        private void LoadPause()
        {
            var pauseFolder = $"{Dir}/Pause/";

            const string pauseBackground = "pause-background";
            PauseBackground = LoadSingleTexture($"{pauseFolder}/{pauseBackground}", QuaverResources.pause_background);

            const string pauseContinue = "pause-continue";
            PauseContinue = LoadSingleTexture($"{pauseFolder}/{pauseContinue}", QuaverResources.pause_continue);

            const string pauseRetry = "pause-retry";
            PauseRetry = LoadSingleTexture($"{pauseFolder}/{pauseRetry}", QuaverResources.pause_retry);

            const string pauseBack = "pause-back";
            PauseBack = LoadSingleTexture($"{pauseFolder}/{pauseBack}", QuaverResources.pause_back);
        }

        /// <summary>
        ///     Loads all scoreboard elements.
        /// </summary>
        private void LoadScoreboard()
        {
            var scoreboardFolder = $"{Dir}/Scoreboard/";

            const string scoreboard = "scoreboard";
            Scoreboard = LoadSingleTexture($"{scoreboardFolder}/{scoreboard}", QuaverResources.scoreboard);

            const string scoreboardOther = "scoreboard-other";
            ScoreboardOther = LoadSingleTexture($"{scoreboardFolder}/{scoreboardOther}", QuaverResources.scoreboard_other);
        }

        /// <summary>
        ///     Loads all health bar elements.
        /// </summary>
        private void LoadHealthBar()
        {
            var healthFolder = $"/Health/";

            const string healthBackground = "health-background";
            HealthBarBackground = LoadSpritesheet(healthFolder, healthBackground, healthBackground, 0, 0);

            const string healthForeground = "health-foreground";
            HealthBarForeground = LoadSpritesheet(healthFolder, healthForeground, healthForeground, 0, 0);
        }

        /// <summary>
        ///     Loads the skip animation element.
        /// </summary>
        private void LoadSkip()
        {
            var skipFolder = $"/Skip/";
            const string skip = "skip";

            Skip = LoadSpritesheet(skipFolder, skip, skip, 1, 31);
        }

        /// <summary>
        ///     Loads all sound effect elements.
        /// </summary>
        private void LoadSoundEffects()
        {
            var sfxFolder = $"{Dir}/SFX/";

            const string soundHit = "sound-hit";
            SoundHit = LoadSoundEffect($"{sfxFolder}/{soundHit}", soundHit);

            const string soundHitClap = "sound-hitclap";
            SoundHitClap = LoadSoundEffect($"{sfxFolder}/{soundHitClap}", soundHitClap);

            const string soundHitWhistle = "sound-hitwhistle";
            SoundHitWhistle = LoadSoundEffect($"{sfxFolder}/{soundHitWhistle}", soundHitWhistle);

            const string soundHitFinish = "sound-hitfinish";
            SoundHitFinish = LoadSoundEffect($"{sfxFolder}/{soundHitFinish}", soundHitFinish);

            const string soundComboBreak = "sound-combobreak";
            SoundComboBreak = LoadSoundEffect($"{sfxFolder}/{soundComboBreak}", soundComboBreak);

            const string soundFailure = "sound-failure";
            SoundFailure = LoadSoundEffect($"{sfxFolder}/{soundFailure}", soundFailure);

            const string soundRetry = "sound-retry";
            SoundRetry = LoadSoundEffect($"{sfxFolder}/{soundRetry}", soundRetry);

            const string soundApplause = "sound-applause";
            SoundApplause = LoadSoundEffect($"{sfxFolder}/{soundApplause}", soundApplause);

            const string soundScreenshot = "sound-screenshot";
            SoundScreenshot = LoadSoundEffect($"{sfxFolder}/{soundScreenshot}", soundScreenshot);

            const string soundClick = "sound-click";
            SoundClick = LoadSoundEffect($"{sfxFolder}/{soundClick}", soundClick);

            const string soundBack = "sound-back";
            SoundBack = LoadSoundEffect($"{sfxFolder}/{soundBack}", soundBack);

            const string soundHover = "sound-hover";
            SoundHover = LoadSoundEffect($"{sfxFolder}/{soundHover}", soundHover);
        }
    }
}