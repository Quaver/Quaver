using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSoundVisualizer : MonoBehaviour {

    //Assign these manually
    public bool isActive = true;
    public ParticleSystem particle;
    private ParticleSystem.MainModule main;

    //Reference Variables
    private float[] spectrum = new float[256];
    private float bassScale = 0;
    private float avgScale;

    void Start()
    {
        if (particle != null)
        {
            main = particle.main;
        }
    }

	void Update () {
		if (isActive && particle != null)
        {
            AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
            bassScale = Mathf.Pow(spectrum[0] * 0.8f + spectrum[1] * 0.2f,2f);
            for (int i = 0; i < 8; i++) avgScale += spectrum[i];
            avgScale = Mathf.Pow(avgScale/8f,0.25f);

            main.simulationSpeed = bassScale*0.7f + avgScale*0.3f + 0.05f;
        }
	}
}
