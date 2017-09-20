
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Main
{
    public class GameState : MonoBehaviour
    {
        public GameObject StateObject;
        public GameObject LoadedStateObject;
        public GameStateManager Manager;
        public GameObject UIObject;
        private bool loaded = false;

        //State Start
        public bool StateStart(GameStateManager newManager)
        {
            Manager = newManager;
            LoadedStateObject = Instantiate(StateObject,Manager.ActiveStateSet.transform);
            LoadedStateObject.GetComponent<GameStateObject>().Manager = Manager;
            loaded = true;
            return true;
        }
        //State End
        public void StateEnd()
        {
            if (loaded)
            {
                if (LoadedStateObject.GetComponent<GameStateObject>().StateUI != null)
                {
                    Destroy(LoadedStateObject.GetComponent<GameStateObject>().StateUI);
                }
                Destroy(LoadedStateObject);
            }
        }
    }
}