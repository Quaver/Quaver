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
            nps_init();

            //Create MA display
            ma_init();
        }

        private void ui_Reset()
        {
            ma_Reset();
            nps_Reset();
        }

        //Update score + display score UI
        private void ui_ScoreChange(int toChange, float hitOffset = -1f)
        {
            //Update Score
            _ScoreSpread[toChange]++;

            int totalNotes = 0;
            for (int i = 0; i < 9; i++) totalNotes += _ScoreSpread[i];
            _acc = 100f * ((_ScoreSpread[0] + _ScoreSpread[1] + _ScoreSpread[6]
                + (_ScoreSpread[2] * 0.75f) + (_ScoreSpread[3] * 0.5f) + (_ScoreSpread[4] * 0.25f)
                + (_ScoreSpread[7] * 0.25f) + (_ScoreSpread[8] * 0.75f)) / Mathf.Max(totalNotes, 1));

            //Update MA UI
            ma_Update(toChange);

            //Display judge
        }

    }
}
