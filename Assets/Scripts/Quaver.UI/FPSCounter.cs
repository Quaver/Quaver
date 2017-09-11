using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quaver.UI
{

	public class FPSCounter : MonoBehaviour 
	{
		
		// Display ui text
		private float FpsTextWeen;
		private float LatencyTextTween;
		public Text FpsText;
		public Text LatencyText;

		void Update () 
		{
			//Set Text of fps/latency ui
			LatencyTextTween += (500 * Time.deltaTime - LatencyTextTween) /100f;
			FpsTextWeen += (1 / Time.deltaTime - FpsTextWeen)/100f;
			FpsText.text = Mathf.Round(FpsTextWeen * 10)/10f + " fps";
			LatencyText.text = "±"+Mathf.Round(LatencyTextTween*100f) /100f + " ms";		
		}
	}
}

