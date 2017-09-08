using Quaver.Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Config;
using Quaver.Screenshot;
using UnityEngine.UI;

namespace Quaver.Main
{

    public class GameStateManager : MonoBehaviour
    {
        // Config File
        public Cfg GameConfig;
        public GameState[] States;
        public GameObject loadingScreenTest;

        //Declare display ui text
        private float FpsTextWeen;
        private float LatencyTextTween;

        public Text FpsText;
        public Text LatencyText;

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

            //Set Text of fps/latency ui
            LatencyTextTween += (500 * Time.deltaTime - LatencyTextTween) /100f;
            FpsTextWeen += (1 / Time.deltaTime - FpsTextWeen)/100f;
            FpsText.text = Mathf.Round(FpsTextWeen * 10)/10f + " fps";
            LatencyText.text = "±"+Mathf.Round(LatencyTextTween*100f) /100f + " ms";
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