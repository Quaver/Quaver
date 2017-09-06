using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Main
{
    public class GameState : MonoBehaviour
    {
        public bool isActive = false;
        public GameState StateObject;
        public void StateStart()
        {
            if (StateObject == null)
            {
                StateObject = Instantiate(this);
                isActive = true;
            }
        }
        public void StateEnd()
        {
            if (StateObject != null)
            {
                Destroy(StateObject);
                isActive = false;
            }
        }
    }
}