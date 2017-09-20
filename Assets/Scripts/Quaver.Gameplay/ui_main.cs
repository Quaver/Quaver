using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Gameplay
{
    public partial class PlayScreen
    {
        private Transform _songPosBar;

        private void ui_init()
        {
            _songPosBar = StateUI.transform.Find("ProgressBar").transform.Find("Scale");

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

        private void ui_Update()
        {
            _songPosBar.localScale = new Vector2(_songAudio.time/_songAudio.clip.length, 1);
            nps_Update();
        }

        //Update score + display score UI
        private void ui_ScoreChange(int toChange, float hitOffset = -1f)
        {
            //Update Score
            _ScoreSpread[toChange]++;

            int totalNotes = 0;
            for (int i = 0; i < 9; i++) totalNotes += _ScoreSpread[i];
            //MARV = 100%, PERF = 100%, GREAT = 50%, GOOD = 25%, BAD = 0%, MISS = 0% //EARLY = 0%, LATE = 50%
            _acc = 100f * ((_ScoreSpread[0] + _ScoreSpread[1] + _ScoreSpread[6] + (_ScoreSpread[2] * 0.5f) 
                + (_ScoreSpread[3] * 0.25f) + (_ScoreSpread[8] * 0.5f)) / Mathf.Max(totalNotes, 1));

            //Update MA UI
            ma_Update(toChange);

            //Display judge
        }

    }
}
