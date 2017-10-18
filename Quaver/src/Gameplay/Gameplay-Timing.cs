using System;
using System.Collections.Generic;

namespace Quaver.Gameplay
{
    internal partial class StatePlayScreen
    {
        //Config Variables
        private double _gameAudioOffset = 0;

        //Audio Variables
        private double _gameAudioPosition;
        private double _gameAudioLength;
        private double _songEndOffset;
        private bool _songIsPlaying;

        //Gameplay Variables
        private const int PlayStartDelayed = 500;

        private double _actualSongTime;
        private double _currentSongTime;
        private int _curSVPart;
        private List<TimingObject> _svQueue;
        private List<TimingObject> _timingQueue;

        //SV + Timing Point Variables
        //private List<TimingObject> _svQueue, _timingQueue, _barQueue, _activeBars;
        //private GameObject[] _activeBarObjects;
        private ulong _curSVPos;

        //Audio File Variables
        private bool _songIsDone;

        private ulong[] _svCalc; //Stores SV position data for efficiency
        private float _averageBpm = 100;

        /// <summary>
        ///     Initialize Timing Contents.
        /// </summary>
        internal void InitializeTiming()
        {
            //TODO: Timing Initializer
            _gameAudioLength = _gameAudio.GetAudioLength();
            _gameAudioPosition = 0;
            _songEndOffset = 0;
            _songIsPlaying = false;

            //Reference Variables
            var i = 0;
            _curSVPos = (ulong) (-PlayStartDelayed + 10000f); //10000ms added since curSVPos is a long
            _curSVPart = 0;

            //Declare Other Values
            _currentSongTime = -PlayStartDelayed;
            _actualSongTime = -PlayStartDelayed;
            //_activeBarObjects = new GameObject[maxNoteCount];

            //Create Timing Points + SVs on a list

            _svQueue = new List<TimingObject>();
            
            for (i = 0; i < _qua.SliderVelocities.Count; i++)
            {
                TimingObject newSV = new TimingObject
                {
                    TargetTime = _qua.SliderVelocities[i].StartTime,
                    SvMultiplier = _qua.SliderVelocities[i].Multiplier
                };
                _svQueue.Add(newSV);
            }

            _timingQueue = new List<TimingObject>();
            for (i = 0; i < _qua.TimingPoints.Count; i++)
            {
                TimingObject newTO = new TimingObject
                {
                    TargetTime = _qua.TimingPoints[i].StartTime,
                    BPM = _qua.TimingPoints[i].Bpm
                };
                _timingQueue.Add(newTO);
            }

            //Calculate Average BPM
            CalculateAverageBpm();

            //Create SVs
            if (!_mod_noSV && _svQueue.Count > 1)
            {
                ConvertTPtoSV();
                NormalizeSVs();
            }
            //If there's no SV, create a single SV Point
            else
            {
                TimingObject newTp = new TimingObject
                {
                    TargetTime = 0,
                    SvMultiplier = 1f
                };
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
                    svPosTime += (ulong)((_svQueue[i + 1].TargetTime - _svQueue[i].TargetTime) * _svQueue[i].SvMultiplier);
                    _svCalc[i + 1] = svPosTime;
                }
                else break;
            }

            Console.WriteLine($@"SV CALC: {_svCalc[0]}, {_svCalc[1]}, {_svCalc[2]}, {_svCalc[3]}");

            //Create Timing bars
            //_barQueue = new List<TimingObject>();
            //time_CreateBars();
        }

        //Set the position of the current play time
        private void SetCurrentSongTime(double dt)
        {
            //Get GameAudio Position
            _gameAudioPosition = _gameAudio.GetAudioPosition();

            //Calculate Time after Song Done
            if (_songIsDone)
            {
                _songEndOffset += dt;
                _actualSongTime = _gameAudioLength + _songEndOffset;
            }
            //Calculate Actual Song Time
            else
            {
                if (_actualSongTime < 0) _actualSongTime += dt;
                else
                {
                    if (!_songIsPlaying)
                    {
                        _songIsPlaying = true;
                        _gameAudio.Play();
                    }
                    _actualSongTime = (_gameAudioPosition + (_actualSongTime + dt)) / 2f;
                }
            }
            _currentSongTime = _actualSongTime - _gameAudioOffset;

            //Get SV Position
            GetCurrentSvPosition();
            //Console.WriteLine($"SONGTIME: {_currentSongTime}");
        }

