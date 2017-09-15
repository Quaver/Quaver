// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Qua;
using Quaver.Graphics;
using Quaver.Main;
using Quaver.Audio;

namespace Quaver.Gameplay
{
    public partial class PlayScreen : GameState
    {
        /*Classes/Gameobjects (public variables will be changed later)*/
        public GameObject hitObjectTest;
        public GameObject receptorBar;
        public GameObject lightingBar;
        public GameObject particleContainer;
        public GameObject hitContainer;
        public GameObject bgMask;
        private GameObject[] _receptors;
        private GameObject[] _hitLighting;

        /*GSM REFERENCES (Anything that references the game state manager)*/
        private QuaFile _qFile;
        private AudioSource _songAudio;

        /*GAME MODS*/
        private const bool mod_noSV = false;
        private const bool mod_pull = false;
        private const bool mod_split = false;
        private const bool mod_spin = false;
        private const bool mod_shuffle = false;

        public void Start()
        {
            //Check if beatmap is valid
            _qFile = QuaParser.Parse(Manager.currentMap.Path);
            if (!_qFile.IsValidQua)
            {
                print("IS NOT VALID QUA FILE");
            }
            else if (_qFile.IsValidQua)
            {

                //Declare Config Variables
                _config_scrollSpeed = Manager.GameConfig.ScrollSpeed;
                _config_timingBars = true;//should be config
                _config_upScroll = !Manager.GameConfig.DownScroll;
                _config_KeyBindings = new KeyCode[] { Manager.GameConfig.KeyLaneMania1, Manager.GameConfig.KeyLaneMania2, Manager.GameConfig.KeyLaneMania3, Manager.GameConfig.KeyLaneMania4 };
                _config_offset = Manager.GameConfig.GlobalOffset;

                //Initializes skin
                skin_init();

                //Initializes Timing
                time_init();

                //Starts rendering notes + sets gameplay variables
                np_init();

                //Plays the song, but delayed
                AudioPlayer.LoadSong(Manager.currentMap, Manager.SongAudioSource, false, (float)config_playStartDelayed / 1000f);
                loaded = true;
            }
        }

        public void Update()
        {
            if (_qFile.IsValidQua && isActive)
            {
                //Check what to do after unpaused
                if (_songAudio.time >= _songAudio.clip.length && !_songDone) _songDone = true;

                //Song Time Calculation (ms)
                time_SetCurrentSongTime();

                //Calculates curSV Position
                time_GetCurrentSVPos();

                //Receptor Modifiers
                if (mod_spin || mod_shuffle) ModifyReceptors();

                //Move TimingBars
                time_MoveTimingBars();

                //Move notes and check if miss
                np_MovePlayNotes();

                //LN Check
                np_ResizeLongNotes();

                //GhostNote Check (darkened keys)
                np_MoveGhostNotes();

                //Key Press Check
                input_CheckInput();
            }
            else
            {
                if (_songAudio.isPlaying) _songAudio.Pause();
            }
        }


        // Modifies the receptors if visual mods are on.
        private void ModifyReceptors()
        {
            for (int k = 0; k < 4; k++)
            {
                //Spin Receptors
                if (mod_spin) _skin_receptorRotations[k] += 0.5f;
                if (_skin_receptorRotations[k] >= 360) _skin_receptorRotations[k] -= 360;

                //XOff Receptors
                if (mod_shuffle)
                {
                    if (k == 0 || k == 2) _receptorXOffset[k] = (Mathf.Pow(Mathf.Sin(_curSongTime * (Mathf.PI / 180f) / 10f), 3) + 1f) * skin_columnSize / config_PixelUnitSize / 2f;
                    else _receptorXOffset[k] = -(Mathf.Pow(Mathf.Sin(_curSongTime * (Mathf.PI / 180f) / 10f), 3) + 1f) * skin_columnSize / config_PixelUnitSize / 2f;
                }

                //Set Receptor Transform
                if (mod_spin) _receptors[k].transform.localEulerAngles = new Vector3(0, 0, _skin_receptorRotations[k]);
                if (mod_shuffle) _receptors[k].transform.localPosition = new Vector3(_receptorXPos[k] + _receptorXOffset[k], _receptorYPos, 1);
                _receptors[k].transform.localScale = Vector3.one * (_receptors[k].transform.localScale.x + ((skin_columnSize / config_PixelUnitSize) - _receptors[k].transform.localScale.x) / 3f);
            }
        }

