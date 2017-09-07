using Quaver.Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Config;

public class GameStateManager : MonoBehaviour {
    public GameState[] States;
    public GameObject loadingScreenTest;
    private float test = 0;
    private bool tested = false;

    void Awake()
    {
        LoadConfig();
    }

    private void Start () {
        //Changes the cameramode so sprites dont clip
        GameObject.Find("Main Camera").GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;

        //Do game start stuff here
        //Starts play mode (TEST)
        //States[0].StateStart();
    }

    private void Update () {

        //TEST. Remove later.
        test+= Time.deltaTime;
        if (!tested && test > 5)
        {
            //loadingScreenTest.active = true; // SHOW LOADING SCREEN
           // States[0].StateEnd();
            //States[1].StateStart();
            print("LOADED");
            tested = true;
        }
    }

    // Responsible for loading/creating a quaver.cfg file. Quaver configs should always be at the root directory.
    void LoadConfig()
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
                }

                Debug.Log("Config file successfully created and parsed!");

            }      
        }   
    }
}
