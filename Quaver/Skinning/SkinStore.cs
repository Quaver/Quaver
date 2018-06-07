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
        internal string Name { get; set; }
        
        /// <summary>
        ///     The author of the skin.
        /// </summary>
        internal string Author { get; set; }
        
        /// <summary>
        ///     The version of the skin.
        /// </summary>
        internal string Version { get; set; }

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
        }
    }
}