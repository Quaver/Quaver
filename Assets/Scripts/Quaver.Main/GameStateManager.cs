using Quaver.Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Config;
using Quaver.Main.Screenshots;

public class GameStateManager : MonoBehaviour {
    // Config File
    public Cfg GameConfig;
    public GameState[] States;
    public GameObject loadingScreenTest;
    private float test = 0;
    private bool tested = false;

    void Awake()
    {
        // The first thing we want to do is load our config before anything else.
        GameConfig = ConfigLoader.Load();
    }

    private void Start () {
        //Changes the cameramode so sprites dont clip
        GameObject.Find("Main Camera").GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;

        //Do game start stuff here
        //Starts play mode (TEST)
        States[0].StateStart();
    }

    private void Update () {

        // Handle screenshots
        ScreenshotService.Capture(GameConfig);

        //TEST. Remove later.
        test+= Time.deltaTime;
        if (!tested && test > 5)
        {
            //loadingScreenTest.active = true; // SHOW LOADING SCREEN
            States[0].StateEnd();
            States[1].StateStart();
            print("LOADED");
            tested = true;
        }
    }
}
