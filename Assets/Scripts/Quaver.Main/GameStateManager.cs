using Quaver.Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour {
    public GameState[] States;
    //public GameObject[] GameStates;
    // Use this for initialization

    private GameState swag;
	void Start () {
        swag = States[0];
        swag.StateStart();
    }
	
	// Update is called once per frame
	void Update () {
    }
}
