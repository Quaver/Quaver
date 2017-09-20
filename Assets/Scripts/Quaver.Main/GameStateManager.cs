﻿
using Quaver.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Config;
using Quaver.Screenshot;
using Quaver.SongSelect;
using Quaver.Cache;
using Quaver.Audio;
using Quaver.Graphics;
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
        public static List<CachedBeatmap> LoadedBeatmaps = new List<CachedBeatmap>();
        public static List<MapDirectory> MapDirectories = new List<MapDirectory>();

        // IMPORTANT! This will hold the currently selected map in our MapDirectories list.
        // It will be used to boot up the main menu music & auto-select the map when the user
        // goes to the song selection screen.
        public CachedBeatmap currentMap;

        // IMPORTANT! This will hold all of the directories in that need to be refreshed
        // this means there was a detected change, and when the user decides to refresh
        // their game, it will handle the change
        public static List<string> SongDirectoryChangeQueue = new List<string>();

        // This will hold the current Window Title. We change it everytime a new beatmap is played.
        public static string WindowTitle;

        //ACTIVE STATES
        public List<GameState> ActiveStates = new List<GameState>();

        // Reference Variables
        public GameState[] States;
        public ParticleSystem DustRenderer;
        public GameObject bgImage;
        public AudioSource SongAudioSource;
        public Camera MainCamera;
        public Camera BgCamera;
        public Camera BlackCamera;

        //Game Canvas/UI
        public GameObject MainCanvas;
        public GameObject ActiveStateUISet;

        //Where the Gamestates will get cloned into
        public GameObject ActiveStateSet;

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

            // IMPORTANT! This will watch the songs directory for any changes. When it detects changes,
            // the user will be prompted to refresh their directory to handle the changes.
            BeatmapWatcher.Watch(GameConfig);

            WindowTitle = "Quaver";
        }

        private void Start()
        {
            //Changes the cameramode so sprites dont clip
            GameObject.Find("Main Camera").GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;

            //Do game start stuff here
            //Starts play mode (TEST)
            States[0].StateStart(this);

            try
            {
                AudioPlayer.LoadSong(currentMap, SongAudioSource);
                BackgroundLoader.LoadSprite(currentMap, bgImage);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[GAME MANAGER] Could not load any song!");
                Debug.LogException(e);
            }

            UpdateResolution();
        }

        //TEMP
        //public float WindowHeight = 720f;
        //public float WindowWidth = 1280f;

        //Gets called when resolution of game gets changed
        private void UpdateResolution()
        {
            float configWidth = GameConfig.WindowWidth;
            float configHeight = GameConfig.WindowHeight;
            if (!GameConfig.WindowLetterboxed)
            {
                configWidth = Screen.width;
                configHeight = Screen.height;
            }


            float newResolution = configWidth / configHeight;
            float defResolution = 1920f / 1080f;
            if (newResolution >= 1920f / 1080f)
            {
                ActiveStateUISet.GetComponent<RectTransform>().sizeDelta = new Vector2((newResolution/ defResolution )* 1920f,1080f);
                MainCanvas.GetComponent<CanvasScaler>().scaleFactor = 0.0001f + (configHeight / 1080f);
            }
            else
            {
                ActiveStateUISet.GetComponent<RectTransform>().sizeDelta = new Vector2(1920f, 1080f* (defResolution/newResolution));
                MainCanvas.GetComponent<CanvasScaler>().scaleFactor = 0.0001f + (configWidth / 1920f);
            }
            float newWidth = configWidth / Screen.width;
            float newHeight = configHeight / Screen.height;
            Rect newCameraRect = new Rect((1 - newWidth) / 2f, (1 - newHeight) / 2f, newWidth, newHeight);
            MainCamera.rect = newCameraRect;
            BgCamera.rect = newCameraRect;
        }

        private void Update()
        {
            // Handle screenshots
            ScreenshotService.Capture(GameConfig);
            
            // This will constantly check if the user has pressed F5, while any song directories are 
            // in the queue. If they are, it will refresh that directory.
            BeatmapCacheIndex.RefreshDirectory(GameConfig);
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
                int randomMapNumber = UnityEngine.Random.Range(0, LoadedBeatmaps.Count);
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
                _testState = nextState;
            }
            else if (_testState == 2)
            {
                int nextState = 1;
                int curState = _testState;
                States[curState].StateEnd();
                States[nextState].StateStart(this);
                _testState = nextState;
            }
        }
    }
}