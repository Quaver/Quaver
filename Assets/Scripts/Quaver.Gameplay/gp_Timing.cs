using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Gameplay
{
    public partial class PlayScreen
    {
        //SV + Timing Point Variables
        private List<TimingObject> _svQueue, _timingQueue, _barQueue;
        private ulong _curSVPos;
        private int _curSVPart;
        private ulong[] _svCalc; //Stores SV position data for efficiency

        //Audio File Variables
        private bool _songDone;
        private float _songEndOffset;
        private float _actualSongTime;
        private float _curSongTime;

        //Initialize Timing Points/Bars
        private void time_init()
        {
            //Reference Variables
            int i = 0;
            _curSVPos = (ulong)(-config_playStartDelayed + 10000f); //10000ms added since curSVPos is a uLong
            _curSVPart = 0;

            //Declare Other Values
            _songAudio = Manager.SongAudioSource;
            _curSongTime = -config_playStartDelayed;
            _actualSongTime = -(float)config_playStartDelayed / 1000f;

            //Create Timing Points + SVs on a list
            TimingObject newTime;
            _svQueue = new List<TimingObject>();
            for (i = 0; i < _qFile.SliderVelocities.Count; i++)
            {
                newTime = new TimingObject();
                newTime.StartTime = _qFile.SliderVelocities[i].StartTime;
                newTime.Multiplier = _qFile.SliderVelocities[i].Multiplier;
                _svQueue.Add(newTime);
            }


            _timingQueue = new List<TimingObject>();
            for (i = 0; i < _qFile.TimingPoints.Count; i++)
            {
                newTime = new TimingObject();
                newTime.StartTime = _qFile.TimingPoints[i].StartTime;
                newTime.BPM = _qFile.TimingPoints[i].BPM;
                _timingQueue.Add(newTime);
            }

            //Create SVs
            if (!mod_noSV && _svQueue.Count > 1)
            {
                time_CreateSVs();
            }
            else
            {
                TimingObject newTp = new TimingObject();
                newTp.StartTime = 0;
                newTp.Multiplier = 1f;
                _svQueue.Add(newTp);
            }

            //Calculates SV for efficiency
            _svCalc = new ulong[_svQueue.Count];
            ulong svPosTime = 0;
            _svCalc[0] = 0;
            for (i = 0; i < _svQueue.Count; i++)
            {
                if (i + 1 < _svQueue.Count)
                {
                    ulong templ = (ulong)((_svQueue[i + 1].StartTime - _svQueue[i].StartTime) * _svQueue[i].Multiplier);
                    if (templ > 10000000) templ = 10000000;
                    svPosTime += templ;
                    _svCalc[i + 1] = svPosTime;
                }
                else break;
            }

            //Create Timing bars
            _barQueue = new List<TimingObject>();
            time_CreateBars();
        }

        //Creates timing bars (used to measure 16 beats)
        private void time_CreateBars()
        {
            int i = 0;
            if (!mod_split && _config_timingBars)
            {
                float curBarTime = 0;
                for (i = 0; i < _timingQueue.Count; i++)
                {
                    curBarTime = _timingQueue[i].StartTime;

                    if (_barQueue.Count > 0 && _barQueue[0].StartTime + 2 > curBarTime) _barQueue.RemoveAt(0);
                    curBarTime += 1000f * 4f * 60f / (_timingQueue[i].BPM);
                    TimingObject curTiming;

                    if (i + 1 < _timingQueue.Count)
                    {
                        while (curBarTime < _timingQueue[i + 1].StartTime)
                        {
                            curTiming = new TimingObject();
                            curTiming.StartTime = (int)(curBarTime);
                            _barQueue.Add(curTiming);
                            curBarTime += 1000f * 4f * 60f / (_timingQueue[i].BPM);
                        }
                    }
                    else
                    {
                        while (curBarTime < _songAudio.clip.length * 1000f)
                        {
                            curTiming = new TimingObject();
                            curTiming.StartTime = (int)(curBarTime);
                            _barQueue.Add(curTiming);
                            curBarTime += 1000f * 4f * 60f / (_timingQueue[i].BPM);
                        }
                    }
                }

                //Create starting bars
                List<TimingObject> tempBars = new List<TimingObject>();
                for (i = 0; i < _barQueue.Count; i++)
                {
                    TimingObject hoo = new TimingObject();
                    GameObject curBar = Instantiate(timingBar, hitContainer.transform);
                    curBar.transform.localScale = new Vector3(((float)(skin_columnSize + skin_bgMaskBufferSize + skin_noteBufferSpacing) / config_PixelUnitSize) * 4f * (config_PixelUnitSize / (float)curBar.transform.GetComponent<SpriteRenderer>().sprite.rect.size.x),
                        (config_PixelUnitSize / (float)curBar.transform.GetComponent<SpriteRenderer>().sprite.rect.size.y) * ((float)skin_timingBarPixelSize / config_PixelUnitSize)
                        , 1f);
                    curBar.transform.localPosition = new Vector3(0f, PosFromSV(_barQueue[i].StartTime), 2f);

                    hoo.StartTime = _barQueue[i].StartTime;
                    hoo.TimingBar = curBar;
                    tempBars.Add(hoo);
                }

                _barQueue = new List<TimingObject>(tempBars);
            }
        }

        //Creates SV Points
        private void time_CreateSVs()
        {
            //AverageBpm Reference Variables
            float longestBpmTime = 0;
            int avgBpmPos = 0;
            float averageBpm = 100;
            int i = 0; int j = 0;

            //Calculate Average BPM of map
            if (_timingQueue.Count > 1)
            {
                for (i=0; i<_timingQueue.Count;i++)
                {
                    if (i + 1 < _timingQueue.Count)
                    {
                        if (_timingQueue[i + 1].StartTime - _timingQueue[i].StartTime > longestBpmTime)
                        {
                            avgBpmPos = i;
                            longestBpmTime = _timingQueue[i + 1].StartTime - _timingQueue[i].StartTime;
                        }
                    }
                    else if (i + 1 == _timingQueue.Count)
                    {
                        if (_songAudio.clip.length*1000f - _timingQueue[i].StartTime > longestBpmTime)
                        {
                            avgBpmPos = i;
                            longestBpmTime = _songAudio.clip.length*1000f - _timingQueue[i].StartTime;
                        }
                    }
                }
                averageBpm = _timingQueue[avgBpmPos].BPM;
            }
            else averageBpm = _timingQueue[0].BPM;

            //Create and converts timing points to SV's
            int hij = 0;
            for (j = 0; j < _timingQueue.Count; j++)
            {
                
                if (_timingQueue[j].StartTime < _svQueue[0].StartTime - 0.01f)
                {
                    TimingObject newTp = new TimingObject();
                    newTp.StartTime = _timingQueue[j].StartTime;
                    if (_timingQueue[j].BPM == averageBpm) newTp.Multiplier = 1;
                    else newTp.Multiplier = _svQueue[0].Multiplier;
                    _svQueue.Insert(0, newTp);
                }
                else if (_timingQueue[j].StartTime > _svQueue[_svQueue.Count - 1].StartTime + 0.01f)
                {
                    TimingObject newTp = new TimingObject();
                    newTp.StartTime = _timingQueue[j].StartTime;
                    newTp.Multiplier = _svQueue[_svQueue.Count - 1].Multiplier;
                    _svQueue.Add(newTp);
                }
                else
                {
                    for (i = hij; i < _svQueue.Count; i++)
                    {
                        if (i + 1 < _svQueue.Count && _timingQueue[j].StartTime < _svQueue[i + 1].StartTime)
                        {
                            if (Mathf.Abs(_timingQueue[j].StartTime - _svQueue[i].StartTime) > 1f)
                            {
                                TimingObject newTp = new TimingObject();
                                newTp.StartTime = _timingQueue[j].StartTime;
                                newTp.Multiplier = 1f;
                                _svQueue.Add(newTp);
                                hij = i;
                            }
                            break;
                        }
                        //else break;
                    }
                }
            }

            //Normalizes SV's in between each BPM change interval
            hij = 0;
            _svQueue.Sort(delegate (TimingObject p1, TimingObject p2) { return p1.StartTime.CompareTo(p2.StartTime); });
            if (_timingQueue.Count >= 1)
            {
                hij = 0;

                for (j = 0; j < _svQueue.Count; j++)
                {
                    if (hij+1 < _timingQueue.Count)
                    {
                        if (_svQueue[j].StartTime < _timingQueue[hij + 1].StartTime - 0.01f)
                        {
                            TimingObject newSV = new TimingObject();
                            newSV.StartTime = _svQueue[j].StartTime;
                            newSV.Multiplier = Mathf.Min(_svQueue[j].Multiplier * _timingQueue[hij].BPM / averageBpm, 512f);
                            _svQueue.RemoveAt(j);
                            _svQueue.Insert(j, newSV);
                        }
                        else
                        {
                            hij++;
                            TimingObject newSV = new TimingObject();
                            newSV.StartTime = _svQueue[j].StartTime;
                            newSV.Multiplier = Mathf.Min(_svQueue[j].Multiplier * _timingQueue[hij].BPM / averageBpm, 512f);
                            _svQueue.RemoveAt(j);
                            _svQueue.Insert(j, newSV);
                        }
                    }
                    else
                    {
                        TimingObject newSV = new TimingObject();
                        newSV.StartTime = _svQueue[j].StartTime;
                        newSV.Multiplier = Mathf.Min(_svQueue[j].Multiplier * _timingQueue[hij].BPM / averageBpm, 512f);
                        _svQueue.RemoveAt(j);
                        _svQueue.Insert(j, newSV);
                    }
                }


            }
        }

        //Move Timing Bars
        private void time_MoveTimingBars()
        {
            if (_config_timingBars && !mod_split)
            {
                if (_barQueue.Count > 0 && _curSongTime > _barQueue[0].StartTime + missTime)
                {
                    Destroy(_barQueue[0].TimingBar);
                    _barQueue.RemoveAt(0);
                }
                else
                {
                    for (int k = 0; k < Mathf.Min(_barQueue.Count, maxNoteCount); k++)
                        _barQueue[k].TimingBar.transform.localPosition = new Vector3(0f, PosFromSV(_barQueue[k].StartTime), 2f);
                }
            }
        }

        //Calculate CurrentTime's Sv Position
        private void time_GetCurrentSVPos()
        {

            if (_curSongTime >= _svQueue[_svQueue.Count - 1].StartTime)
            {
                _curSVPart = _svQueue.Count - 1;
            }
            else if (_curSVPart < _svQueue.Count - 2)
            {
                for (int j = _curSVPart; j < _svQueue.Count - 1; j++)
                {
                    if (_curSongTime > _svQueue[_curSVPart + 1].StartTime) _curSVPart++;
                    else break;
                }
            }
            _curSVPos = _svCalc[_curSVPart] + (ulong)((float)((_curSongTime) - (_svQueue[_curSVPart].StartTime)) * _svQueue[_curSVPart].Multiplier + 10000);

        }

        //Set the position of the current play time
        private void time_SetCurrentSongTime()
        {
            //Calculate Time after Song Done
            if (!_songDone && !_songAudio.isPlaying && _songAudio.time < _songAudio.clip.length) _songAudio.UnPause();
            else if (_songDone) _songEndOffset += Time.deltaTime;

            //Calculate Actual Song Time
            if (_songDone)
            {
                _actualSongTime = _songAudio.clip.length + _songEndOffset;
            }
            else
            {
                if (_actualSongTime < 0) _actualSongTime += Time.deltaTime;
                else _actualSongTime = ((_songAudio.time) + (_actualSongTime + Time.deltaTime)) / 2f;
            }
            _curSongTime = _actualSongTime * 1000f - _config_offset;
        }

    }
}