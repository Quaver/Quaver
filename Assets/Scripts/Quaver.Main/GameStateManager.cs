using Quaver.Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Config;
using Quaver.Screenshot;

namespace Quaver.Main
{

    public class GameStateManager : MonoBehaviour
    {
        // Config File
        public Cfg GameConfig;
        public GameState[] States;
        public GameObject loadingScreenTest;

        //Temp Variable
        private bool tested = false;

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
            States[0].StateStart();

        }

        private void Update()
        {

            // Handle screenshots
            ScreenshotService.Capture(GameConfig);
        }
        public void SwitchState()
        {
            //Test button click
            if (!tested)
            {
                tested = true;
                int nextState = 1;
                int curState = 0;
                States[curState].StateEnd();
                States[nextState].StateStart();
            }
        }
    }
}