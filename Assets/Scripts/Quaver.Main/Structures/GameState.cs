using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Main
{
    public class GameState : MonoBehaviour
    {
        public bool isActive = false;
        public GameObject StateObject;
        //State Start
        public void StateStart()
        {
            if (StateObject == null)
            {
                isActive = true;
                StateObject = Instantiate(this.gameObject);
            }
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