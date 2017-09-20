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

            //Create MA display
            if(_config_EnableMAdisplay) ma_init();
        }

        //Update score + display score UI
        private void ui_ScoreChange(int toChange)
        {
            //Update Score
            _ScoreSpread[toChange]++;

            //Update MA UI
            ma_Update(toChange);

            //Display judge

        }

    }
}
