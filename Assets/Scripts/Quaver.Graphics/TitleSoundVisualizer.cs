using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Quaver.Config;

public class TitleSoundVisualizer : MonoBehaviour {
    private float[] spectrum = new float[512];
    private float bassScale;
    private float logoScale = 3f;
    private float prevBassValue;
    private float cameraShake = 0;

    public GameObject[] emitters;
    public GameObject logo;
    public GameObject titleScreen;
    public AudioSource gameAudio;

	void Start () {

        // This is just a test for the songs folder, we'll choose a random beatmap later instead of just choosing a 
        // random song, but here's sample code for loading a .WAV
        string[] files = Directory.GetFiles(ConfigDefault.SongDirectory, "*.ogg", SearchOption.AllDirectories);

        if (files.Length == 0)
        {
            Debug.LogError("No .ogg files detected in folder!");
        }

        string url = "file:///" + files[Random.Range(0, files.Length)];
        gameAudio = GetComponent<AudioSource>();

        WWW audioLoader = new WWW(url);

                 while (!audioLoader.isDone)
         {
             Debug.Log("uploading");
         }

        gameAudio.clip = audioLoader.GetAudioClip(false, false, AudioType.OGGVORBIS);

        if (!gameAudio.isPlaying && gameAudio.clip.isReadyToPlay)
        {
             Debug.Log("playing");
             gameAudio.Play();
        }
	}

    void Update()
    {
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
        int i = 0;
        float tween = 0;
        for (i = 0; i < 40; i++)
        {
            //Debug.DrawLine(new Vector3((i - 1) / 20f, spectrum[i - 1] - 10, 1), new Vector3(i / 20f, spectrum[i] - 10, 1), Color.green);
            if (i <= 2) tween += spectrum[i];
        }
        bassScale += (tween - bassScale) / 2f;
        if (bassScale - prevBassValue > 0.15f) logoScale += (3f + Mathf.Min(200f * Mathf.Pow(bassScale - prevBassValue,3.5f),2f)-logoScale)/2f;
        //print(bassScale - prevBassValue);
        prevBassValue = bassScale;
        cameraShake += 0.01f;
        if (cameraShake > Mathf.PI*2f) cameraShake -= Mathf.PI*2f;
        this.transform.localPosition = new Vector3(Mathf.Sin(cameraShake) / 3f, Mathf.Cos(cameraShake) / 3f, 0);
        logo.transform.localScale = Vector3.one * logoScale;
        logoScale += (3f - logoScale) / 12f;

        for (i = 0; i < emitters.Length; i++)
        {
            emitters[i].transform.GetComponent<ParticleSystem>().emissionRate = bassScale * 20f + 2f;
            emitters[i].transform.GetComponent<ParticleSystem>().playbackSpeed = bassScale * 4f+0.1F;
        }

    }
}
