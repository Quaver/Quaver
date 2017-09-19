using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quaver.UI
{
    public class FPSCounter : MonoBehaviour
    {
        // Display ui text
        private float _fpsTextWeen;
        private float _latencyTextTween;
        public Text FpsText;
        public Text LatencyText;

        private void Update()
        {
            //Set Text of fps/latency ui
            _latencyTextTween += ((500f * Time.deltaTime) - _latencyTextTween) / 100f;
            _fpsTextWeen += ((1f / Time.deltaTime) - _fpsTextWeen) / 100f;
            FpsText.text = (Mathf.Round(_fpsTextWeen * 10f) / 10f) + " fps";
            LatencyText.text = "\u00B1" + (Mathf.Round(_latencyTextTween * 100f) / 100f) + " ms";
        }
    }
}