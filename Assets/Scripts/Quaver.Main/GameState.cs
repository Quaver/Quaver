using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Main
{
    public class GameState : MonoBehaviour
    {
        public bool isActive = false;
        public bool loaded = false;
        public GameObject StateObject;
        public GameStateManager Manager;

        //State Start
        public bool StateStart(GameStateManager newManager)
        {
            if (StateObject == null)
            {
                isActive = true;
                Manager = newManager;
                StateObject = Instantiate(this.gameObject);
            }
            return true;
        }
        //State End
        public void StateEnd()
        {
            if (StateObject != null)
            {
                isActive = false;
                Destroy(StateObject);
            }
        }
    }
}