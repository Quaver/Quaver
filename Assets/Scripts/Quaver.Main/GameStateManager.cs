using Quaver.Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour {
    public GameState[] States;
    //public GameObject[] GameStates;
    // Use this for initialization

	void Start () {
        States[0].StateStart();
    }

    // Update is called once per frame
    float test = 0;
    bool tested = false;
	void Update () {
        test+= Time.deltaTime;
        if (!tested && test > 5)
        {
            States[0].StateEnd();
            //print("endded");
            //States[0].StateStart();
            tested = true;
        }
    }
}
