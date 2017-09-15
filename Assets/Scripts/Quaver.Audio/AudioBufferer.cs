// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Cache;

namespace Quaver.Audio
{
    public class AudioBufferer : MonoBehaviour
    {
        private WWW _audioLoader;
        private AudioSource _bufferAudio;
        private CachedBeatmap _bufferMap;
        private bool _bufferPreview;
        private float _bufferPlayDelay;

        public void init(CachedBeatmap map, AudioSource gameAudio, bool usePreviewTime = false, float playDelay = 0f)
        {
            _bufferAudio = gameAudio;
            _bufferMap = map;
            _bufferPreview = usePreviewTime;
            _bufferPlayDelay = playDelay;

            string url = "file:///" + map.AudioPath;
            _audioLoader = new WWW(url);
            StartCoroutine(BufferAudio());
        }

        private IEnumerator BufferAudio()
        {
            _bufferAudio.Stop();

            //Wait until audio is loaded
            yield return _audioLoader;
            yield return new WaitForSeconds(0.1f);
            //Set and play audio
            _bufferAudio.clip = _audioLoader.GetAudioClip(false, true, AudioType.OGGVORBIS);
            if (_bufferPreview) _bufferAudio.time = _bufferMap.AudioPreviewTime / 1000f;
            else _bufferAudio.time = 0;
            _bufferAudio.PlayDelayed(_bufferPlayDelay);

            //Removes game object
            Destroy(this.gameObject);
        }
    }
}
