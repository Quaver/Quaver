using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Cache;

namespace Quaver.Audio
{


    public class AudioBufferer : MonoBehaviour
    {

        private WWW audioLoader;
        private GameObject obj;
        private AudioSource bufferAudio;
        private CachedBeatmap bufferMap;
        private bool bufferPreview;

        public AudioBufferer(CachedBeatmap map, AudioSource gameAudio, bool usePreviewTime = false)
        {

            bufferAudio = gameAudio;
            bufferMap = map;
            bufferPreview = usePreviewTime;

            string url = "file:///" + map.AudioPath;
            audioLoader = new WWW(url);
            obj = new GameObject();
            obj.AddComponent<AudioBufferer>().StartCoroutine(BufferAudio());
        }

        private IEnumerator BufferAudio()
        {

            bufferAudio.Stop();

            //Wait until audio is loaded
            while (!audioLoader.isDone) yield return null;
            yield return new WaitForSeconds(0.01f);

            //Set and play audio
            bufferAudio.clip = audioLoader.GetAudioClip(false,true, AudioType.OGGVORBIS);
            if (bufferPreview) bufferAudio.time = bufferMap.AudioPreviewTime / 1000f;
            bufferAudio.Play();

            //Removes game object
            Debug.Log("[AUDIO BUFFER] DONE!");
            Destroy(obj);
        }
    }
}
