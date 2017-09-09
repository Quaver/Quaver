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
        public GameState[] States;
        public GameObject loadingScreenTest;
        public Camera CameraBlur;

        //Declare display ui text
        private float FpsTextWeen;
        private float LatencyTextTween;

        public Text FpsText;
        public Text LatencyText;

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

            if (testState == 2 && CameraBlur.GetComponent<Blur>().iterations > 0 )
            {
                CameraBlur.GetComponent<Blur>().iterations--;
            }
            else if (CameraBlur.GetComponent<Blur>().enabled && CameraBlur.GetComponent<Blur>().iterations == 0)
            {
                CameraBlur.GetComponent<Blur>().enabled = false;
            }

        }
        
        public void SwitchState()
        {
            //Test button click
            if (testState <= 1)
            {
                print(testState);
                int nextState = testState+1;
                int curState = testState;
                States[curState].StateEnd();
                States[nextState].StateStart();
                testState++;
            }

        }
    }
}