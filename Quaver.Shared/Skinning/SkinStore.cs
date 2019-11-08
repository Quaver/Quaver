/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using IniFileParser;
using IniFileParser.Model;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Wobble;
using Wobble.Assets;
using Wobble.Audio.Samples;
using Wobble.Logging;

namespace Quaver.Shared.Skinning
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
        internal IniData Config { get; private set; }

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
        ///     Whether the cursor should be centered.
        /// </summary>
        internal bool CenterCursor { get; private set; }

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
        ///     The scoreboard for the red team
        /// </summary>
        internal Texture2D ScoreboardRedTeam { get; set; }

        /// <summary>
        ///     The scoreboard for the red team (other players)
        /// </summary>
        internal Texture2D ScoreboardRedTeamOther { get; set; }

        /// <summary>
        ///     The scoreboard for the blue team
        /// </summary>
        internal Texture2D ScoreboardBlueTeam { get; set; }

        /// <summary>
        ///     The scoreboard for the blue team (other players)
        /// </summary>
        internal Texture2D ScoreboardBlueTeamOther { get; set; }

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
        ///     Displayed when the user achieves high combos
        /// </summary>
        internal List<Texture2D> ComboAlerts { get; private set; }

        /// <summary>
        ///     Displayed when being eliminated from battle royale
        /// </summary>
        internal Texture2D BattleRoyaleEliminated { get; private set; }

        /// <summary>
        ///     Displayed when in danger of being eliminated
        /// </summary>
        internal Texture2D BattleRoyaleWarning { get; private set; }

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
        internal List<AudioSample> SoundComboAlerts { get; private set; }

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
            GameBase.Game.GlobalUserInterface.Cursor.Center = CenterCursor;
        }

        /// <summary>
        ///     Loads up the config file and its default elements.
        /// </summary>
        private void LoadConfig()
        {
            const string name = "skin.ini";

            if (!File.Exists($"{Dir}/{name}"))
                return;

            Config = new IniFileParser.IniFileParser(new ConcatenateDuplicatedKeysIniDataParser()).ReadFile($"{Dir}/{name}");

            // Parse very general things in config.
            Name = ConfigHelper.ReadString(Name, Config["General"]["Name"]);
            Author = ConfigHelper.ReadString(Author, Config["General"]["Author"]);
            Version = ConfigHelper.ReadString(Version, Config["General"]["Version"]);
            CenterCursor = ConfigHelper.ReadBool(false, Config["General"]["CenterCursor"]);
        }

        /// <summary>
        ///     Loads universal skin elements used across every single game mode.
        /// </summary>
        private void LoadUniversalElements()
        {
            const string cursor = "main-cursor";
            Cursor = LoadSingleTexture($"{Dir}/Cursor/{cursor}", $"Quaver.Resources/Textures/Skins/Shared/Cursor/{cursor}.png");

            LoadGradeElements();
            LoadJudgements();
            LoadNumberDisplays();
            LoadPause();
            LoadScoreboard();
            LoadHealthBar();
            LoadSkip();
            LoadComboAlert();
            LoadMultiplayerElements();
            LoadSoundEffects();
        }

        /// <summary>
        ///     Loads a single texture element.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resource"></param>
        /// <param name="extension"></param>
        internal static Texture2D LoadSingleTexture(string path, string resource, string extension = ".png")
        {
            path += extension;

            try
            {
                return File.Exists(path)
                    ? AssetLoader.LoadTexture2DFromFile(path)
                    : AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get(resource));
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load: {resource}. Using default!", LogType.Runtime);
                return UserInterface.BlankBox;
            }
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
                return new List<Texture2D> { LoadSingleTexture( $"{dir}/{element}", resource + ".png")};

            return AssetLoader.LoadSpritesheetFromTexture(AssetLoader.LoadTexture2D(
                GameBase.Game.Resources.Get($"{resource}@{rows}x{columns}.png")), rows, columns);
        }

        /// <summary>
        ///     Loads .wav sound effect files.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private static AudioSample LoadSoundEffect(string path, string element, string resourceFolder)
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
            return new AudioSample(GameBase.Game.Resources.Get($"Quaver.Resources/SFX/{resourceFolder}/{element}.wav"));
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

                string defaultGrade = null;

                switch (grade)
                {
                    case Grade.None:
                        break;
                    case Grade.A:
                        defaultGrade = $"Quaver.Resources/Textures/Skins/Shared/Grades/grade-small-a.png";
                        break;
                    case Grade.B:
                        defaultGrade = $"Quaver.Resources/Textures/Skins/Shared/Grades/grade-small-b.png";
                        break;
                    case Grade.C:
                        defaultGrade = $"Quaver.Resources/Textures/Skins/Shared/Grades/grade-small-c.png";
                        break;
                    case Grade.D:
                        defaultGrade = $"Quaver.Resources/Textures/Skins/Shared/Grades/grade-small-d.png";
                        break;
                    case Grade.F:
                        defaultGrade = $"Quaver.Resources/Textures/Skins/Shared/Grades/grade-small-f.png";
                        break;
                    case Grade.S:
                        defaultGrade = $"Quaver.Resources/Textures/Skins/Shared/Grades/grade-small-s.png";
                        break;
                    case Grade.SS:
                        defaultGrade = $"Quaver.Resources/Textures/Skins/Shared/Grades/grade-small-ss.png";
                        break;
                    case Grade.X:
                        defaultGrade = $"Quaver.Resources/Textures/Skins/Shared/Grades/grade-small-x.png";
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
                    continue;

                var element = $"judge-{j.ToString().ToLower()}";
               Judgements[j] = new List<Texture2D>()
               {
                   LoadSingleTexture( $"{Dir}/{folder}/{element}", $"Quaver.Resources/Textures/Skins/Shared/Judgements/{element}.png")
               };
            }

            // Load judgement overlay
            const string judgementOverlay = "judgement-overlay";
            JudgementOverlay = LoadSingleTexture( $"{Dir}/{folder}/{judgementOverlay}",
                $"Quaver.Resources/Textures/Skins/Shared/Judgements/{judgementOverlay}.png");
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
                ScoreDisplayNumbers[i] = LoadSingleTexture( $"{numberDisplayFolder}/{scoreElement}",
                    $"Quaver.Resources/Textures/Skins/Shared/Numbers/{scoreElement}.png");

                // Combo
                var comboElement = $"combo-{i}";
                ComboDisplayNumbers[i] = LoadSingleTexture( $"{numberDisplayFolder}/{comboElement}",
                    $"Quaver.Resources/Textures/Skins/Shared/Numbers/{comboElement}.png");

                // Song Time
                var songTimeElement = $"song-time-{i}";
                SongTimeDisplayNumbers[i] = LoadSingleTexture( $"{numberDisplayFolder}/{songTimeElement}",
                    $"Quaver.Resources/Textures/Skins/Shared/Numbers/{songTimeElement}.png");
            }

            const string scoreDecimal = "score-decimal";
            ScoreDisplayDecimal = LoadSingleTexture($"{numberDisplayFolder}/{scoreDecimal}", $"Quaver.Resources/Textures/Skins/Shared/Numbers/{scoreDecimal}.png");

            const string scorePercent = "score-percent";
            ScoreDisplayPercent = LoadSingleTexture($"{numberDisplayFolder}/{scorePercent}", $"Quaver.Resources/Textures/Skins/Shared/Numbers/{scorePercent}.png");

            const string songTimeColon = "song-time-colon";
            SongTimeDisplayColon = LoadSingleTexture($"{numberDisplayFolder}/{songTimeColon}", $"Quaver.Resources/Textures/Skins/Shared/Numbers/{songTimeColon}.png");

            const string songTimeMinus = "song-time-minus";
            SongTimeDisplayMinus = LoadSingleTexture($"{numberDisplayFolder}/{songTimeMinus}", $"Quaver.Resources/Textures/Skins/Shared/Numbers/{songTimeMinus}.png");
        }

        /// <summary>
        ///     Loads all pause menu elements.
        /// </summary>
        private void LoadPause()
        {
            var pauseFolder = $"{Dir}/Pause/";

            const string pauseBackground = "pause-background";
            PauseBackground = LoadSingleTexture($"{pauseFolder}/{pauseBackground}", $"Quaver.Resources/Textures/Skins/Shared/Pause/{pauseBackground}.png");

            const string pauseContinue = "pause-continue";
            PauseContinue = LoadSingleTexture($"{pauseFolder}/{pauseContinue}", $"Quaver.Resources/Textures/Skins/Shared/Pause/{pauseContinue}.png");

            const string pauseRetry = "pause-retry";
            PauseRetry = LoadSingleTexture($"{pauseFolder}/{pauseRetry}", $"Quaver.Resources/Textures/Skins/Shared/Pause/{pauseRetry}.png");

            const string pauseBack = "pause-back";
            PauseBack = LoadSingleTexture($"{pauseFolder}/{pauseBack}", $"Quaver.Resources/Textures/Skins/Shared/Pause/{pauseBack}.png");
        }

        /// <summary>
        ///     Loads all scoreboard elements.
        /// </summary>
        private void LoadScoreboard()
        {
            var scoreboardFolder = $"{Dir}/Scoreboard/";

            const string scoreboard = "scoreboard";
            Scoreboard = LoadSingleTexture($"{scoreboardFolder}/{scoreboard}", $"Quaver.Resources/Textures/Skins/Shared/Scoreboard/{scoreboard}.png");

            const string scoreboardOther = "scoreboard-other";
            ScoreboardOther = LoadSingleTexture($"{scoreboardFolder}/{scoreboardOther}", $"Quaver.Resources/Textures/Skins/Shared/Scoreboard/{scoreboardOther}.png");

            const string scoreboardRedTeam = "scoreboard-red-team";
            ScoreboardRedTeam = LoadSingleTexture($"{scoreboardFolder}/{scoreboardRedTeam}", $"Quaver.Resources/Textures/Skins/Shared/Scoreboard/{scoreboardRedTeam}.png");

            const string scoreboardRedTeamOther = "scoreboard-red-team-other";
            ScoreboardRedTeamOther = LoadSingleTexture($"{scoreboardFolder}/{scoreboardRedTeamOther}", $"Quaver.Resources/Textures/Skins/Shared/Scoreboard/{scoreboardRedTeamOther}.png");

            const string scoreboardBlueTeam = "scoreboard-blue-team";
            ScoreboardBlueTeam = LoadSingleTexture($"{scoreboardFolder}/{scoreboardBlueTeam}", $"Quaver.Resources/Textures/Skins/Shared/Scoreboard/{scoreboardBlueTeam}.png");

            const string scoreboardBlueTeamOther = "scoreboard-blue-team-other";
            ScoreboardBlueTeamOther = LoadSingleTexture($"{scoreboardFolder}/{scoreboardBlueTeamOther}", $"Quaver.Resources/Textures/Skins/Shared/Scoreboard/{scoreboardBlueTeamOther}.png");
        }

        /// <summary>
        ///     Loads all health bar elements.
        /// </summary>
        private void LoadHealthBar()
        {
            var healthFolder = $"/Health/";

            const string healthBackground = "health-background";
            HealthBarBackground = LoadSpritesheet(healthFolder, healthBackground,
                $"Quaver.Resources/Textures/Skins/Shared/Health/health-background", 0, 0);

            const string healthForeground = "health-foreground";
            HealthBarForeground = LoadSpritesheet(healthFolder, healthForeground,
                $"Quaver.Resources/Textures/Skins/Shared/Health/health-foreground", 0, 0);
        }

        /// <summary>
        ///     Loads the skip animation element.
        /// </summary>
        private void LoadSkip()
        {
            var skipFolder = $"/Skip/";
            const string skip = "skip";

            Skip = LoadSpritesheet(skipFolder, skip, $"Quaver.Resources/Textures/Skins/Shared/Skip/{skip}", 0, 0);
        }

        /// <summary>
        ///     Loads combo alerts if they exist
        /// </summary>
        private void LoadComboAlert()
        {
            var comboAlertFolder = $"{Dir}/Combo/";

            const string comboAlert = "combo-alert";

            ComboAlerts = new List<Texture2D>();

            for (var i = 0; i < 100 && File.Exists($"{comboAlertFolder}/{comboAlert}-{i + 1}.png"); i++)
            {
                ComboAlerts.Add(LoadSingleTexture($"{comboAlertFolder}/{comboAlert}-{i + 1}",
                    $"Quaver.Resources/Textures/Skins/Shared/Combo/{comboAlert}-{i + 1}.png"));
            }
        }

        private void LoadMultiplayerElements()
        {
            var multiplayerFolder = $"{Dir}/Multiplayer/";
            const string battleRoyaleEliminated = "eliminated";

            BattleRoyaleEliminated = LoadSingleTexture($"{multiplayerFolder}/{battleRoyaleEliminated}"
                ,$"Quaver.Resources/Textures/Skins/Shared/Multiplayer/{battleRoyaleEliminated}.png");

            const string battleRoyaleWarning = "warning";
            BattleRoyaleWarning = LoadSingleTexture($"{multiplayerFolder}/{battleRoyaleWarning}"
                ,$"Quaver.Resources/Textures/Skins/Shared/Multiplayer/{battleRoyaleWarning}.png");
        }

        /// <summary>
        ///     Loads all sound effect elements.
        /// </summary>
        private void LoadSoundEffects()
        {
            var sfxFolder = $"{Dir}/SFX/";

            const string soundHit = "sound-hit";
            SoundHit = LoadSoundEffect($"{sfxFolder}/{soundHit}", soundHit, "Gameplay");

            const string soundHitClap = "sound-hitclap";
            SoundHitClap = LoadSoundEffect($"{sfxFolder}/{soundHitClap}", soundHitClap, "Gameplay");

            const string soundHitWhistle = "sound-hitwhistle";
            SoundHitWhistle = LoadSoundEffect($"{sfxFolder}/{soundHitWhistle}", soundHitWhistle, "Gameplay");

            const string soundHitFinish = "sound-hitfinish";
            SoundHitFinish = LoadSoundEffect($"{sfxFolder}/{soundHitFinish}", soundHitFinish, "Gameplay");

            const string soundComboBreak = "sound-combobreak";
            SoundComboBreak = LoadSoundEffect($"{sfxFolder}/{soundComboBreak}", soundComboBreak, "Gameplay");

            const string soundFailure = "sound-failure";
            SoundFailure = LoadSoundEffect($"{sfxFolder}/{soundFailure}", soundFailure, "Gameplay");

            const string soundRetry = "sound-retry";
            SoundRetry = LoadSoundEffect($"{sfxFolder}/{soundRetry}", soundRetry, "Gameplay");

            const string soundApplause = "sound-applause";
            SoundApplause = LoadSoundEffect($"{sfxFolder}/{soundApplause}", soundApplause, "Menu");

            const string soundScreenshot = "sound-screenshot";
            SoundScreenshot = LoadSoundEffect($"{sfxFolder}/{soundScreenshot}", soundScreenshot, "Menu");

            const string soundClick = "sound-click";
            SoundClick = LoadSoundEffect($"{sfxFolder}/{soundClick}", soundClick, "Menu");

            const string soundBack = "sound-back";
            SoundBack = LoadSoundEffect($"{sfxFolder}/{soundBack}", soundBack, "Menu");

            const string soundHover = "sound-hover";
            SoundHover = LoadSoundEffect($"{sfxFolder}/{soundHover}", soundHover, "Menu");

            const string soundComboAlert = "sound-combo-alert";
            SoundComboAlerts = new List<AudioSample>();

            for (var i = 0; i < 100 && File.Exists($"{sfxFolder}/{soundComboAlert}-{i + 1}.wav"); i++)
                SoundComboAlerts.Add(LoadSoundEffect($"{sfxFolder}/{soundComboAlert}-{i + 1}", soundComboAlert + "-" + i + 1, "Menu"));
        }
    }
}