        private void time_DestroyBars()
        {
            //for (int i = 0; i < _activeBars.Count; i++) Destroy(_activeBars[i].TimingBar);
        }

        //Creates timing bars (used to measure 16 beats)
        private void time_CreateBars()
        {
            /*
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

                //Create all bars in music
                List<TimingObject> tempBars = new List<TimingObject>();
                for (i = 0; i < _barQueue.Count; i++)
                {
                    TimingObject hoo = new TimingObject();

                    hoo.StartTime = _barQueue[i].StartTime;
                    tempBars.Add(hoo);
                }
                _barQueue = new List<TimingObject>(tempBars);

                //Create starting bars
                _activeBars = new List<TimingObject>();
                for (i = 0; i < maxNoteCount; i++)
                {
                    if (_barQueue.Count > 0) _activeBarObjects[i] = time_InstantiateBar(null);
                    else break;
                }

            }
            */
        }

        //Creates a bar object
        /*
        private GameObject time_InstantiateBar(GameObject curBar)
        {

            if (curBar == null)
            {
                curBar = Instantiate(timingBar, hitContainer.transform);
                curBar.transform.localScale = new Vector3(((float)(skin_columnSize + skin_bgMaskBufferSize + skin_noteBufferSpacing) / config_PixelUnitSize) * 4f * (config_PixelUnitSize / (float)curBar.transform.GetComponent<SpriteRenderer>().sprite.rect.size.x),
                    (config_PixelUnitSize / (float)curBar.transform.GetComponent<SpriteRenderer>().sprite.rect.size.y) * ((float)skin_timingBarPixelSize / config_PixelUnitSize)
                    , 1f);
            }
            TimingObject newTimeOb = new TimingObject();
            newTimeOb.StartTime = _barQueue[0].StartTime;
            newTimeOb.TimingBar = curBar;

            curBar.transform.localPosition = new Vector3(0f, PosFromSV(newTimeOb.StartTime), 2f);

            _activeBars.Add(newTimeOb);
            _barQueue.RemoveAt(0);
            return curBar;
        }
        */

        //Recycles bar objects from object pool or destroys them
        /*
        private void time_RemoveBar(GameObject curBar)
        {
            if (_barQueue.Count > 0) time_InstantiateBar(curBar);
            else Destroy(curBar);
        }*/

