
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Config
{
    public static class ConfigLoader
    {
        /// <summary>
        /// Loads a quaver.cfg file. This is usually done upon game start.
        /// If it can't load the file, it'll generate a new one.
        /// </summary>
        /// <returns></returns>
        public static Cfg Load()
        {
            string configPath = Application.dataPath + "/quaver.cfg";

            // Try and parse config file
            Cfg config = ConfigParser.Parse(configPath);

            // If there wasn't a valid config file, we want to go ahead and generate one
            // and try parsing it again.
            if (!config.IsValid)
            {
                Debug.LogWarning("No valid configuration file found. Generating a new one.");

                // Generate a config file, and receive whether it succeeded or failed.
                bool hasGenerated = ConfigGenerator.Generate();

                // we've successfully generated a config file, try and parse it.
                if (hasGenerated)
                {
                    config = ConfigParser.Parse(configPath);

                    // Check again if the newly generated config file is valid.
                    if (!config.IsValid)
                    {
                        Debug.LogError("Could not generate and parse config file after attempting to!");
                        Application.Quit(); // Quit the program if we cannot generate a configuration file. -- MAJOR BUG IF WE GET HERE!
                    }

                    Debug.Log("Config file successfully created and parsed!");
                }
            }

            return config;
        }
    }
}
