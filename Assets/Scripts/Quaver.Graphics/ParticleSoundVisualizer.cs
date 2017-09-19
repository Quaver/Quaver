using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSoundVisualizer : MonoBehaviour
{
    //Assign these manually
    public bool isActive = true;
    public ParticleSystem particle;
    private ParticleSystem.MainModule _main;

    //Reference Variables
    private float[] _spectrum = new float[256];
    private float _bassScale = 0;
    private float _avgScale;

    private void Start()
    {
        if (particle != null)
        {
            _main = particle.main;
        }
    }

    private void Update()
    {
        if (isActive && particle != null)
        {
            AudioListener.GetSpectrumData(_spectrum, 0, FFTWindow.Rectangular);
            _bassScale = Mathf.Pow(_spectrum[0] * 0.8f + _spectrum[1] * 0.2f, 2f);
            for (int i = 0; i < 8; i++) _avgScale += _spectrum[i];
            _avgScale = Mathf.Pow(_avgScale / 8f, 0.25f);

            _main.simulationSpeed = _bassScale * 0.7f + _avgScale * 0.3f + 0.05f;
        }
    }
}
