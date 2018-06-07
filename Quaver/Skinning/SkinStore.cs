using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Config;
using Quaver.Helpers;

namespace Quaver.Skinning
{
    internal class SkinStore
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
        internal Dictionary<GameMode, SkinKeys> KeysSkins { get; }

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
        ///     Ctor - Loads up a skin from a given directory.
        /// </summary>
        internal SkinStore()
        {
            LoadConfig();

            // Load up Keys game mode skins.
            KeysSkins = new Dictionary<GameMode, SkinKeys>
            {
                {GameMode.Keys4, new SkinKeys(this, GameMode.Keys4)},
                {GameMode.Keys7, new SkinKeys(this, GameMode.Keys7)}
            };
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
        ///     Loads a single texture element.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resource"></param>
        internal static Texture2D LoadSingleTexture(string path, string resource)
        {
            return File.Exists(path) ? GraphicsHelper.LoadTexture2DFromFile(path) : ResourceHelper.LoadTexture(resource);
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
                    var regex = new Regex(SpritesheetRegex($"{element}{extension}"));
                    var match = regex.Match(Path.GetFileName(f));
                
                    // See if the file matches the regex.
                    if (match.Success)
                    {                    
                        // Load it up if so.
                        var texture = GraphicsHelper.LoadTexture2DFromFile(f);
                        return GraphicsHelper.LoadSpritesheetFromTexture(texture, int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
                    }
                
                    // Otherwise check to see if that base element (without animations) actually exists.
                    // if so, load it singularly into a list.
                    if (Path.GetFileNameWithoutExtension(f) == element)
                        return new List<Texture2D> { GraphicsHelper.LoadTexture2DFromFile(f) };
                }
            }
                
            // If we end up getting down here, that means we need to load the spritesheet from our resources.
            // if 0x0 is specified for the default, then it'll simply load the element without rowsxcolumns
            if (rows == 0 && columns == 0)
                return new List<Texture2D> { LoadSingleTexture($"{dir}/{element}{extension}", resource)};

            return GraphicsHelper.LoadSpritesheetFromTexture(ResourceHelper.LoadTexture($"{resource}_{rows}x{columns}"), rows, columns);
        }
    }
}