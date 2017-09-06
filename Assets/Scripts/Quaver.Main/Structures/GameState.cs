using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Main
{
    public class GameState : MonoBehaviour
    {
        public bool isActive = false;
        public GameObject StateObject;
        public void StateStart()
        {
            if (StateObject == null)
            {
                isActive = true;
                StateObject = Instantiate(this.gameObject);
            }
        }
        public void StateEnd()
        {
            if (StateObject != null)
            {
                isActive = false;
                print("State Ended");
                Destroy(StateObject);
            }
        }
    }
}