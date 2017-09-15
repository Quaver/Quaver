// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using Quaver.Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Config;
using Quaver.Screenshot;
using Quaver.SongSelect;
using Quaver.Cache;
using Quaver.Audio;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

namespace Quaver.Main
{
    public class GameStateManager : MonoBehaviour
    {
        // IMPORTANT! These are the parsed config & all the loaded beatmaps!
        // These need to be loaded first before any other state begins!!!!!!
        // VERY IMPORTANT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public Cfg GameConfig;
        public List<CachedBeatmap> LoadedBeatmaps = new List<CachedBeatmap>();
        public List<MapDirectory> MapDirectories = new List<MapDirectory>();

        // IMPORTANT! This will hold the currently selected map in our MapDirectories list.
        // It will be used to boot up the main menu music & auto-select the map when the user
        // goes to the song selection screen.
        public CachedBeatmap currentMap;

        // Reference Variables
        public GameState[] States;
        public ParticleSystem DustRenderer;
        public GameObject bgImage;
        public AudioSource SongAudioSource;

        // UI Variables
        public float SelectScreenDim = 0.7f;

        // Test (remove later)
        public GameObject loadingScreenTest;
        private int _testState = 0;

        private void Awake()
        {
            // TODO: We'll want to initialize a state here which will be our loading screen
            // before the game begin. Only then will we continue forward to the main menu
            // when everything has loaded.
            // States[0].StateStart(this);
            LoadConfiguration();

            // IMPORTANT! This will make the game not pause when it loses focus. This 
            // should be set to true at the beginning of the game, so songs can be heard even when
            // the game is minimized.
            Application.runInBackground = true;
        }

        private void Start()
        {
            //Changes the cameramode so sprites dont clip
            GameObject.Find("Main Camera").GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;

            //Do game start stuff here
            //Starts play mode (TEST)
            States[0].StateStart(this);

            AudioPlayer.LoadSong(currentMap, SongAudioSource);
        }

        private void Update()
        {
            // Handle screenshots
            ScreenshotService.Capture(GameConfig);

            if (_testState >= 1)
            {
                DustRenderer.emissionRate = 120;
            }
        }

        // This is the first thing that will be called upon Game Start. It is responsible for
        // creating and syncing our beatmap database, loading our configuration file,
        // and sorting the loaded beatmaps into their respective directories, so they can 
        // be used during song select.
        private void LoadConfiguration()
        {
            // Create and/or sync the beatmap database.
            BeatmapCacheIndex.CreateDatabase();

            // The first thing we want to do is load our config before anything else.
            GameConfig = ConfigLoader.Load();

            // Here, we load all the beatmaps from the database, these are all the playable maps.
            LoadedBeatmaps = BeatmapCacheIndex.LoadBeatmaps(GameConfig);

            // Now, we want to sort all of the beatmaps.
            MapDirectories = OrderMapsBy.Directory(LoadedBeatmaps);
            MapDirectories = OrderMapsBy.Artist(MapDirectories);

            // Select a random map from the map directories, which will be our first selected map.
            // Generate a random number from the list of loaded beatmaps, then we will
            // go through them and find the correct map.
            if (MapDirectories.Count != 0)
            {
                int randomMapNumber = Random.Range(0, LoadedBeatmaps.Count);
                int mapsLookedThrough = 0;
                foreach (MapDirectory dir in MapDirectories)
                {
                    foreach (CachedBeatmap map in dir.Beatmaps)
                    {
                        // If the maps looked through is equivalent to the random map number we generated
                        // then we consider that the selected map.
                        if (mapsLookedThrough == randomMapNumber)
                        {
                            currentMap = map;
                        }

                        mapsLookedThrough++;
                    }
                }

                Debug.Log("[CONFIG LOADER] The random map selected on start up is: " + currentMap.Path);
            }

            // This is all that we need to load, so we can continue on with the main menu state.
            Debug.Log("[CONFIG LOADER] There were: " + LoadedBeatmaps.Count + " beatmaps in " + MapDirectories.Count + " directories loaded.");
        }

        // This is a method used for testing to switch our state to the next one. This will eventually
        // be removed, once we have proper buttons to switch states.
        public void SwitchState()
        {
            //Test button click
            if (_testState <= 1)
            {
                int nextState = _testState + 1;
                int curState = _testState;
                States[curState].StateEnd();
                States[nextState].StateStart(this);
                _testState++;
            }
        }
    }
}