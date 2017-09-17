
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Quaver.Config;

public class TitleSoundVisualizer : MonoBehaviour
{
    private float[] _spectrum = new float[512];
    private float _bassScale;
    private float _logoScale = 3f;
    private float _prevBassValue;
    private float _cameraShake = 0;

    public GameObject[] emitters;
    public GameObject logo;
    public GameObject titleScreen;
    public AudioSource gameAudio;

    private void Start()
    {
    }

    private void Update()
    {
        AudioListener.GetSpectrumData(_spectrum, 0, FFTWindow.Rectangular);
        int i = 0;
        float tween = 0;
        for (i = 0; i < 40; i++)
        {
            //Debug.DrawLine(new Vector3((i - 1) / 20f, spectrum[i - 1] - 10, 1), new Vector3(i / 20f, spectrum[i] - 10, 1), Color.green);
            if (i <= 2) tween += _spectrum[i];
        }
        _bassScale += (tween - _bassScale) / 2f;
        if (_bassScale - _prevBassValue > 0.15f) _logoScale += (3f + Mathf.Min(200f * Mathf.Pow(_bassScale - _prevBassValue, 3.5f), 2f) - _logoScale) / 2f;
        //print(bassScale - prevBassValue);
        _prevBassValue = _bassScale;
        _cameraShake += 0.01f;
        if (_cameraShake > Mathf.PI * 2f) _cameraShake -= Mathf.PI * 2f;
        this.transform.localPosition = new Vector3(Mathf.Sin(_cameraShake) / 3f, Mathf.Cos(_cameraShake) / 3f, 0);
        logo.transform.localScale = Vector3.one * _logoScale;
        _logoScale += (3f - _logoScale) / 12f;

        for (i = 0; i < emitters.Length; i++)
        {
            emitters[i].transform.GetComponent<ParticleSystem>().emissionRate = _bassScale * 20f + 2f;
            emitters[i].transform.GetComponent<ParticleSystem>().playbackSpeed = _bassScale * 4f + 0.1F;
        }
    }
}
