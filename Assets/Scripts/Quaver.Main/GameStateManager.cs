using Quaver.Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Config;
using Quaver.Screenshot;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

namespace Quaver.Main
{

    public class GameStateManager : MonoBehaviour
    {
        // Config File
        public Cfg GameConfig;

        // Reference Varialbes
        public GameState[] States;
        public ParticleSystem DustRenderer;
        public GameObject bgImage;
        public AudioSource SongAudioSource;

        // Test (remove later)
        public GameObject loadingScreenTest;
        private int testState = 0;

        private void Awake()
        {
            // The first thing we want to do is load our config before anything else.
            GameConfig = ConfigLoader.Load();
        }

        private void Start()
        {
            //Changes the cameramode so sprites dont clip
            GameObject.Find("Main Camera").GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;

            //Do game start stuff here
            //Starts play mode (TEST)
            States[0].StateStart(this);



            /*
            // This is just a test for the songs folder, we'll choose a random beatmap later instead of just choosing a 
            // random song, but here's sample code for loading a .WAV
            string[] files = Directory.GetFiles(ConfigDefault.SongDirectory, "*.ogg", SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                Debug.LogError("No .ogg files detected in folder!");
            }

            string url = "file:///" + files[Random.Range(0, files.Length)];
            gameAudio = GetComponent<AudioSource>();

            WWW audioLoader = new WWW(url);

            while (!audioLoader.isDone)
            {
                Debug.Log("Loading beatmap & audio track...");
            }

            if (audioLoader.isDone)
            {
                gameAudio.clip = audioLoader.GetAudioClip(false, false, AudioType.OGGVORBIS);

                if (!gameAudio.isPlaying && gameAudio.clip.isReadyToPlay)
                {
                    Debug.Log("Beatmap & Audio Track have been loaded, beginning to play.");
                    gameAudio.Play();
                }
            }*/

        }

        private void Update()
        {

            // Handle screenshots
            ScreenshotService.Capture(GameConfig);
            
            if (testState >= 1)
            {
                DustRenderer.emissionRate = 120;
            }

        }
        
        public void SwitchState()
        {
            //Test button click
            if (testState <= 1)
            {
                int nextState = testState+1;
                int curState = testState;
                States[curState].StateEnd();
                States[nextState].StateStart(this);
                testState++;
            }

        }
    }
}