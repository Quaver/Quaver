using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quaver.Gameplay
{
    public partial class PlayScreen
    {
        private Text[] _maText;
        private GameObject _maDisplay;

        private void ma_init()
        {
            if (_config_EnableMAdisplay)
            {
                _maDisplay = StateUI.transform.Find("maInfo").gameObject;
                _maText = new Text[10];
                _maText[0] = _maDisplay.transform.Find("Marv").GetComponent<Text>();
                _maText[1] = _maDisplay.transform.Find("Perf").GetComponent<Text>();
                _maText[2] = _maDisplay.transform.Find("Great").GetComponent<Text>();
                _maText[3] = _maDisplay.transform.Find("Good").GetComponent<Text>();
                _maText[4] = _maDisplay.transform.Find("Bad").GetComponent<Text>();
                _maText[5] = _maDisplay.transform.Find("Miss").GetComponent<Text>();
                _maText[6] = _maDisplay.transform.Find("Ace").GetComponent<Text>();
                _maText[7] = _maDisplay.transform.Find("Early").GetComponent<Text>();
                _maText[8] = _maDisplay.transform.Find("Late").GetComponent<Text>();
                _maText[9] = _maDisplay.transform.Find("Acc").GetComponent<Text>();
            }
        }

        private void ma_Reset()
        {
            if (_config_EnableMAdisplay)
            {
                for (int i = 0; i < 9; i++) _maText[i].text = "0";
                _maText[9].text = "0.00%";
            }
        }

        private void ma_Update(int toChange)
        {
            if (_config_EnableMAdisplay)
            {
                //Set text
                _maText[toChange].text = _ScoreSpread[toChange].ToString();
                _maText[9].text = string.Format("{0:f2}", _acc) + "%";
            }
        }
    }
}