        //Sets the GameObject position of the note
        private void SetNotePos(NoteObject curNote, int lnMode)
        {
            float StartTime = curNote.StartTime;
            if (lnMode == 2) StartTime = _curSongTime;
            float splitFactor = 1f;
            if (mod_split && curNote.KeyLane >= 3) splitFactor = -1f;
            if (lnMode != 2 || mod_shuffle)
                curNote.HitSet.transform.localPosition = new Vector3(_receptorXPos[curNote.KeyLane - 1] + _receptorXOffset[curNote.KeyLane - 1], splitFactor * PosFromSV(StartTime), 0);
            else curNote.HitSet.transform.localPosition = new Vector3(_receptorXPos[curNote.KeyLane - 1] + _receptorXOffset[curNote.KeyLane - 1], _receptorYPos * splitFactor, 0);
            if (mod_spin) curNote.HitSprite.transform.eulerAngles = new Vector3(0, 0, _skin_receptorRotations[curNote.KeyLane - 1]);
            if ((lnMode != 1 || mod_pull) && lnMode != 3 && curNote.EndTime > 0 && curNote.EndTime > StartTime)
            {
                float lnSize = splitFactor * Mathf.Min(Mathf.Abs(PosFromSV(curNote.EndTime) - PosFromSV(StartTime)), 50f);

                curNote.SliderMiddleSprite.size = new Vector2(1f, -_scrollNegativeFactor * lnSize * (config_PixelUnitSize / skin_columnSize));
                curNote.SliderEndObject.transform.localPosition = new Vector3(0f, _scrollNegativeFactor * lnSize, 0.1f);

                if (lnMode == 0)
                {
                    curNote.SliderMiddleObject.transform.localScale = Vector3.one * (skin_columnSize / config_PixelUnitSize);
                    curNote.SliderMiddleSprite.color = new Color(1f, 1f, 1f, 1f);
                    curNote.SliderMiddleObject.SetActive(true);

                    curNote.SliderEndSprite.color = new Color(1f, 1f, 1f, 1f);
                    curNote.SliderEndObject.transform.localScale = Vector3.one * (skin_columnSize / config_PixelUnitSize);
                    curNote.SliderEndObject.SetActive(true);
                    if (splitFactor >= 1) curNote.SliderEndSprite.flipY = !_config_upScroll;
                    else curNote.SliderEndSprite.flipY = _config_upScroll;
                }
            }
        }

        //Calculates the position from SV
        private float PosFromSV(float timePos)
        {
            float returnVal;
            if (Mathf.Abs(timePos - _curSongTime) >= 0.01)
            {
                if (!mod_noSV)
                {
                    ulong svPosTime = 0;
                    int curPos = 0;
                    if (timePos >= _svQueue[_svQueue.Count - 1].StartTime)
                    {
                        curPos = _svQueue.Count - 1;
                    }
                    else
                    {
                        for (int i = 0; i < _svQueue.Count - 1; i++)
                        {
                            if (timePos < _svQueue[i + 1].StartTime)
                            {
                                curPos = i;
                                break;
                            }
                        }
                    }
                    svPosTime = _svCalc[curPos] + (ulong)(15000 + (timePos - _svQueue[curPos].StartTime) * _svQueue[curPos].Multiplier);
                    //10000ms added for negative, since svPos is a ulong

                    returnVal = (float)(svPosTime - _curSVPos - 5000f) / 1000f * (float)_config_scrollSpeed * (1 / _songAudio.pitch);
                }
                else returnVal = (timePos - _curSongTime) / 1000f * (float)_config_scrollSpeed * (1 / _songAudio.pitch);
            }
            else returnVal = 0;

            if (mod_pull) returnVal = 2f * Mathf.Max(Mathf.Pow(returnVal, 0.6f), 0) + Mathf.Min(timePos - _curSongTime, 0f) / 1000f * (float)_config_scrollSpeed * (1 / _songAudio.pitch);

            return returnVal * _scrollNegativeFactor + _receptorYPos;
        }
    }

    //Dont rmeove below this
}