        //Creates SV Points
        private void ConvertTPtoSV()
        {
            //Create and converts timing points to SV's
            for (int j = 0; j < _timingQueue.Count; j++)
            {
                if (_timingQueue[j].TargetTime < _svQueue[0].TargetTime - 0.01f)
                {
                    TimingObject newTp = new TimingObject();
                    newTp.TargetTime = _timingQueue[j].TargetTime;
                    if (_timingQueue[j].BPM == _averageBpm) newTp.SvMultiplier = 1;
                    else newTp.SvMultiplier = _svQueue[0].SvMultiplier;
                    _svQueue.Insert(0, newTp);
                }
                else if (_timingQueue[j].TargetTime > _svQueue[_svQueue.Count - 1].TargetTime + 0.01f)
                {
                    TimingObject newTp = new TimingObject();
                    newTp.TargetTime = _timingQueue[j].TargetTime;
                    newTp.SvMultiplier = _svQueue[_svQueue.Count - 1].SvMultiplier;
                    _svQueue.Add(newTp);
                }
                else
                {
                    int lastIndex = 0;
                    for (int i = lastIndex; i < _svQueue.Count; i++)
                    {
                        if (i + 1 < _svQueue.Count && _timingQueue[j].TargetTime < _svQueue[i + 1].TargetTime)
                        {
                            if (Math.Abs(_timingQueue[j].TargetTime - _svQueue[i].TargetTime) > 1f)
                            {
                                TimingObject newTp = new TimingObject();
                                newTp.TargetTime = _timingQueue[j].TargetTime;
                                newTp.SvMultiplier = 1f;
                                _svQueue.Add(newTp);
                                lastIndex = i;
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculate Average BPM of map
        /// </summary>
        private void CalculateAverageBpm()
        {
            //TODO: Make this calculate consistancy based average bpm instead of timing point that is longest
            //AverageBpm Reference Variables
            double longestBpmTime = 0;
            var avergeBpmIndex = 0;
            var i = 0;

            //Calculate Average BPM
            if (_timingQueue.Count > 1)
            {
                for (i = 0; i < _timingQueue.Count; i++)
                {
                    if (i + 1 < _timingQueue.Count)
                    {
                        if (_timingQueue[i + 1].TargetTime - _timingQueue[i].TargetTime > longestBpmTime)
                        {
                            avergeBpmIndex = i;
                            longestBpmTime = _timingQueue[i + 1].TargetTime - _timingQueue[i].TargetTime;
                        }
                    }
                    else if (i + 1 == _timingQueue.Count)
                    {
                        if (_gameAudioLength - _timingQueue[i].TargetTime > longestBpmTime)
                        {
                            avergeBpmIndex = i;
                            longestBpmTime = _gameAudioLength - _timingQueue[i].TargetTime;
                        }
                    }
                }
                _averageBpm = _timingQueue[avergeBpmIndex].BPM;
            }
            else _averageBpm = _timingQueue[0].BPM;
        }

        //Normalizes SV's in between each BPM change interval
        private void NormalizeSVs()
        {
            //Reference Variables + Sort
            var i = 0;
            var lastIndex = 0;
            _svQueue.Sort(delegate (TimingObject p1, TimingObject p2) { return p1.TargetTime.CompareTo(p2.TargetTime); });

            //Normalize
            if (_timingQueue.Count >= 1)
            {
                for (i = 0; i < _svQueue.Count; i++)
                {
                    if (lastIndex + 1 < _timingQueue.Count)
                    {
                        if (_svQueue[i].TargetTime < _timingQueue[lastIndex + 1].TargetTime - 0.01f)
                        {
                            _svQueue[i].TargetTime = _svQueue[i].TargetTime;
                            _svQueue[i].SvMultiplier = Math.Min(_svQueue[i].SvMultiplier * _timingQueue[lastIndex].BPM / _averageBpm, 64f);
                        }
                        else
                        {
                            lastIndex++;
                            _svQueue[i].TargetTime = _svQueue[i].TargetTime;
                            _svQueue[i].SvMultiplier = Math.Min(_svQueue[i].SvMultiplier * _timingQueue[lastIndex].BPM / _averageBpm, 64f);
                        }
                    }
                    else
                    {
                        _svQueue[i].TargetTime = _svQueue[i].TargetTime;
                        _svQueue[i].SvMultiplier = Math.Min(_svQueue[i].SvMultiplier * _timingQueue[lastIndex].BPM / _averageBpm, 512f);
                    }
                }
            }
        }

        //Move Timing Bars
        private void MoveTimingBars()
        {
            /*
            if (_config_timingBars && !mod_split)
            {
                if (_activeBars.Count > 0 && _CurrentSongTime > _activeBars[0].StartTime + missTime)
                {
                    time_RemoveBar(_activeBars[0].TimingBar);
                    _activeBars.RemoveAt(0);
                }
                else
                {
                    for (int k = 0; k < _activeBars.Count; k++)
                    {
                        _activeBars[k].TimingBar.transform.localPosition = new Vector3(0f, PosFromSV(_activeBars[k].StartTime), 2f);
                    }
                }
            }
            */
        }

        //Calculate CurrentTime's Sv Position
        private void GetCurrentSvPosition()
        {
            if (_currentSongTime >= _svQueue[_svQueue.Count - 1].TargetTime)
            {
                _curSVPart = _svQueue.Count - 1;
            }
            else if (_curSVPart < _svQueue.Count - 2)
            {
                for (int j = _curSVPart; j < _svQueue.Count - 1; j++)
                {
                    if (_currentSongTime > _svQueue[_curSVPart + 1].TargetTime) _curSVPart++;
                    else break;
                }
            }
            _curSVPos = _svCalc[_curSVPart] + (ulong)(((float)((_currentSongTime) - (_svQueue[_curSVPart].TargetTime)) * _svQueue[_curSVPart].SvMultiplier) + 10000);
        }
    }
}