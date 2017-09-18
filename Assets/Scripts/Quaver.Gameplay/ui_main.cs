using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Gameplay
{
    public partial class PlayScreen
    {
        private GameObject uiCanvas;

        private void ui_init()
        {
            uiCanvas = this.transform.Find("PlayCanvasUI").gameObject;

            //Creates nps graph
            if (_config_EnableNpsGraph) nps_init();
        }

    }
}
