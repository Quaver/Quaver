using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Cache;

namespace Quaver.Audio
{


    public class AudioBufferer : MonoBehaviour
    {

        private WWW audioLoader;
        private AudioSource bufferAudio;
        private CachedBeatmap bufferMap;
        private bool bufferPreview;
        private float bufferPlayDelay;

        public void init(CachedBeatmap map, AudioSource gameAudio, bool usePreviewTime = false, float playDelay = 0f)
        {
            bufferAudio = gameAudio;
            bufferMap = map;
            bufferPreview = usePreviewTime;
            bufferPlayDelay = playDelay;

            string url = "file:///" + map.AudioPath;
            audioLoader = new WWW(url);
            StartCoroutine(BufferAudio());
        }

        private IEnumerator BufferAudio()
        {

            bufferAudio.Stop();

            //Wait until audio is loaded
            yield return audioLoader;
            yield return new WaitForSeconds(0.1f);
            //Set and play audio
            bufferAudio.clip = audioLoader.GetAudioClip(false,true, AudioType.OGGVORBIS);
            if (bufferPreview) bufferAudio.time = bufferMap.AudioPreviewTime / 1000f;
            else bufferAudio.time = 0;
            bufferAudio.PlayDelayed(bufferPlayDelay);

            //Removes game object
            Destroy(this.gameObject);
        }
    }
}
